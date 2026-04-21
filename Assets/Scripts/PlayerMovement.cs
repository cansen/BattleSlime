using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.6f;

    private Rigidbody rigidBody;
    private PlayerStats stats;
    private InputSystem_Actions inputActions;
    private Transform cameraTransform;

    private Vector2 moveInput;
    private bool isDashing;
    private bool isGrounded;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        stats = GetComponent<PlayerStats>();
        inputActions = new InputSystem_Actions();
        cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Sprint.performed += OnDashStarted;
        inputActions.Player.Sprint.canceled += OnDashCanceled;
    }

    private void OnDisable()
    {
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Sprint.performed -= OnDashStarted;
        inputActions.Player.Sprint.canceled -= OnDashCanceled;
        inputActions.Player.Disable();
    }

    private void Update()
    {
        if (stats.Runner != null && !stats.HasStateAuthority)
        {
            return;
        }

        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        isGrounded = CheckGrounded();
    }

    private void FixedUpdate()
    {
        if (stats.Runner != null && !stats.HasStateAuthority)
        {
            return;
        }

        ApplyMovement();
        ApplySlide();
    }

    private void ApplyMovement()
    {
        if (moveInput.sqrMagnitude < 0.01f)
        {
            return;
        }

        Vector3 direction = CalculateMoveDirection();
        float speed = CalculateMovementSpeed();
        Vector3 velocity = direction * speed;
        velocity.y = rigidBody.linearVelocity.y;
        rigidBody.linearVelocity = velocity;
    }

    private void ApplySlide()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            return;
        }

        Vector3 horizontal = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);
        if (horizontal.sqrMagnitude < 0.01f)
        {
            return;
        }

        Vector3 damped = horizontal - horizontal * (stats.playSlideDamping * Time.fixedDeltaTime);
        rigidBody.linearVelocity = new Vector3(damped.x, rigidBody.linearVelocity.y, damped.z);
    }

    private Vector3 CalculateMoveDirection()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        return (forward.normalized * moveInput.y + right.normalized * moveInput.x).normalized;
    }

    private float CalculateMovementSpeed()
    {
        float sizePenalty = 1f + (stats.playerCurrentSize - 1f) * stats.playerSizeMovementConstant;
        float speed = stats.playerBaseMovementSpeed / sizePenalty;
        return isDashing ? speed * stats.playerDashMultiplier : speed;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (stats.Runner != null && !stats.HasStateAuthority)
        {
            return;
        }

        if (!isGrounded)
        {
            return;
        }

        rigidBody.AddForce(Vector3.up * CalculateJumpForce(), ForceMode.Impulse);
    }

    private float CalculateJumpForce()
    {
        float gravity = Mathf.Abs(Physics.gravity.y);
        float velocity = Mathf.Sqrt(2f * gravity * stats.playerJumpHeight);
        return rigidBody.mass * velocity;
    }

    private void OnDashStarted(InputAction.CallbackContext context) => isDashing = true;

    private void OnDashCanceled(InputAction.CallbackContext context) => isDashing = false;

    private bool CheckGrounded()
    {
        float scaledDistance = transform.localScale.y * 0.5f + groundCheckDistance;
        return Physics.Raycast(transform.position, Vector3.down, scaledDistance, groundLayer);
    }
}
