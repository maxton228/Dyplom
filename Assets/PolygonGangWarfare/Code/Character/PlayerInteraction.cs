using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask doorLayer;
    private AdvancedDoor currentLookTarget; // Двері, на які просто дивимось
    private AdvancedDoor draggingDoor;
    private AdvancedDoor currentDoor;
    private bool isHoldingKey = false;
    private float holdTimer = 0f;
    private float clickThreshold = 0.2f; // Час у секундах: якщо тримав менше - це клік

    void Update()
    {
        CheckForDoors(); // Постійно шукаємо двері поглядом
        HandleInput();
    }
    void CheckForDoors()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Використовуємо SphereCast (радіус 0.05f щоб працювало впритул)
        if (Physics.SphereCast(ray, 0.05f, out hit, interactRange)) // Прибрав фільтр шару дверей, щоб бачити все
        {
            // 1. ПЕРЕВІРКА НА ДВЕРІ
            AdvancedDoor door = hit.collider.GetComponentInParent<AdvancedDoor>();
            if (door != null)
            {
                currentLookTarget = door;
                currentLookTarget.ShowPrompt(); // "Натисни F"
                                                // ... логіка дверей
            }
            else
            {
                currentLookTarget = null;
            }

            // 2. ПЕРЕВІРКА НА ПРЕДМЕТИ (НОВЕ!)
            ItemPickup item = hit.collider.GetComponentInParent<ItemPickup>();
            if (item != null)
            {
                // Тут можна показати UI "Підібрати [Назва]"

                if (Input.GetKeyDown(KeyCode.F))
                {
                    item.OnInteract();
                }
            }
        }
    }
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F) && currentLookTarget != null)
        {
            draggingDoor = currentLookTarget; // Запам'ятовуємо, що схопили
            isHoldingKey = true;
            holdTimer = 0f;
            draggingDoor.BeginDrag();
        }

        // ТРИМАЄМО F
        if (Input.GetKey(KeyCode.F) && isHoldingKey && draggingDoor != null)
        {
            holdTimer += Time.deltaTime;

            // Рух миші (підстав свій варіант введення)
            float mouseX = 0f;
            if (Mouse.current != null)
                mouseX = Mouse.current.delta.x.ReadValue() * 0.1f;
            else
                mouseX = Input.GetAxis("Mouse X");

            if (Mathf.Abs(mouseX) > 0.01f)
            {
                draggingDoor.OnDrag(mouseX);
            }
        }

        // ВІДПУСТИЛИ F
        if (Input.GetKeyUp(KeyCode.F) && isHoldingKey)
        {
            if (draggingDoor != null)
            {
                draggingDoor.EndDrag();

                // Якщо це був швидкий клік -> відкрити/закрити
                if (holdTimer < clickThreshold)
                {
                    draggingDoor.ToggleDoor(transform.position);
                }
            }

            isHoldingKey = false;
            draggingDoor = null;
        }
    }
}

