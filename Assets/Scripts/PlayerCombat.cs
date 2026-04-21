using Fusion;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCombat : NetworkBehaviour
{
    [Header("Formula Parameters")]
    [SerializeField] private float sizeMultiplier = 1f;
    [SerializeField] private float velocityMultiplier = 1f;
    [SerializeField] private float velocityConstant = 0.1f;
    [SerializeField] private float damageMultiplier = 0.005f;
    [SerializeField] private float damageConstant = 5000f;
    [SerializeField] private float minimumVelocity = 0.1f;

    [Header("Elimination")]
    [SerializeField] private float eliminationSizeRatio = 2f;
    [SerializeField] private float absorptionMultiplier = 1f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 15f;

    private PlayerStats stats;
    private Rigidbody rigidBody;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        rigidBody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        PlayerCombat opponentCombat = collision.gameObject.GetComponent<PlayerCombat>();
        if (opponentCombat == null)
        {
            return;
        }

        ResolveIncomingDamage(opponentCombat);
    }

    private void ResolveIncomingDamage(PlayerCombat opponent)
    {
        float mySize = stats.playerCurrentSize;
        float opponentSize = opponent.stats.playerCurrentSize;

        if (opponentSize / mySize >= eliminationSizeRatio)
        {
            Debug.Log($"[Combat] {gameObject.name} instantly eliminated by size ratio.");
            opponent.RpcGrow(mySize * absorptionMultiplier);
            stats.Die();
            return;
        }

        if (mySize / opponentSize >= eliminationSizeRatio)
        {
            return;
        }

        ApplySelfDamage(opponent);
    }

    private void ApplySelfDamage(PlayerCombat opponent)
    {
        PlayerStats opponentStats = opponent.stats;
        float myVelocity = GetEffectiveVelocity();
        float opponentVelocity = opponent.GetEffectiveVelocity();

        int myMomentum = CombatFormula.CalculateMomentum(stats.playerCurrentSize, myVelocity, sizeMultiplier, velocityMultiplier, velocityConstant);
        int opponentMomentum = CombatFormula.CalculateMomentum(opponentStats.playerCurrentSize, opponentVelocity, sizeMultiplier, velocityMultiplier, velocityConstant);

        int damageToMe = CombatFormula.CalculateDamage(opponentMomentum, myMomentum, damageMultiplier, damageConstant);

        Debug.Log($"[Combat] {gameObject.name} — Size: {stats.playerCurrentSize:F1}, Velocity: {myVelocity:F2}, Momentum: {myMomentum} | Takes: {damageToMe}");

        float mySizeBeforeDamage = stats.playerCurrentSize;
        stats.TakeDamage(damageToMe);

        if (stats.isDead)
        {
            opponent.RpcGrow(mySizeBeforeDamage * absorptionMultiplier);
            return;
        }

        ApplySelfKnockback(opponent.rigidBody);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcGrow(float amount)
    {
        stats.Grow(amount);
    }

    private void ApplySelfKnockback(Rigidbody opponentRigidbody)
    {
        Vector3 direction = (rigidBody.position - opponentRigidbody.position).normalized;
        rigidBody.AddForce(direction * knockbackForce, ForceMode.Impulse);
    }

    public float GetEffectiveVelocity()
    {
        return Mathf.Max(rigidBody.linearVelocity.magnitude, minimumVelocity);
    }
}
