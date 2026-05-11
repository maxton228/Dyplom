using UnityEngine;
using UnityEngine.InputSystem;
using InfimaGames.LowPolyShooterPack;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Посилання")]
    [SerializeField] private Transform playerCamera;

    [SerializeField] private Movement movementScript;

    [Header("Налаштування шарів")]
    [SerializeField] private LayerMask ignoreLayers;
    [SerializeField] private LayerMask corpseLayer;

    [Header("Дальність")]
    [SerializeField] private float doorDistance = 2.5f;
    [SerializeField] private float itemDistance = 3.0f;
    [SerializeField] private float corpseMaxGrabDistance = 2.5f;
    [SerializeField] private float itemSphereRadius = 0.3f;

    [Header("Ефекти Тяжіння")]
    [Range(0.1f, 1f)][SerializeField] private float speedMultiplier = 0.3f;
    [Range(0.1f, 1f)][SerializeField] private float sensitivityMultiplier = 0.4f;

    private AdvancedDoor currentDoor;
    private ItemPickup currentItem;
    private Rigidbody currentCorpsePart;
    private AdvancedDoor draggingDoor;

    private HackablePanelInteract currentPanel;

    private ConfigurableJoint corpseJoint;
    private GameObject dragAnchor;
    private float currentDragDistance;

    private bool isHoldingKey = false;
    private float holdTimer = 0f;
    private float clickThreshold = 0.2f;

    void Update()
    {
        CheckSurroundings();
        HandleInput();
        UpdateDragAnchor();
    }

    void CheckSurroundings()
    {
        if (playerCamera == null) return;
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        currentItem = null; currentDoor = null; currentCorpsePart = null; currentPanel = null;

        LayerMask mask = ~ignoreLayers;

        if (Physics.Raycast(ray, out hit, itemDistance, mask))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            currentDoor = hit.collider.GetComponentInParent<AdvancedDoor>();
            currentItem = hit.collider.GetComponentInParent<ItemPickup>();

            currentPanel = hit.collider.GetComponentInParent<HackablePanelInteract>();

            bool isCorpseLayer = ((1 << hit.collider.gameObject.layer) & corpseLayer) != 0;
            if (isCorpseLayer || hit.collider.GetComponent<Rigidbody>() != null)
            {
                if (hit.collider.transform.root.GetComponentInChildren<CorpseInteract>() != null)
                {
                    currentCorpsePart = hit.collider.GetComponent<Rigidbody>();
                    if (interactable == null)
                    {
                        interactable = hit.collider.transform.root.GetComponentInChildren<IInteractable>();
                    }
                }
            }

            if (interactable != null)
            {
                interactable.ShowPrompt();
            }
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (currentPanel != null)
            {
                currentPanel.Interact();
            }
            else if (currentItem != null && !isHoldingKey && corpseJoint == null)
                currentItem.OnInteract();
            else if (currentDoor != null)
            {
                draggingDoor = currentDoor;
                isHoldingKey = true;
                holdTimer = 0f;
                draggingDoor.BeginDrag();
            }
            else if (currentCorpsePart != null && corpseJoint == null)
                GrabCorpse(currentCorpsePart);
        }

        if (Input.GetKey(KeyCode.F) && isHoldingKey && draggingDoor != null)
        {
            holdTimer += Time.deltaTime;
            float mouseX = (Mouse.current != null) ? Mouse.current.delta.x.ReadValue() * 0.1f : Input.GetAxis("Mouse X");
            if (Mathf.Abs(mouseX) > 0.01f) draggingDoor.OnDrag(mouseX);
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            if (isHoldingKey && draggingDoor != null)
            {
                draggingDoor.EndDrag();
                if (holdTimer < clickThreshold) draggingDoor.ToggleDoor(transform.position);
                isHoldingKey = false;
                draggingDoor = null;
            }
            if (corpseJoint != null) ReleaseCorpse();
        }
    }

    void GrabCorpse(Rigidbody targetPart)
    {
        Vector3 playerPosFlat = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPosFlat = new Vector3(targetPart.position.x, 0, targetPart.position.z);
        currentDragDistance = Vector3.Distance(playerPosFlat, targetPosFlat);
        CorpseInteract corpse = targetPart.transform.root.GetComponentInChildren<CorpseInteract>();
        if (corpse != null) corpse.StartDragging();
        dragAnchor = new GameObject("DragAnchor");
        dragAnchor.transform.position = targetPart.position;
        dragAnchor.transform.SetParent(this.transform);

        Rigidbody anchorRB = dragAnchor.AddComponent<Rigidbody>();
        anchorRB.isKinematic = true;

        corpseJoint = targetPart.gameObject.AddComponent<ConfigurableJoint>();
        corpseJoint.autoConfigureConnectedAnchor = false;
        corpseJoint.connectedBody = anchorRB;
        corpseJoint.anchor = Vector3.zero;
        corpseJoint.connectedAnchor = Vector3.zero;

        corpseJoint.xMotion = corpseJoint.yMotion = corpseJoint.zMotion = ConfigurableJointMotion.Limited;
        corpseJoint.angularXMotion = corpseJoint.angularYMotion = corpseJoint.angularZMotion = ConfigurableJointMotion.Free;

        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = 0.05f;
        corpseJoint.linearLimit = limit;

        Rigidbody[] allParts = targetPart.transform.root.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in allParts)
        {
            rb.linearDamping = 5.0f;
            rb.angularDamping = 15.0f;
        }

        SetDraggingState(true);

        Collider playerCol = GetComponentInParent<Collider>();
        if (playerCol != null)
        {
            foreach (var col in targetPart.transform.root.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(playerCol, col, true);
        }

        foreach (var smr in targetPart.transform.root.GetComponentsInChildren<SkinnedMeshRenderer>())
            smr.updateWhenOffscreen = true;
    }

    void UpdateDragAnchor()
    {
        if (corpseJoint != null && dragAnchor != null)
        {
            Vector3 forwardFlat = transform.forward;
            forwardFlat.y = 0;
            forwardFlat.Normalize();

            Vector3 targetPos = transform.position + (forwardFlat * currentDragDistance);

            RaycastHit hit;
            if (Physics.Raycast(targetPos + Vector3.up * 1.5f, Vector3.down, out hit, 4f, ~corpseLayer))
            {
                targetPos.y = hit.point.y + 0.2f;
            }

            dragAnchor.transform.position = Vector3.Lerp(dragAnchor.transform.position, targetPos, Time.deltaTime * 8f);
        }
    }

    void ReleaseCorpse()
    {
        if (corpseJoint != null)
        {
            CorpseInteract corpse = corpseJoint.transform.root.GetComponentInChildren<CorpseInteract>();
            if (corpse != null) corpse.StopDragging();
            Rigidbody[] allParts = corpseJoint.transform.root.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in allParts)
            {
                rb.linearDamping = 0.05f;
                rb.angularDamping = 0.05f;
            }

            SetDraggingState(false);

            Collider playerCol = GetComponentInParent<Collider>();
            if (playerCol != null)
            {
                foreach (var col in corpseJoint.transform.root.GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(playerCol, col, false);
            }

            Destroy(corpseJoint);
            Destroy(dragAnchor);
            corpseJoint = null;
        }
    }

    void SetDraggingState(bool isDragging)
    {
        if (movementScript != null)
        {
            movementScript.isDraggingCorpse = isDragging;
            movementScript.draggingMultiplier = isDragging ? speedMultiplier : 1.0f;
            movementScript.mouseSensitivity = isDragging ? sensitivityMultiplier : 1.0f;
        }
    }
}