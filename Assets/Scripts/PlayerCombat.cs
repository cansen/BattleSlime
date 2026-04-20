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
    [SerializeField] private int instantDeathThreshold = 50;
    [SerializeField] private float minimumVelocity = 0.1f;

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

        LogCollision(otherCombat);
    }

    private void LogCollision(PlayerCombat otherCombat)
    {
        PlayerStats otherStats = otherCombat.stats;

        float myVelocity = GetEffectiveVelocity();
        float otherVelocity = otherCombat.GetEffectiveVelocity();

        int myMomentum = CombatFormula.CalculateMomentum(stats.playerCurrentSize, myVelocity, sizeMultiplier, velocityMultiplier, velocityConstant);
        int otherMomentum = CombatFormula.CalculateMomentum(otherStats.playerCurrentSize, otherVelocity, sizeMultiplier, velocityMultiplier, velocityConstant);

        int damageToOther = CombatFormula.CalculateDamage(myMomentum, otherMomentum, damageMultiplier, damageConstant);
        int damageToMe = CombatFormula.CalculateDamage(otherMomentum, myMomentum, damageMultiplier, damageConstant);

        Debug.Log($"[Combat] {gameObject.name} — Size: {stats.playerCurrentSize}, Velocity: {myVelocity:F2}, Momentum: {myMomentum} | Damage dealt: {damageToOther}");
        Debug.Log($"[Combat] {otherStats.gameObject.name} — Size: {otherStats.playerCurrentSize}, Velocity: {otherVelocity:F2}, Momentum: {otherMomentum} | Damage dealt: {damageToMe}");
    }

    public float GetEffectiveVelocity()
    {
        return Mathf.Max(rigidBody.linearVelocity.magnitude, minimumVelocity);
    }
}
