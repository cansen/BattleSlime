using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [Header("Movement")]
    public float playerBaseMovementSpeed = 12f;
    public float playerSizeMovementConstant = 0.333f;
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
    public float massPerSize = 1f;

    [Header("Spawn")]
    [SerializeField] private float groundLevel = 0.42f;
    [SerializeField] private GameObject miniSpherePrefab;
    [SerializeField] private int maxMiniSpheresPerHit = 10;
    [SerializeField] private float miniSphereScatterForce = 8f;
    [SerializeField] private float miniSphereGraceDuration = 1.5f;

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
    private Rigidbody cachedRigidbody;

    private void Awake()
    {
        jellyEffect = GetComponent<JellyEffect>();
        cachedRigidbody = GetComponent<Rigidbody>();
        localSize = playerStartSize;
        transform.localScale = Vector3.one * CalculateVisualScale();
        SyncMassToSize();
        jellyEffect?.RefreshBaseScale();
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            isLocalPlayer = true;
            playerCurrentSize = playerStartSize;
            CameraController cam = Camera.main?.GetComponent<CameraController>();
            cam?.SetTarget(transform);

            if (cachedRigidbody != null)
            {
                Vector3 pos = cachedRigidbody.position;
                pos.y = groundLevel + CalculateVisualScale() * 0.5f;
                cachedRigidbody.position = pos;
            }
        }

        transform.localScale = Vector3.one * CalculateVisualScale();
        SyncMassToSize();
        jellyEffect?.RefreshBaseScale();
    }

    public override void Render()
    {
        if (playerCurrentSize > 0f)
        {
            float scale = CalculateVisualScale();
            if (jellyEffect != null)
            {
                jellyEffect.SetBaseSize(Vector3.one * scale);
            }
            else
            {
                transform.localScale = Vector3.one * scale;
            }
        }
    }

    private void OnEnable() => activePlayers.Add(this);
    private void OnDisable() => activePlayers.Remove(this);

    public void Grow(float amount)
    {
        float oldScale = CalculateVisualScale();
        playerCurrentSize = Mathf.Min(playerCurrentSize + amount, playerMaxSize);
        float newScale = CalculateVisualScale();
        transform.localScale = Vector3.one * newScale;

        if (cachedRigidbody != null)
        {
            Vector3 pos = cachedRigidbody.position;
            pos.y += (newScale - oldScale) * 0.5f;
            cachedRigidbody.position = pos;
        }

        SyncMassToSize();
        jellyEffect?.RefreshBaseScale();
    }

    public void TakeDamage(float damage)
    {
        playerCurrentSize -= damage;

        // Spawn mini spheres for lost health
        if (damage > 0f && miniSpherePrefab != null)
        {
            int numSpheres = Mathf.Clamp(Mathf.CeilToInt(damage), 1, maxMiniSpheresPerHit);
            float valuePerSphere = damage / numSpheres;
            for (int i = 0; i < numSpheres; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 2f;
                offset.y = 0f;
                Vector3 spawnPos = transform.position + offset;
                GameObject sphere = Instantiate(miniSpherePrefab, spawnPos, Quaternion.identity);
                CollectibleObject co = sphere.GetComponent<CollectibleObject>();
                if (co != null)
                {
                    co.objectSizeValue = valuePerSphere;
                    co.canDamagePlayer = false;
                    co.ScheduleDespawn(10f);
                    co.SetGracePeriod(miniSphereGraceDuration);
                }

                Rigidbody sphereRb = sphere.GetComponent<Rigidbody>();
                if (sphereRb != null)
                {
                    Vector3 scatterDir = offset.sqrMagnitude > 0.001f ? offset.normalized : Random.onUnitSphere;
                    scatterDir.y = Mathf.Abs(scatterDir.y) + 0.3f;
                    sphereRb.AddForce(scatterDir.normalized * miniSphereScatterForce, ForceMode.Impulse);
                }
            }
        }

        if (playerCurrentSize <= playerInstantDeathSize)
        {
            Die();
            return;
        }

        transform.localScale = Vector3.one * CalculateVisualScale();
        SyncMassToSize();
        jellyEffect?.RefreshBaseScale();
    }

    private void SyncMassToSize()
    {
        if (cachedRigidbody == null)
        {
            return;
        }
        cachedRigidbody.mass = Mathf.Max(playerCurrentSize * massPerSize, 0.0001f);
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
