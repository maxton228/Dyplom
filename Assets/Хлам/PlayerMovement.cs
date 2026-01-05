using UnityEngine;
using UnityEngine.InputSystem; // Обов'язково

public class PlayerMovementRB : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // ЗМІНА ТУТ: Замість 'InputAction.CallbackContext' використовуємо 'InputValue'
    public void OnMove(InputValue value)
    {
        // ЗМІНА ТУТ: Замість 'ReadValue' використовуємо 'Get'
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }
}