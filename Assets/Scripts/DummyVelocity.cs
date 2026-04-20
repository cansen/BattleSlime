using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DummyVelocity : MonoBehaviour
{
    [SerializeField] private Vector3 constantVelocity = Vector3.zero;

    private Rigidbody rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rigidBody.linearVelocity = new Vector3(constantVelocity.x, rigidBody.linearVelocity.y, constantVelocity.z);
    }
}
