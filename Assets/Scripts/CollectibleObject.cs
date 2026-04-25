using Fusion;
using UnityEngine;

public class CollectibleObject : NetworkBehaviour
{
    [Header("Collection")]
    [SerializeField] private float defaultSizeValue = 1f;
    [SerializeField] private float stationaryVelocity = 0.1f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 0.05f;

    [Header("Formula Parameters")]
    [SerializeField] private float sizeMultiplier = 1f;
    [SerializeField] private float velocityMultiplier = 1f;
    [SerializeField] private float velocityConstant = 0.1f;
    [SerializeField] private float damageMultiplier = 0.005f;
    [SerializeField] private float damageConstant = 5000f;

    [Networked] private float networkedSizeValue { get; set; }
    [Networked] private NetworkBool networkedCanDamagePlayer { get; set; }
    [Networked] private NetworkBool networkedIsCollected { get; set; }
    [Networked] private TickTimer lifetimeTimer { get; set; }

    private float localSizeValue;
    private bool localCanDamagePlayer = true;
    private bool localIsCollected;
    private float graceDuration;
    private float spawnTime = float.MinValue;
    private Rigidbody rb;

    public float objectSizeValue
    {
        get => Runner != null ? networkedSizeValue : localSizeValue;
        set
        {
            localSizeValue = value;
            if (Runner != null)
            {
                networkedSizeValue = value;
            }
        }
    }

    public bool canDamagePlayer
    {
        get => Runner != null ? (bool)networkedCanDamagePlayer : localCanDamagePlayer;
        set
        {
            localCanDamagePlayer = value;
            if (Runner != null)
            {
                networkedCanDamagePlayer = value;
            }
        }
    }

    private bool isCollected
    {
        get => Runner != null ? (bool)networkedIsCollected : localIsCollected;
        set
        {
            localIsCollected = value;
            if (Runner != null)
            {
                networkedIsCollected = value;
            }
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        localSizeValue = defaultSizeValue;
    }

    public void ScheduleDespawn(float time)
    {
        if (Runner != null)
        {
            lifetimeTimer = TickTimer.CreateFromSeconds(Runner, time);
        }
        else
        {
            Destroy(gameObject, time);
        }
    }

    public void SetGracePeriod(float duration)
    {
        graceDuration = duration;
        spawnTime = Time.time;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }
        if (lifetimeTimer.IsRunning && lifetimeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isCollected)
        {
            return;
        }
        if (Time.time - spawnTime < graceDuration)
        {
            return;
        }

        PlayerStats player = collision.gameObject.GetComponent<PlayerStats>();
        if (player == null)
        {
            return;
        }
        if (player.Runner != null && !player.HasStateAuthority)
        {
            return;
        }

        if (player.playerCurrentSize > objectSizeValue)
        {
            isCollected = true;
            player.Grow(objectSizeValue);
            if (Runner != null)
            {
                Runner.Despawn(Object);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            ApplyCombatDamage(player);
        }
    }

    private void ApplyCombatDamage(PlayerStats player)
    {
        if (!canDamagePlayer)
        {
            return;
        }

        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
        float playerVelocity = playerCombat != null ? playerCombat.GetEffectiveVelocity() : stationaryVelocity;
        float collectibleVelocity = rb != null ? Mathf.Max(rb.linearVelocity.magnitude, stationaryVelocity) : stationaryVelocity;

        int playerMomentum = CombatFormula.CalculateMomentum(player.playerCurrentSize, playerVelocity, sizeMultiplier, velocityMultiplier, velocityConstant);
        int collectibleMomentum = CombatFormula.CalculateMomentum(objectSizeValue, collectibleVelocity, sizeMultiplier, velocityMultiplier, velocityConstant);
        int damageToPlayer = CombatFormula.CalculateDamage(collectibleMomentum, playerMomentum, damageMultiplier, damageConstant);

        Debug.Log($"[Combat] {player.gameObject.name} hit collectible — PlayerMomentum: {playerMomentum}, CollectibleMomentum: {collectibleMomentum}, Damage: {damageToPlayer}");

        player.TakeDamage(damageToPlayer);

        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        if (playerRigidbody != null)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            float force = Mathf.Max(collectibleMomentum - playerMomentum, 0) * knockbackForce;
            playerRigidbody.AddForce(direction * force, ForceMode.Impulse);
        }
    }
}
