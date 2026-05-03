using Fusion;
using UnityEngine;

public class CollectibleObject : NetworkBehaviour
{
    [Header("Collection")]
    [SerializeField] private float defaultSizeValue = 1f;
    [SerializeField] private float stationaryVelocity = 0.1f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 0.05f;

    [Header("Mass")]
    [SerializeField] private float massPerSize = 1f;

    [Header("Formula Parameters")]
    [SerializeField] private float sizeMultiplier = 1f;
    [SerializeField] private float velocityMultiplier = 1f;
    [SerializeField] private float velocityConstant = 0.1f;
    [SerializeField] private float damageMultiplier = 0.005f;
    [SerializeField] private float damageConstant = 0f;

    [Networked] private float networkedSizeValue { get; set; }
    [Networked] private NetworkBool networkedCanDamagePlayer { get; set; }
    [Networked] private NetworkBool networkedIsCollected { get; set; }
    [Networked] private NetworkBool networkedIsInitial { get; set; }
    [Networked] private TickTimer lifetimeTimer { get; set; }

    private float localSizeValue;
    private bool localCanDamagePlayer = true;
    private bool localIsCollected;
    private bool localIsInitial;
    private float graceDuration;
    private float spawnTime = float.MinValue;
    private Rigidbody rb;
    private Collider cachedCollider;

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
            SyncMassToSize();
        }
    }

    private void SyncMassToSize()
    {
        if (rb == null)
        {
            return;
        }
        rb.mass = Mathf.Max(objectSizeValue * massPerSize, 0.0001f);
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

    public bool isInitial
    {
        get => Runner != null ? (bool)networkedIsInitial : localIsInitial;
        set
        {
            localIsInitial = value;
            if (Runner != null)
            {
                networkedIsInitial = value;
            }
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cachedCollider = GetComponent<Collider>();
        localSizeValue = defaultSizeValue;
        SyncMassToSize();

        JellyEffect jelly = GetComponent<JellyEffect>();
        if (jelly != null)
        {
            Destroy(jelly);
        }
    }

    public override void Spawned()
    {
        if (isInitial)
        {
            ApplyInitialAppearance();
        }
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

    public void ApplyInitialAppearance()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(block);
            block.SetColor("_BaseColor", Color.gray);
            block.SetColor("_Color", Color.gray);
            r.SetPropertyBlock(block);
        }

        if (rb != null)
        {
            rb.isKinematic = true;
        }
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
        Debug.Log($"[Collect] OnCollisionEnter via '{collision.gameObject.name}' on collectible size={objectSizeValue:F1}");
        HandlePlayerContact(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Collect] OnTriggerEnter via '{other.gameObject.name}' on collectible size={objectSizeValue:F1}");
        HandlePlayerContact(other.gameObject);
    }

    private void HandlePlayerContact(GameObject contact)
    {
        if (isCollected)
        {
            Debug.Log($"[Collect] EARLY-RETURN isCollected on collectible size={objectSizeValue:F1}");
            return;
        }
        if (Time.time - spawnTime < graceDuration)
        {
            Debug.Log($"[Collect] EARLY-RETURN grace on collectible size={objectSizeValue:F1}");
            return;
        }

        PlayerStats player = contact.GetComponent<PlayerStats>();
        if (player == null)
        {
            Debug.Log($"[Collect] EARLY-RETURN no PlayerStats on '{contact.name}'");
            return;
        }
        if (player.Runner != null && !player.HasStateAuthority)
        {
            Debug.Log($"[Collect] EARLY-RETURN no auth (player '{player.gameObject.name}')");
            return;
        }

        Debug.Log($"[Collect] CHECK player.size={player.playerCurrentSize:F1} vs obj.size={objectSizeValue:F1} → {(player.playerCurrentSize > objectSizeValue ? "ABSORB" : "DAMAGE")}");

        if (player.playerCurrentSize > objectSizeValue)
        {
            isCollected = true;
            if (cachedCollider != null)
            {
                cachedCollider.enabled = false;
            }
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
