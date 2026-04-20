using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DummyAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private string targetTag = "Player";

    private Rigidbody rigidBody;
    private Transform target;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        GameObject targetObject = GameObject.FindWithTag(targetTag);
        if (targetObject != null)
        {
            target = targetObject.transform;
        }
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        MoveTowardTarget();
    }

    private void MoveTowardTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;
        rigidBody.linearVelocity = new Vector3(direction.x * moveSpeed, rigidBody.linearVelocity.y, direction.z * moveSpeed);
    }
}
