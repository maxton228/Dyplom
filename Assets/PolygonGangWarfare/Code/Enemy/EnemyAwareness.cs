using UnityEngine;
using System;
using System.Collections.Generic;
public class EnemyAwareness : MonoBehaviour
{
    [Header("Параметри")]
    public float detectionSpeed = 1.5f;
    public float decaySpeed = 0.5f;

    [Header("Стан")]
    [Range(0, 1)] public float currentAwareness = 0f;
    public bool IsAlerted { get; private set; }
    [Header("Поріг миттєвої реакції")]
    public float instantDetectionRange = 2.5f;
    public event Action<float> OnAwarenessChanged;
    public event Action OnAlerted;
    public static List<EnemyAwareness> AllEnemies = new List<EnemyAwareness>();
    public void ProcessVision(float visibilityFactor, float distance, float maxRange, float angleToPlayer)
    {
        if (IsAlerted) return;

        if (visibilityFactor > 0.05f) // Зменшив поріг, щоб ворог був уважнішим
        {
            // 1. Дистанція: використовуємо квадрат відстані (експоненціально)
            // Чим ближче гравець, тим швидше зростає підозра
            float distRatio = Mathf.Clamp01(distance / maxRange);
            float distanceWeight = Mathf.Lerp(2.0f, 0.1f, distRatio * distRatio);

            // 2. Периферійний зір: у центрі погляду (0 градусів) помічають швидше
            // Використовуємо косинус кута для плавного згасання уваги до країв FOV
            float angleWeight = Mathf.Cos(angleToPlayer * Mathf.Deg2Rad);
            angleWeight = Mathf.Max(0.2f, angleWeight); // Мінімальна увага навіть боком

            // 3. Бонус за близькість: якщо в упор - множимо швидкість
            if (distance < instantDetectionRange)
            {
                distanceWeight *= 4f;
            }

            // Розрахунок приросту
            float increase = detectionSpeed * visibilityFactor * distanceWeight * angleWeight * Time.deltaTime;
            currentAwareness += increase;
        }
        else
        {
            // Якщо гравець сховався, шкала падає не миттєво (ефект "що це було?")
            currentAwareness -= decaySpeed * Time.deltaTime;
        }

        CheckLimits();
    }

    public void AddSuspicion(float amount)
    {
        if (IsAlerted) return;
        currentAwareness += amount;
        CheckLimits();
    }

    public void TriggerInstantAlert()
    {
        if (IsAlerted) return;

        currentAwareness = 1f;
        CheckLimits();

        AlertAllEnemies();
    }

    private void CheckLimits()
    {
        currentAwareness = Mathf.Clamp01(currentAwareness);
        OnAwarenessChanged?.Invoke(currentAwareness);

        if (currentAwareness >= 1f && !IsAlerted)
        {
            IsAlerted = true;
            OnAlerted?.Invoke();
        }
    }
    void OnEnable()
    {
        if (!AllEnemies.Contains(this)) AllEnemies.Add(this);
    }

    void OnDisable()
    {
        AllEnemies.Remove(this);
    }

    private static void AlertAllEnemies()
    {
        for (int i = AllEnemies.Count - 1; i >= 0; i--)
        {
            if (AllEnemies[i] != null && !AllEnemies[i].IsAlerted)
            {
                AllEnemies[i].TriggerInstantAlert();
            }
        }
    }
}
