using System;

public static class CombatFormula
{
    public static int CalculateMomentum(float size, float velocity, float sizeMultiplier, float velocityMultiplier, float velocityConstant)
    {
        return (int)((size * sizeMultiplier) * ((velocity * velocityMultiplier) + velocityConstant));
    }

    public static int CalculateDamage(int attackerMomentum, int defenderMomentum, float damageMultiplier, float damageConstant)
    {
        float diff = attackerMomentum - defenderMomentum;
        return (int)Math.Ceiling((diff + damageConstant) * damageMultiplier);
    }
}
