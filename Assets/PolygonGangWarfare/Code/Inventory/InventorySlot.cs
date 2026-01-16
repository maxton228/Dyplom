using UnityEngine;
using UnityEngine.UI;
public class InventorySlot : MonoBehaviour
{
    private Image buttonImage;
    private Button button;

    private ItemData currentItem;

    // Цей метод спрацює автоматично при створенні кнопки
    void Awake()
    {
        // Скрипт сам знаходить компоненти Image та Button на цьому ж об'єкті
        buttonImage = GetComponent<Image>();
        button = GetComponent<Button>();
    }

    // Цей метод викликає InventorySystem, коли додає предмет
    public void Setup(ItemData newItem)
    {
        currentItem = newItem;

        if (currentItem != null)
        {
            // Якщо компонент Image знайшовся — міняємо йому спрайт
            if (buttonImage != null)
            {
                buttonImage.sprite = currentItem.icon;
                buttonImage.enabled = true;
            }
        }
    }

    public void OnClick()
    {
        if (currentItem != null)
        {
            Debug.Log("Клікнули по предмету: " + currentItem.itemName);
        }
    }
}
