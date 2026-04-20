using NUnit.Framework;

public class CombatFormulaTests
{
    private const float SizeMultiplier = 1f;
    private const float VelocityMultiplier = 1f;
    private const float VelocityConstant = 0.1f;
    private const float DamageMultiplier = 0.005f;
    private const float DamageConstant = 5000f;

    [Test]
    public void CalculateMomentum_CsvChar1_ReturnsExpected()
    {
        int momentum = CombatFormula.CalculateMomentum(100f, 100f, SizeMultiplier, VelocityMultiplier, VelocityConstant);
        Assert.AreEqual(10010, momentum);
    }

    [Test]
    public void CalculateMomentum_CsvChar2_ReturnsExpected()
    {
        int momentum = CombatFormula.CalculateMomentum(51f, 100f, SizeMultiplier, VelocityMultiplier, VelocityConstant);
        Assert.AreEqual(5105, momentum);
    }

    [Test]
    public void CalculateDamage_Char1AttacksChar2_Returns50()
    {
        int damage = CombatFormula.CalculateDamage(10010, 5105, DamageMultiplier, DamageConstant);
        Assert.AreEqual(50, damage);
    }

    [Test]
    public void CalculateDamage_Char2AttacksChar1_Returns1()
    {
        int damage = CombatFormula.CalculateDamage(5105, 10010, DamageMultiplier, DamageConstant);
        Assert.AreEqual(1, damage);
    }

    [Test]
    public void CalculateMomentum_ZeroVelocity_UsesConstantOnly()
    {
        int momentum = CombatFormula.CalculateMomentum(100f, 0f, SizeMultiplier, VelocityMultiplier, VelocityConstant);
        Assert.AreEqual(10, momentum);
    }

    [Test]
    public void CalculateDamage_EqualMomentum_ReturnsMinimumFloor()
    {
        int damage = CombatFormula.CalculateDamage(5000, 5000, DamageMultiplier, DamageConstant);
        Assert.AreEqual(25, damage);
    }
}
