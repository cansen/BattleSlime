using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
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
    public float playerMaxSize = 1000f;
    [SerializeField] private float playerStartSize = 100f;
    public float playerInstantDeathSize = 10f;

    [Header("Combat")]
    public float playerDestructionThreshold = 50f;
    public float playerDamageIndicatorDuration = 1.5f;
    public float playerCollisionForce = 10f;
    public float playerMass = 1f;

    [Networked] private float networkedSize { get; set; }
    [Networked] private NetworkBool networkedIsDead { get; set; }

    private float localSize;
    private bool localIsDead;

    public float playerCurrentSize
    {
        get => Runner != null ? networkedSize : localSize;
        set
        {
            localSize = value;
            if (Runner != null)
            {
                networkedSize = value;
            }
        }
    }

    public bool isDead
    {
        get => Runner != null ? networkedIsDead : localIsDead;
        set
        {
            localIsDead = value;
            if (Runner != null)
            {
                networkedIsDead = value;
            }
        }
    }

    public bool isLocalPlayer { get; private set; }

    private static readonly List<PlayerStats> activePlayers = new List<PlayerStats>();
    public static IReadOnlyList<PlayerStats> ActivePlayers => activePlayers;

    private JellyEffect jellyEffect;

    private void Awake()
    {
        jellyEffect = GetComponent<JellyEffect>();
        localSize = playerStartSize;
        transform.localScale = Vector3.one * CalculateVisualScale();
        jellyEffect?.RefreshBaseScale();
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            isLocalPlayer = true;
            playerCurrentSize = playerStartSize;
        }

        transform.localScale = Vector3.one * CalculateVisualScale();
        jellyEffect?.RefreshBaseScale();
    }

    public override void Render()
    {
        if (playerCurrentSize > 0f)
        {
            transform.localScale = Vector3.one * CalculateVisualScale();
        }
    }

    private void OnEnable() => activePlayers.Add(this);
    private void OnDisable() => activePlayers.Remove(this);

    public void Grow(float amount)
    {
        playerCurrentSize = Mathf.Min(playerCurrentSize + amount, playerMaxSize);
        transform.localScale = Vector3.one * CalculateVisualScale();
        jellyEffect?.RefreshBaseScale();
    }

    public void TakeDamage(float damage)
    {
        playerCurrentSize -= damage;

        if (playerCurrentSize <= playerInstantDeathSize)
        {
            Die();
            return;
        }

        transform.localScale = Vector3.one * CalculateVisualScale();
        jellyEffect?.RefreshBaseScale();
    }

    public void Die()
    {
        isDead = true;

        if (Runner != null)
        {
            Runner.Despawn(Object);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float CalculateVisualScale()
    {
        return Mathf.Pow(playerCurrentSize, 1f / 3f);
    }
}
