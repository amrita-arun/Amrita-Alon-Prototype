using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public enum ControlScheme
    {
        WASD,
        ArrowKeys
    }

    [Header("Controls")]
    [SerializeField] private ControlScheme controlScheme;
    [SerializeField] private float movementSpeed = 6f;

    private Rigidbody rb;
    private Vector2 movementInput;
    private float startingScale;
    private float knockbackTimer;

    public bool HasMovementInput =>
    movementInput.sqrMagnitude > 0.01f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startingScale = transform.localScale.x;
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        // While being knocked back, preserve the impact velocity instead of
        // immediately replacing it with normal movement.
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        float currentScale = Mathf.Max(transform.localScale.x, 0.01f);
        float adjustedSpeed = movementSpeed * (startingScale / currentScale);

        rb.linearVelocity = new Vector3(
            movementInput.x * adjustedSpeed,
            movementInput.y * adjustedSpeed,
            0f
        );
    }

    public void ApplyKnockback(Vector3 velocity, float duration)
    {
        velocity.z = 0f;
        rb.linearVelocity = velocity;
        knockbackTimer = duration;
    }

    private void ReadInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            movementInput = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (controlScheme == ControlScheme.WASD)
        {
            if (keyboard.aKey.isPressed) horizontal = -1f;
            if (keyboard.dKey.isPressed) horizontal = 1f;
            if (keyboard.sKey.isPressed) vertical = -1f;
            if (keyboard.wKey.isPressed) vertical = 1f;
        }
        else
        {
            if (keyboard.leftArrowKey.isPressed) horizontal = -1f;
            if (keyboard.rightArrowKey.isPressed) horizontal = 1f;
            if (keyboard.downArrowKey.isPressed) vertical = -1f;
            if (keyboard.upArrowKey.isPressed) vertical = 1f;
        }

        movementInput = new Vector2(horizontal, vertical);
    }

    private void OnDisable()
    {
        knockbackTimer = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }
}