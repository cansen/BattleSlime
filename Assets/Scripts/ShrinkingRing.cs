using System.Collections.Generic;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShrinkingRing : NetworkBehaviour
{
    [Header("Match")]
    [SerializeField] private float matchDuration = 120f;

    [Header("Ring")]
    [SerializeField] private float ringStartRadius = 150f;
    [SerializeField] private float ringShrinkInterval = 30f;
    [SerializeField] private float ringShrinkAmount = 50f;
    [SerializeField] private float ringMinRadius = 15f;

    [Header("Damage")]
    [SerializeField] private float ringDamagePerSecond = 5f;
    [SerializeField] private float instantDeathThreshold = 50f;

    [Header("Visual")]
    [SerializeField] private int ringSegments = 64;
    [SerializeField] private float ringVisualHeight = 1f;
    [SerializeField] private float ringLineWidth = 0.5f;
    [SerializeField] private Color ringColor = new Color(0f, 0.8f, 1f, 1f);

    [Networked] private float networkedCurrentRadius { get; set; }
    [Networked] private float networkedMatchTimer { get; set; }
    [Networked] private float networkedShrinkTimer { get; set; }
    [Networked] private float networkedDamageTimer { get; set; }

    private float currentRadius;
    private LineRenderer lineRenderer;
    private ParticleSystem ringParticles;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        ringParticles = GetComponentInChildren<ParticleSystem>();
    }

    public override void Spawned()
    {
        networkedCurrentRadius = ringStartRadius;
        networkedMatchTimer = 0f;
        networkedShrinkTimer = 0f;
        networkedDamageTimer = 0f;
        SetupLineRenderer();
    }

    private void Update()
    {
        if (Runner == null)
        {
            return;
        }
        currentRadius = networkedCurrentRadius;
        UpdateVisuals();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        networkedMatchTimer += Runner.DeltaTime;
        if (networkedMatchTimer >= matchDuration)
        {
            return;
        }

        networkedShrinkTimer += Runner.DeltaTime;
        if (networkedShrinkTimer >= ringShrinkInterval)
        {
            networkedShrinkTimer -= ringShrinkInterval;
            networkedCurrentRadius = Mathf.Max(networkedCurrentRadius - ringShrinkAmount, ringMinRadius);
        }

        networkedDamageTimer += Runner.DeltaTime;
        if (networkedDamageTimer >= 1f)
        {
            networkedDamageTimer -= 1f;
            ApplyRingDamage();
        }
    }

    private void SetupLineRenderer()
    {
        lineRenderer.loop = true;
        lineRenderer.positionCount = ringSegments;
        lineRenderer.startWidth = ringLineWidth;
        lineRenderer.endWidth = ringLineWidth;
        lineRenderer.startColor = ringColor;
        lineRenderer.endColor = ringColor;
        lineRenderer.useWorldSpace = true;
    }

    private void UpdateVisuals()
    {
        DrawRing();
        UpdateParticles();
    }

    private void DrawRing()
    {
        for (int i = 0; i < ringSegments; i++)
        {
            float angle = i * 2f * Mathf.PI / ringSegments;
            float x = transform.position.x + currentRadius * Mathf.Cos(angle);
            float z = transform.position.z + currentRadius * Mathf.Sin(angle);
            lineRenderer.SetPosition(i, new Vector3(x, ringVisualHeight, z));
        }
    }

    private void UpdateParticles()
    {
        if (ringParticles == null)
        {
            return;
        }

        ParticleSystem.ShapeModule shape = ringParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = currentRadius;
    }

    private void ApplyRingDamage()
    {
        List<PlayerStats> outsidePlayers = new List<PlayerStats>();

        for (int i = 0; i < PlayerStats.ActivePlayers.Count; i++)
        {
            PlayerStats player = PlayerStats.ActivePlayers[i];
            if (IsOutsideRing(player.transform.position))
            {
                outsidePlayers.Add(player);
            }
        }

        for (int i = 0; i < outsidePlayers.Count; i++)
        {
            PlayerStats player = outsidePlayers[i];
            if (player.isDead)
            {
                continue;
            }

            if (ringDamagePerSecond >= instantDeathThreshold)
            {
                Debug.Log($"[Ring] {player.gameObject.name} instantly eliminated by ring.");
                player.Die();
            }
            else
            {
                Debug.Log($"[Ring] {player.gameObject.name} took {ringDamagePerSecond} ring damage.");
                player.TakeDamage(ringDamagePerSecond);
            }
        }
    }

    private bool IsOutsideRing(Vector3 position)
    {
        Vector2 flatPosition = new Vector2(position.x, position.z);
        Vector2 flatCenter = new Vector2(transform.position.x, transform.position.z);
        return Vector2.Distance(flatPosition, flatCenter) > networkedCurrentRadius;
    }

    public float CurrentRadius => Runner != null ? currentRadius : ringStartRadius;
    public float MatchTimeRemaining => Runner != null ? Mathf.Max(matchDuration - networkedMatchTimer, 0f) : matchDuration;
    public float TimeUntilNextShrink => Runner != null && currentRadius > ringMinRadius ? Mathf.Max(ringShrinkInterval - networkedShrinkTimer, 0f) : 0f;
}
