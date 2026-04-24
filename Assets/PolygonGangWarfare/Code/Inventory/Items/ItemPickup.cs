using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;
    private InteractablePrompt _prompt;

    void Awake() => _prompt = GetComponent<InteractablePrompt>();

    public void ShowPrompt()
    {
        // Просто передаємо текст
        if (_prompt != null) _prompt.Show("Підібрати " + itemData.itemName);
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
