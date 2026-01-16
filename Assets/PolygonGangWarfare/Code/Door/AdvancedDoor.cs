using UnityEngine;

public class AdvancedDoor: MonoBehaviour
{
    [Header("Налаштування")]
    [SerializeField] private float maxAngle = 90f;
    [SerializeField] private float smoothSpeed = 4f;
    [SerializeField] private bool invertRotation = false;

    [Header("UI")]
    [SerializeField] private GameObject interactionUI; // Твій Canvas

    [Header("Бот")]
    [SerializeField] private string botTag = "Bot";

    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private bool isDragging = false;

    // Таймер для зникання кнопки
    private float lastLookTime = -1f;

    void Start()
    {
        if (interactionUI != null) interactionUI.SetActive(false);
    }

    void Update()
    {
        // --- ЛОГІКА UI (АВТОМАТИЧНЕ ЗНИКНЕННЯ) ---
        if (interactionUI != null)
        {
            // Якщо на двері подивилися менше ніж 0.1 сек тому -> показати
            // Якщо минуло більше часу (гравець відвів погляд) -> сховати
            bool shouldShow = (Time.time - lastLookTime < 0.1f) && !isDragging;

            if (interactionUI.activeSelf != shouldShow)
            {
                interactionUI.SetActive(shouldShow);
            }
        }

        // --- ФІЗИКА ДВЕРЕЙ ---
        if (!isDragging)
        {
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);
            transform.localRotation = Quaternion.Euler(0, currentAngle, 0);
        }
    }

    // --- Цей метод викликає скрипт Гравця щокадру, коли наводиться ---
    public void ShowPrompt()
    {
        lastLookTime = Time.time;
    }

    // --- МЕТОДИ ВЗАЄМОДІЇ ---
    public void ToggleDoor(Vector3 playerPosition)
    {
        if (Mathf.Abs(targetAngle) > 1f) targetAngle = 0f;
        else
        {
            Vector3 directionToPlayer = playerPosition - transform.position;
            float dot = Vector3.Dot(transform.forward, directionToPlayer.normalized);
            float angle = (dot > 0) ? maxAngle : -maxAngle;
            if (invertRotation) angle = -angle;
            targetAngle = angle;
        }
    }

    public void BeginDrag() => isDragging = true;

    public void OnDrag(float mouseDelta)
    {
        float multiplier = invertRotation ? -1.5f : 1.5f;
        currentAngle -= mouseDelta * multiplier;
        currentAngle = Mathf.Clamp(currentAngle, -maxAngle, maxAngle);
        transform.localRotation = Quaternion.Euler(0, currentAngle, 0);
        targetAngle = currentAngle;
    }

    public void EndDrag()
    {
        isDragging = false;
        if (Mathf.Abs(currentAngle) < 10f) targetAngle = 0f;
    }

    // --- ТРИГЕРИ (ЛИШЕ ДЛЯ БОТА) ---
    // Працює, навіть якщо тригер на дочірньому об'єкті (якщо там нема Rigidbody)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(botTag) && !isDragging)
        {
            Vector3 directionToBot = other.transform.position - transform.position;
            float dot = Vector3.Dot(transform.forward, directionToBot.normalized);
            float angle = (dot > 0) ? -maxAngle : maxAngle;
            if (invertRotation) angle = -angle;
            targetAngle = angle;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(botTag) && !isDragging) targetAngle = 0f;
    }
}
