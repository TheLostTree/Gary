using System.Globalization;

namespace Gary.Widgets.DamageTracker;

public enum FightProp : uint
{
    None = 0,
    BaseHp = 1,
    FlatHpAdded = 2,
    PercentHp = 3,
    BaseAtk = 4,
    FlatAtkAdded = 5,
    PercentAtk = 6,
    BaseDef = 7,
    FlatDefAdded = 8,
    PercentDef = 9,
    BaseAtkSpeed = 10,
    PercentAtkSpeed = 11,
    FightPropHpMpPercent = 12,
    FightPropAttackMpPercent = 13,
    CritChance = 20,
    ReducedCritDmg = 21,
    CritDamage = 22,
    EnergyRecharge = 23,
    DmgIncrease = 24,
    RecvDmgDecrease = 25,
    HealIncrease = 26,
    RecvHealIncrease = 27,
    ElementalMastery = 28,
    RecvPhysicalDmgDecrease = 29,
    PhysicalDmgIncrease = 30,
    FightPropDefenceIgnoreRatio = 31,
    FightPropDefenceIgnoreDelta = 32,
    PyroDmgIncrease = 40,
    ElectroDmgIncrease = 41,
    HydroDmgIncrease = 42,
    DendroDmgIncrease = 43,
    AnemoDmgIncrease = 44,
    GeoDmgIncrease = 45,
    CryoDmgIncrease = 46,
    WeakpointDmgIncrease = 47,
    RecvPyroDmgReduc = 50,
    RecvElectroDmgReduc = 51,
    RecvHydroDmgReduc = 52,
    RecvDendroDmgReduc = 53,
    RecvAnemoDmgReduc = 54,
    RecvGeoDmgReduc = 55,
    RecvCryoDmgReduc = 56,
    FightPropEffectHit = 60,
    FightPropEffectResist = 61,
    FightPropFreezeResist = 62,
    FightPropTorporResist = 63,
    FightPropDizzyResist = 64,
    FightPropFreezeShorten = 65,
    FightPropTorporShorten = 66,
    FightPropDizzyShorten = 67,
    MaxFireEnergy = 70,
    MaxElectroEnergy = 71,
    MaxHydroEnergy = 72,
    MaxDendroEnergy = 73,
    MaxAnemoEnergy = 74,
    MaxIceEnergy = 75,
    MaxGeoEnergy = 76,
    CooldownReduction = 80,
    FightPropShieldCostMinusRatio = 81,
    CurFireEnergy = 1000,
    CurElectroEnergy = 1001,
    CurHydroEnergy = 1002,
    CurDendroEnergy = 1003,
    CurAnemoEnergy = 1004,
    CurCryoEnergy = 1005,
    CurGeoEnergy = 1006,
    CurHp = 1010,
    MaxHp = 2000,
    CurAtk = 2001,
    CurDef = 2002,
    CurAtkSpeed = 2003,
    FightPropNonextraAttack = 3000,
    FightPropNonextraDefense = 3001,
    FightPropNonextraCritical = 3002,
    FightPropNonextraAntiCritical = 3003,
    FightPropNonextraCriticalHurt = 3004,
    FightPropNonextraChargeEfficiency = 3005,
    FightPropNonextraElementMastery = 3006,
    FightPropNonextraPhysicalSubHurt = 3007,
    FightPropNonextraFireAddHurt = 3008,
    FightPropNonextraElecAddHurt = 3009,
    FightPropNonextraWaterAddHurt = 3010,
    FightPropNonextraGrassAddHurt = 3011,
    FightPropNonextraWindAddHurt = 3012,
    FightPropNonextraRockAddHurt = 3013,
    FightPropNonextraIceAddHurt = 3014,
    FightPropNonextraFireSubHurt = 3015,
    FightPropNonextraElecSubHurt = 3016,
    FightPropNonextraWaterSubHurt = 3017,
    FightPropNonextraGrassSubHurt = 3018,
    FightPropNonextraWindSubHurt = 3019,
    FightPropNonextraRockSubHurt = 3020,
    FightPropNonextraIceSubHurt = 3021,
    FightPropNonextraSkillCdMinusRatio = 3022,
    FightPropNonextraShieldCostMinusRatio = 3023,
    FightPropNonextraPhysicalAddHurt = 3024
}

public static class FightPropUtils
{
    public static string ToString(this FightProp fightProp, float value)
    {
        return MathF.Round(value, 10).ToString(CultureInfo.InvariantCulture);
    }

    public static string GetName(this FightProp fightProp)
    {
        string? returnVal = Enum.GetName(typeof(FightProp), fightProp);
        if (returnVal == null) returnVal = fightProp.ToString();
        return returnVal;
    }
}