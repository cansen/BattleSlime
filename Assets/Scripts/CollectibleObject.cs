using UnityEngine;

public class CollectibleObject : MonoBehaviour
{
    [SerializeField] public float objectSizeValue = 1f;
    [SerializeField] private float stationaryVelocity = 0.1f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 15f;

    [Header("Formula Parameters")]
    [SerializeField] private float sizeMultiplier = 1f;
    [SerializeField] private float velocityMultiplier = 1f;
    [SerializeField] private float velocityConstant = 0.1f;
    [SerializeField] private float damageMultiplier = 0.005f;
    [SerializeField] private float damageConstant = 5000f;

    private bool isCollected;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isCollected) return;

        PlayerStats player = collision.gameObject.GetComponent<PlayerStats>();
        if (player == null) return;

        if (player.playerCurrentSize > objectSizeValue)
        {
            isCollected = true;
            player.Grow(objectSizeValue);
            Destroy(gameObject);
        }
        else
        {
            ApplyCombatDamage(player);
        }
    }

    private void ApplyCombatDamage(PlayerStats player)
    {
        PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
        float playerVelocity = playerCombat != null ? playerCombat.GetEffectiveVelocity() : stationaryVelocity;

        float collectibleVelocity = rb != null ? Mathf.Max(rb.linearVelocity.magnitude, stationaryVelocity) : stationaryVelocity;

        int playerMomentum = CombatFormula.CalculateMomentum(player.playerCurrentSize, playerVelocity, sizeMultiplier, velocityMultiplier, velocityConstant);
        int collectibleMomentum = CombatFormula.CalculateMomentum(objectSizeValue, collectibleVelocity, sizeMultiplier, velocityMultiplier, velocityConstant);

        int damageToPlayer = CombatFormula.CalculateDamage(collectibleMomentum, playerMomentum, damageMultiplier, damageConstant);

        Debug.Log($"[Combat] {player.gameObject.name} hit collectible (size {objectSizeValue}) — PlayerMomentum: {playerMomentum}, CollectibleMomentum: {collectibleMomentum}, Damage: {damageToPlayer}");

        player.TakeDamage(damageToPlayer);

        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
        if (playerRigidbody != null)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            playerRigidbody.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
    }
}
