using Godot;
using System;
using Scribe.Scripts.Core.Interfaces;
using Scribe.Scripts.Data.ComponentData;

namespace Scribe.Scripts.Core.Components;

public partial class MeleeAttackComponent : Component, IMeleeAttacker
{
    private MeleeAttackComponentData _data;
    private readonly Random _random = new();
    
    public int AttackBonus { get; private set; }
    public int NumDice { get; private set; }
    public DieType DieType { get; private set; }
    public int DamageBonus { get; private set; }
    public DamageType DamageType { get; private set; }
    public int MeleeRange { get; private set; }
    public string AttackName { get; private set; }
    
    public void Initialize(MeleeAttackComponentData data)
    {
        _data = data;
        AttackBonus = data.AttackBonus;
        NumDice = data.NumDice;
        DieType = data.DieType;
        DamageBonus = data.DamageBonus;
        DamageType = data.DamageType;
        MeleeRange = data.Range;
        AttackName = data.AttackName;
    }
    
    public override void Initialize()
    {
    }
    
    public AttackResult MeleeAttack(IDamageable target)
    {
        int d20Roll = RollD20();
        int attackRoll = d20Roll + AttackBonus;
        bool isCritical = d20Roll == 20;
        bool isCriticalMiss = d20Roll == 1;
        
        if (isCriticalMiss)
        {
            return new AttackResult(false, attackRoll, 0, false);
        }
        
        bool hit = isCritical || attackRoll >= target.ArmorClass;
        
        if (!hit)
        {
            return new AttackResult(false, attackRoll, 0, false);
        }
        
        int damage = RollDamage(isCritical);
        target.TakeDamage(damage, DamageType);
        
        return new AttackResult(true, attackRoll, damage, isCritical);
    }
    
    private int RollD20()
    {
        return _random.Next(1, 21);
    }
    
    private int RollDamage(bool critical)
    {
        int totalDamage = 0;
        int rolls = critical ? 2 : 1;
        int dieSize = (int)DieType;
        
        for (int r = 0; r < rolls; r++)
        {
            for (int i = 0; i < NumDice; i++)
            {
                totalDamage += _random.Next(1, dieSize + 1);
            }
        }
        
        totalDamage += DamageBonus;
        
        return totalDamage;
    }
}