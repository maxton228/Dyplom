using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class MinigameController : MonoBehaviour
{
    public static MinigameController Instance { get; private set; }

    [Header("Об'єкти мінігри")]
    public GameObject lockpickingPrefab;

    // ВАЖЛИВО: Тип змінений з MonoBehaviour на Lockpicking!
    public Lockpicking lockpickingScript;

    public GameObject blurVolume;

    [Header("Події")]
    public UnityEvent OnMinigameStart;
    public UnityEvent OnMinigameEnd;

    private AdvancedDoor currentDoor;
    private bool isMinigameActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (isMinigameActive && Input.GetKeyDown(KeyCode.Escape))
        {
            StopLockpicking(false);
        }
    }

    public void StartLockpicking(AdvancedDoor door)
    {
        if (isMinigameActive) return;

        currentDoor = door;
        isMinigameActive = true;

        if (lockpickingPrefab != null) lockpickingPrefab.SetActive(true);

        if (lockpickingScript == null && lockpickingPrefab != null)
        {
            lockpickingScript = lockpickingPrefab.GetComponentInChildren<Lockpicking>(true);
        }

        if (lockpickingScript != null)
        {
            lockpickingScript.enabled = true;
            lockpickingScript.ResetMinigame();
        }
        else
        {
            Debug.LogError("MinigameController: Не вдалося знайти скрипт Lockpicking на об'єкті мінігри!");
        }

        if (blurVolume != null) blurVolume.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnMinigameStart.Invoke();
    }

    public void StopLockpicking(bool success = false)
    {
        isMinigameActive = false;

        if (success)
        {
            StartCoroutine(HandleSuccessDelay());
        }
        else
        {
            HideUI();
            EndMinigameSession();
        }
    }

    private IEnumerator HandleSuccessDelay()
    {
        yield return new WaitForSeconds(1.0f);

        HideUI();

        if (currentDoor != null)
        {
            currentDoor.Unlock();
        }

        EndMinigameSession();
    }

    private void HideUI()
    {
        if (lockpickingPrefab != null) lockpickingPrefab.SetActive(false);
        if (blurVolume != null) blurVolume.SetActive(false);
    }

    private void EndMinigameSession()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnMinigameEnd.Invoke();
        currentDoor = null;
    }
}