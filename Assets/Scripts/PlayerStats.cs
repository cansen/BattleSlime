using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Movement")]
    public float playerBaseMovementSpeed = 5f;
    public float playerSizeMovementConstant = 1f;
    public float playerSlideSpeed = 3f;
    public float playSlideDamping = 4f;
    public float playerDashMultiplier = 2.5f;

    [Header("Jump")]
    public float playerJumpHeight = 2f;
    public float playerJumpHeightReachDuration = 0.4f;
    public float instantStopDuration = 0.1f;

    [Header("Size")]
    public float playerMaxSize = 10f;
    public float playerCurrentSize = 1f;
    public float playerInstantDeathSize = 0.3f;

    [Header("Combat")]
    public float playerDestructionThreshold = 50f;
    public float playerDamageIndicatorDuration = 1.5f;
    public float playerCollisionForce = 10f;
    public float playerMass = 1f;

    private JellyEffect jellyEffect;

    private void Awake()
    {
        jellyEffect = GetComponent<JellyEffect>();
        transform.localScale = Vector3.one * CalculateVisualScale();
        jellyEffect?.RefreshBaseScale();
    }

    public void Grow(float amount)
    {
        playerCurrentSize = Mathf.Min(playerCurrentSize + amount, playerMaxSize);
        transform.localScale = Vector3.one * CalculateVisualScale();
        jellyEffect?.RefreshBaseScale();
    }

    public float CalculateVisualScale()
    {
        return Mathf.Pow(playerCurrentSize, 1f / 3f);
    }
}
