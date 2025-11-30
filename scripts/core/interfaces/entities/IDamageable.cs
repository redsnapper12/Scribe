namespace Scribe.Scripts.Core.Interfaces;

public interface IDamageable
{
    int CurrentHP { get; }
    int MaxHP { get; }
    int ArmorClass { get; }
    bool IsAlive { get; }

    void TakeDamage(int amount, DamageType damageType = DamageType.Bludgeoning);
    void Heal(int amount);
}

public enum DamageType
{
    Bludgeoning,
    Piercing,
    Slashing,
    Fire,
    Cold,
    Lightning,
    Thunder,
    Acid,
    Poison,
    Necrotic,
    Radiant,
    Force,
    Psychic
}