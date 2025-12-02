#region Enums and Types

using System;
using Godot;

public enum DiceRollCategory
{
    CriticalFailure,
    Standard,
    CriticalSuccess
}

public enum DieType
{
    D4 = 4,
    D6 = 6,
    D8 = 8,
    D10 = 10,
    D12 = 12,
    D20 = 20,
    D100 = 100
}

public struct DiceRollRequest
{
    public DieType DieType;
    public int Count;
    public int Modifier;

    public DiceRollRequest(DieType dieType, int count, int modifier)
    {
        DieType = dieType;
        Count = count;
        Modifier = modifier;
    }
}

public struct DiceRollResult
{
    public DiceRollCategory DiceRollCategory;
    public int BaseRoll;
    public int Result;

    public static DiceRollResult CriticallySucceeded(int baseRoll, int result)
    {
        return new DiceRollResult
        {
            DiceRollCategory = DiceRollCategory.CriticalSuccess,
            BaseRoll = baseRoll,
            Result = result
        };
    }
    
    public static DiceRollResult CriticallyFailed(int baseRoll, int result)
    {
        return new DiceRollResult
        {
            DiceRollCategory = DiceRollCategory.CriticalFailure,
            BaseRoll = baseRoll,
            Result = result
        };
    }

    public static DiceRollResult Success(int baseRoll, int result)
    {
        return new DiceRollResult
        {
            DiceRollCategory = DiceRollCategory.Standard,
            BaseRoll = baseRoll,
            Result = result
        };
    }
}

#endregion

public static class DiceRollingService
{
    private static Random _random;
    private static bool _initialized = false;
    
    public static void Initialize()
    {
        _random = new();
        _initialized = true;
    }
    
    public static DiceRollResult RequestRoll(DiceRollRequest request)
    {
        if (!_initialized)
        {
            GD.PrintErr("DiceRollingService not initialized!");
            return new DiceRollResult();
        }

        int roll = RollDie(request.DieType);

        if (request.DieType == DieType.D20)
        {
            if(roll == 20)
            {
                return DiceRollResult.CriticallySucceeded(roll, roll + request.Modifier);
            } 
            else if(roll == 1)
            {
                return DiceRollResult.CriticallyFailed(roll, roll + request.Modifier);
            }
        }

        return DiceRollResult.Success(roll, roll + request.Modifier);
    }

    private static int RollDie(DieType dieType)
    {
        return _random.Next(1, (int)(dieType + 1));
    }
}