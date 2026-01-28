using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Налаштування")]
    public ItemData itemData;
    [SerializeField] private GameObject uiCanvas; // Канвас з літерою F

    // Таймер (як у твоїх дверях)
    private float lastLookTime = -1f;

    void Start()
    {
        if (uiCanvas != null) uiCanvas.SetActive(false);
    }

    void Update()
    {
        if (uiCanvas != null)
        {
            // ЛОГІКА АВТО-ЗНИКНЕННЯ (1 в 1 як у дверях)
            // Якщо гравець подивився менше ніж 0.1с тому -> показуємо
            bool shouldShow = (Time.time - lastLookTime < 0.1f);

            if (uiCanvas.activeSelf != shouldShow)
            {
                uiCanvas.SetActive(shouldShow);
            }

            // Додатково: повертаємо F до камери, щоб читалось
            if (shouldShow && Camera.main != null)
            {
                uiCanvas.transform.LookAt(uiCanvas.transform.position + Camera.main.transform.forward);
            }
        }
    }

    // Гравець просто "пінгує" цей метод щокадру (як у дверях)
    public void ShowPrompt()
    {
        lastLookTime = Time.time;
    }

    public void OnInteract()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Add(itemData);
            Debug.Log($"Підібрано: {itemData.itemName}");
        }
        Destroy(gameObject);
    }
}
