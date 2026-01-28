using UnityEngine;
using System.Collections.Generic;
using InfimaGames.LowPolyShooterPack;
using UnityEngine.InputSystem;
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject inventoryUI; // Посилання на InventoryWindow
    [SerializeField] private Transform slotsParent;  // Теж InventoryWindow (куди додавати слоти)
    [SerializeField] private InventorySlot slotPrefab; // Наш префаб слота

    [Header("Керування")]
    [SerializeField] private PlayerInput playerInput;

    public List<InventoryItem> items = new List<InventoryItem>();

    private bool isOpened = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (playerInput == null) playerInput = FindObjectOfType<PlayerInput>();
        inventoryUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }
    public int GetAmmoCount(ItemData ammoType)
    {
        int total = 0;
        foreach (InventoryItem item in items)
        {
            if (item.data == ammoType)
            {
                total += item.amount;
            }
        }
        return total;
    }
    public int TakeAmmo(ItemData ammoType, int amountNeeded)
    {
        int amountStillNeeded = amountNeeded;
        int amountTakenTotal = 0;

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].data == ammoType)
            {
                int amountInSlot = items[i].amount;

                if (amountInSlot >= amountStillNeeded)
                {
                    items[i].amount -= amountStillNeeded;
                    amountTakenTotal += amountStillNeeded;
                    amountStillNeeded = 0;

                    if (items[i].amount <= 0) items.RemoveAt(i);

                    break; 
                }
                else
                {
                    amountTakenTotal += amountInSlot;
                    amountStillNeeded -= amountInSlot;

                    items.RemoveAt(i);
                }
            }
        }

        if (amountTakenTotal > 0) UpdateUI();

        return amountTakenTotal;
    }
    public void ToggleInventory()
    {
        if (playerInput == null) playerInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
        isOpened = !isOpened;
        inventoryUI.SetActive(isOpened);

        if (isOpened)
        {
            UpdateUI();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerInput != null) playerInput.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerInput != null) playerInput.enabled = true;
        }
    }
    public void Add(ItemData itemData)
    {
        if (itemData.isStackable)
        {
            foreach (InventoryItem item in items)
            {
                if (item.data == itemData)
                {
                    item.amount++;
                    return;
                }
            }
        }

        InventoryItem newItem = new InventoryItem();
        newItem.data = itemData;
        newItem.amount = 1;
        items.Add(newItem);
    }

    void UpdateUI()
    {
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (InventoryItem item in items)
        {
            InventorySlot newSlot = Instantiate(slotPrefab, slotsParent);
            newSlot.AddItem(item.data, item.amount);
        }
    }
}

[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int amount;
}