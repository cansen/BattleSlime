using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 60f;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

    private InputSystem_Actions inputActions;
    private float horizontalAngle;
    private float verticalAngle = 20f;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        RotateCamera();
        PositionCamera();
    }

    private void RotateCamera()
    {
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        horizontalAngle += lookInput.x * sensitivity;
        verticalAngle -= lookInput.y * sensitivity;
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
    }

    private void PositionCamera()
    {
        Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0f);
        Vector3 pivot = target.position + targetOffset;
        transform.position = pivot - rotation * Vector3.forward * distance;
        transform.LookAt(pivot);
    }
}
