using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class JellyEffect : MonoBehaviour
{
    [Header("Squash & Stretch")]
    [SerializeField] private float stretchAmount = 0.25f;
    [SerializeField] private float squashAmount = 0.15f;
    [SerializeField] private float deformationSmoothing = 10f;

    [Header("Wobble")]
    [SerializeField] private float wobbleFrequency = 12f;
    [SerializeField] private float wobbleDamping = 4f;
    [SerializeField] private float collisionImpactMultiplier = 0.4f;

    private Rigidbody rigidBody;
    private SphereCollider sphereCollider;
    private Vector3 baseScale;
    private float baseColliderRadius;
    private float wobbleAmount;
    private float wobbleVelocity;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        baseScale = transform.localScale;
        baseColliderRadius = sphereCollider != null ? sphereCollider.radius : 0f;
    }

    public void RefreshBaseScale()
    {
        baseScale = transform.localScale;
        baseColliderRadius = sphereCollider != null ? sphereCollider.radius : 0f;
    }

    private void Update()
    {
        UpdateWobble();
        ApplyDeformation();
    }

    private void UpdateWobble()
    {
        wobbleVelocity -= wobbleAmount * wobbleFrequency * wobbleFrequency * Time.deltaTime;
        wobbleVelocity -= wobbleVelocity * wobbleDamping * Time.deltaTime;
        wobbleAmount += wobbleVelocity * Time.deltaTime;
    }

    private void ApplyDeformation()
    {
        Vector3 velocity = rigidBody.linearVelocity;
        float horizontalSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        float verticalSpeed = Mathf.Abs(velocity.y);

        float yScale = CalculateYScale(horizontalSpeed, verticalSpeed);
        float xzScale = CalculateXZScale(yScale);

        Vector3 targetScale = new Vector3(
            baseScale.x * xzScale,
            baseScale.y * yScale,
            baseScale.z * xzScale
        );

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * deformationSmoothing);
        SyncColliderRadius();
    }

    private void SyncColliderRadius()
    {
        if (sphereCollider == null)
        {
            return;
        }

        sphereCollider.radius = baseColliderRadius * transform.localScale.x / baseScale.x;
    }

    private float CalculateYScale(float horizontalSpeed, float verticalSpeed)
    {
        float movementSquash = 1f - horizontalSpeed * squashAmount * 0.1f;
        float verticalStretch = 1f + verticalSpeed * stretchAmount * 0.1f;
        return Mathf.Clamp(movementSquash * verticalStretch + wobbleAmount, 0.5f, 2f);
    }

    private float CalculateXZScale(float yScale)
    {
        return Mathf.Clamp(1f + (1f - yScale) * 0.5f, 0.5f, 2f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        wobbleVelocity += impact * collisionImpactMultiplier;
    }
}
