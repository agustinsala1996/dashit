using UnityEngine;

public class JoystickMove : MonoBehaviour
{
    public Joystick movementJoystick;
    public float playerSpeed = 5f;
    private Rigidbody2D rb;

    private DashButton dashButton; // Referencia
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dashButton = Object.FindFirstObjectByType<DashButton>();
    }

    private void FixedUpdate()
    {
        if (dashButton != null && dashButton.GetIsDashing())
            return; // ❌ No sobrescribimos el dash

        Vector2 input = movementJoystick.Direction;

        if (input.magnitude > 0.1f)
        {
            rb.linearVelocity = input.normalized * playerSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}