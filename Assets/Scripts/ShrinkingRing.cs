using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShrinkingRing : MonoBehaviour
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

    private float currentRadius;
    private float matchTimer;
    private float shrinkTimer;
    private float damageTimer;

    private LineRenderer lineRenderer;
    private ParticleSystem ringParticles;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        ringParticles = GetComponentInChildren<ParticleSystem>();
    }

    private void Start()
    {
        currentRadius = ringStartRadius;
        SetupLineRenderer();
        UpdateVisuals();
    }

    private void Update()
    {
        matchTimer += Time.deltaTime;
        if (matchTimer >= matchDuration)
        {
            return;
        }

        shrinkTimer += Time.deltaTime;
        if (shrinkTimer >= ringShrinkInterval)
        {
            shrinkTimer -= ringShrinkInterval;
            currentRadius = Mathf.Max(currentRadius - ringShrinkAmount, ringMinRadius);
            UpdateVisuals();
            Debug.Log($"[Ring] Shrunk to radius {currentRadius}");
        }

        damageTimer += Time.deltaTime;
        if (damageTimer >= 1f)
        {
            damageTimer -= 1f;
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
        return Vector2.Distance(flatPosition, flatCenter) > currentRadius;
    }

    public float CurrentRadius => currentRadius;
}
