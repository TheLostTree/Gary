namespace Gary.Widgets.DamageTracker;

public class DamageEvent
{
    public double damage;
    public double healed;
    public uint attackerId;

    public ulong nowMs;

    public String abilitySource;
}