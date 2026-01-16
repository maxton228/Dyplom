using UnityEngine;
using UnityEngine;
using InfimaGames.LowPolyShooterPack;
using System.Collections.Generic;
using System.Collections;

public class InventorySystem : MonoBehaviour
{// --- œŒ¬≈–Õ”¬ SINGLETON ---
    public static InventorySystem Instance;

    private void Awake()
    {
        Instance = this;
    }

    [Header("Õ‡Î‡¯ÚÛ‚‡ÌÌˇ")]
    [SerializeField] private Character characterController;
    [SerializeField] private Animator mainCharacterAnimator;
    [SerializeField] private GameObject tabletModel;

    private bool isInventoryOpen = false;
    private Coroutine switchRoutine;

    private int tabletLayerIndex = -1;
    private int posesLayerIndex = -1;

    private int hashIkLeft;
    private int hashIkRight;

    void Start()
    {
        if (mainCharacterAnimator != null)
        {
            tabletLayerIndex = mainCharacterAnimator.GetLayerIndex("Tablet Layer");

            posesLayerIndex = mainCharacterAnimator.GetLayerIndex("Layer Poses");

            hashIkLeft = Animator.StringToHash("Alpha IK Hand Left");
            hashIkRight = Animator.StringToHash("Alpha IK Hand Right");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (switchRoutine != null) return;
            isInventoryOpen = !isInventoryOpen;
            switchRoutine = StartCoroutine(NuclearSwitch(isInventoryOpen));
        }
    }

    public void AddItem(ItemData item)
    {
        Debug.Log("œ≥‰≥·‡ÌÓ: " + item.itemName);
    }

    IEnumerator NuclearSwitch(bool opening)
    {
        var inventory = characterController.GetComponent<Inventory>();

        if (opening) 
        {
            if (characterController != null)
            {
                characterController.SetInventoryOpen(true);
                characterController.SetHolstered(true);
            }

            yield return new WaitForSeconds(0.1f);

            if (inventory != null)
            {
                var weapon = inventory.GetEquipped();
                if (weapon != null) weapon.gameObject.SetActive(false);
            }

            if (mainCharacterAnimator != null && posesLayerIndex != -1)
                mainCharacterAnimator.SetLayerWeight(posesLayerIndex, 0f);

            if (mainCharacterAnimator != null && tabletLayerIndex != -1)
                mainCharacterAnimator.SetLayerWeight(tabletLayerIndex, 1f);

            if (mainCharacterAnimator != null)
            {
                mainCharacterAnimator.SetFloat(hashIkLeft, 1f);
                mainCharacterAnimator.SetFloat(hashIkRight, 1f);
                mainCharacterAnimator.Play("Tablet_Show", tabletLayerIndex, 0f);
            }

            if (tabletModel != null) tabletModel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else 
        {
            if (mainCharacterAnimator != null)
                mainCharacterAnimator.Play("Tablet_Hide", tabletLayerIndex, 0f);

            yield return new WaitForSeconds(0.5f); 

            if (mainCharacterAnimator != null)
            {
                if (tabletLayerIndex != -1) mainCharacterAnimator.SetLayerWeight(tabletLayerIndex, 0f);
                mainCharacterAnimator.SetFloat(hashIkLeft, 0f);
                mainCharacterAnimator.SetFloat(hashIkRight, 0f);
            }

            if (mainCharacterAnimator != null && posesLayerIndex != -1)
                mainCharacterAnimator.SetLayerWeight(posesLayerIndex, 1f);

            if (inventory != null)
            {
                var weapon = inventory.GetEquipped();
                if (weapon != null) weapon.gameObject.SetActive(true);
            }

            if (tabletModel != null) tabletModel.SetActive(false);

            if (characterController != null)
            {
                characterController.SetInventoryOpen(false);
                characterController.SetHolstered(false);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        switchRoutine = null;
    }
}
