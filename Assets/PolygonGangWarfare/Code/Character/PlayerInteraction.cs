using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInteraction : MonoBehaviour
{
    [Header("Налаштування")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private LayerMask ignoreLayers;

    [Header("Дальність")]
    [SerializeField] private float doorDistance = 2.5f;
    [SerializeField] private float itemDistance = 3.0f;
    [SerializeField] private float itemSphereRadius = 0.3f; // Радіус "плями" для предметів

    // Поточні цілі
    private AdvancedDoor currentDoor;
    private ItemPickup currentItem;

    // Змінні дверей
    private AdvancedDoor draggingDoor;
    private bool isHoldingKey = false;
    private float holdTimer = 0f;
    private float clickThreshold = 0.2f;

    void Update()
    {
        CheckSurroundings();
        HandleInput();
    }

    void CheckSurroundings()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        // 1. ШУКАЄМО ПРЕДМЕТИ (SphereCast - прощає неточність)
        currentItem = null; // Скидаємо, якщо нічого не знайшли - предмет сам вимкне UI по таймеру

        if (Physics.SphereCast(ray, itemSphereRadius, out hit, itemDistance, ~ignoreLayers))
        {
            ItemPickup item = hit.collider.GetComponentInParent<ItemPickup>();
            if (item != null)
            {
                currentItem = item;
                currentItem.ShowPrompt(); // "Пінгуємо" предмет -> він покаже F
            }
        }

        // 2. ШУКАЄМО ДВЕРІ (Raycast - точність)
        currentDoor = null;

        if (Physics.Raycast(ray, out hit, doorDistance, ~ignoreLayers))
        {
            AdvancedDoor door = hit.collider.GetComponentInParent<AdvancedDoor>();
            if (door != null)
            {
                currentDoor = door;
                currentDoor.ShowPrompt(); // "Пінгуємо" двері
            }
        }
    }

    void HandleInput()
    {
        // ПРІОРИТЕТ 1: Підняти предмет (якщо не тягнемо двері)
        if (Input.GetKeyDown(KeyCode.F) && currentItem != null && !isHoldingKey && draggingDoor == null)
        {
            currentItem.OnInteract();
            currentItem = null;
            return;
        }

        // ПРІОРИТЕТ 2: Двері
        if (Input.GetKeyDown(KeyCode.F) && currentDoor != null)
        {
            draggingDoor = currentDoor;
            isHoldingKey = true;
            holdTimer = 0f;
            draggingDoor.BeginDrag();
        }

        // Логіка тягання дверей...
        if (Input.GetKey(KeyCode.F) && isHoldingKey && draggingDoor != null)
        {
            holdTimer += Time.deltaTime;
            float mouseX = 0f;
            if (Mouse.current != null) mouseX = Mouse.current.delta.x.ReadValue() * 0.1f;
            else mouseX = Input.GetAxis("Mouse X");

            if (Mathf.Abs(mouseX) > 0.01f) draggingDoor.OnDrag(mouseX);
        }

        if (Input.GetKeyUp(KeyCode.F) && isHoldingKey)
        {
            if (draggingDoor != null)
            {
                draggingDoor.EndDrag();
                if (holdTimer < clickThreshold) draggingDoor.ToggleDoor(transform.position);
            }
            isHoldingKey = false;
            draggingDoor = null;
        }
    }
}

