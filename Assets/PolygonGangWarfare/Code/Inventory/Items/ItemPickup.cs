using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData; // —юди перет€гуЇш створений ScriptableObject

    public void OnInteract()
    {
        // ƒодаЇмо в ≥нвентар
        InventorySystem.Instance.AddItem(itemData);

        // «нищуЇмо об'Їкт з≥ сцени
        Destroy(gameObject);
    }
}
