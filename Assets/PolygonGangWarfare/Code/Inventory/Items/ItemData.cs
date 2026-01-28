using UnityEngine;
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon = null; // Картинка предмету
    [TextArea] public string description = "Опис предмету...";
    public bool isStackable = false;
}