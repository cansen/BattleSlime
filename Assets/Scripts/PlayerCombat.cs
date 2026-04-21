using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCombat : MonoBehaviour
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
        PlayerCombat otherCombat = collision.gameObject.GetComponent<PlayerCombat>();
        if (otherCombat == null)
        {
            return;
        }

        if (GetInstanceID() < collision.gameObject.GetInstanceID())
        {
            return;
        }

        ResolveCollision(otherCombat);
    }

    private void ResolveCollision(PlayerCombat otherCombat)
    {
        if (CheckInstantElimination(otherCombat.stats))
        {
            return;
        }

        ApplyDamage(otherCombat);
    }

    private bool CheckInstantElimination(PlayerStats otherStats)
    {
        float mySize = stats.playerCurrentSize;
        float otherSize = otherStats.playerCurrentSize;

        if (mySize / otherSize >= eliminationSizeRatio)
        {
            Debug.Log($"[Combat] {otherStats.gameObject.name} instantly eliminated by size ratio.");
            stats.Grow(otherSize * absorptionMultiplier);
            otherStats.Die();
            return true;
        }

        if (otherSize / mySize >= eliminationSizeRatio)
        {
            Debug.Log($"[Combat] {gameObject.name} instantly eliminated by size ratio.");
            otherStats.Grow(mySize * absorptionMultiplier);
            stats.Die();
            return true;
        }

        return false;
    }

    private void ApplyDamage(PlayerCombat otherCombat)
    {
        PlayerStats otherStats = otherCombat.stats;

        float myVelocity = GetEffectiveVelocity();
        float otherVelocity = otherCombat.GetEffectiveVelocity();

        int myMomentum = CombatFormula.CalculateMomentum(stats.playerCurrentSize, myVelocity, sizeMultiplier, velocityMultiplier, velocityConstant);
        int otherMomentum = CombatFormula.CalculateMomentum(otherStats.playerCurrentSize, otherVelocity, sizeMultiplier, velocityMultiplier, velocityConstant);

        int damageToOther = CombatFormula.CalculateDamage(myMomentum, otherMomentum, damageMultiplier, damageConstant);
        int damageToMe = CombatFormula.CalculateDamage(otherMomentum, myMomentum, damageMultiplier, damageConstant);

        Debug.Log($"[Combat] {gameObject.name} — Size: {stats.playerCurrentSize:F1}, Velocity: {myVelocity:F2}, Momentum: {myMomentum} | Damage dealt: {damageToOther}");
        Debug.Log($"[Combat] {otherStats.gameObject.name} — Size: {otherStats.playerCurrentSize:F1}, Velocity: {otherVelocity:F2}, Momentum: {otherMomentum} | Damage dealt: {damageToMe}");

        float mySizeBeforeDamage = stats.playerCurrentSize;
        float otherSizeBeforeDamage = otherStats.playerCurrentSize;

        stats.TakeDamage(damageToMe);
        otherStats.TakeDamage(damageToOther);

        bool iDied = stats.isDead;
        bool otherDied = otherStats.isDead;

        if (otherDied && !iDied)
        {
            stats.Grow(otherSizeBeforeDamage * absorptionMultiplier);
        }
        else if (iDied && !otherDied)
        {
            otherStats.Grow(mySizeBeforeDamage * absorptionMultiplier);
        }

        if (!iDied && !otherDied)
        {
            ApplyKnockback(myMomentum, otherMomentum, otherCombat.rigidBody);
        }
    }

    private void ApplyKnockback(int myMomentum, int otherMomentum, Rigidbody otherRigidbody)
    {
        Vector3 direction = (otherRigidbody.position - rigidBody.position).normalized;
        rigidBody.AddForce(-direction * knockbackForce, ForceMode.Impulse);
        otherRigidbody.AddForce(direction * knockbackForce, ForceMode.Impulse);
    }

    public float GetEffectiveVelocity()
    {
        return Mathf.Max(rigidBody.linearVelocity.magnitude, minimumVelocity);
    }
}
