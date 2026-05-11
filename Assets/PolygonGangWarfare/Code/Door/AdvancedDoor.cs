using UnityEngine;
using TMPro;

public class AdvancedDoor : MonoBehaviour, IInteractable
{
    [Header("Налаштування")]
    [SerializeField] private float maxAngle = 90f;
    [SerializeField] private float smoothSpeed = 4f;
    [SerializeField] private bool invertRotation = false;

    [Header("Замок та Мінігра")]
    public bool isLocked = false;

    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private bool isDragging = false;

    private InteractablePrompt _prompt;

    void Awake()
    {
        _prompt = GetComponent<InteractablePrompt>();
    }

    void Update()
    {
        if (!isDragging)
        {
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);
            transform.localRotation = Quaternion.Euler(0, currentAngle, 0);
        }
    }

    public void ShowPrompt()
    {
        if (_prompt == null) return;

        if (isLocked)
        {
            _prompt.Show("Взлом [F]", Color.red);
        }
        else
        {
            _prompt.Show("[F]", Color.white);
        }
    }

    public void ToggleDoor(Vector3 playerPosition)
    {
        if (isLocked)
        {
            if (MinigameController.Instance != null)
                MinigameController.Instance.StartLockpicking(this);
            return;
        }

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

    public void BeginDrag()
    {
        if (isLocked)
        {
            if (MinigameController.Instance != null) MinigameController.Instance.StartLockpicking(this);
            return;
        }
        isDragging = true;
    }

    public void OnDrag(float mouseDelta)
    {
        if (isLocked) return;
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

    public void Unlock() => isLocked = false;
}