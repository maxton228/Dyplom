using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InventorySlot : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Button button; // ўоб можна було кл≥кати

    private ItemData item;

    public void AddItem(ItemData newItem, int amount)
    {
        item = newItem;
        iconImage.sprite = item.icon;
        iconImage.enabled = true;

        if (amount > 1)
        {
            amountText.text = amount.ToString();
            amountText.enabled = true;
        }
        else
        {
            amountText.enabled = false;
        }
    }

    public void ClearSlot()
    {
        item = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
        amountText.enabled = false;
    }
}
