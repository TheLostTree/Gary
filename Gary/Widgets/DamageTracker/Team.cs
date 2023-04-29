using Gary.Widgets.DamageTracker.Entity;

namespace Gary.Widgets.DamageTracker;

public class Team
{
    public List<AvatarEntity> avatars = new List<AvatarEntity>();

    public double totalDamage;


    public bool isEncounterStarted = false;

    private ulong encounterStartedMs = 0;

    private ulong lastDamageInstanceMs = 0;

    public ulong encounterLengthMs => lastDamageInstanceMs - encounterStartedMs;

    public void Set(List<AvatarEntity> newTeam)
    {
        
        Reset();
        avatars.Clear();
        avatars.AddRange(newTeam);


    }

    public void Reset()
    {
        // return;
        avatars.ForEach(x=>x.Reset());
        totalDamage = 0;
        encounterStartedMs = 0;
        isEncounterStarted = false;
        
        //todo: log something to a file idk
    }

    public void AddDamageEvent(DamageEvent damageEvent)
    {
        if (!isEncounterStarted)
        {
            isEncounterStarted = true;
            encounterStartedMs = damageEvent.nowMs;
        }
        var avatar = avatars.Find(x => x.EntityId == damageEvent.attackerId);
        if (avatar is null)
        {
            Console.Write($"avatar with id is null: {damageEvent.attackerId}");
            return;
        }

        totalDamage += damageEvent.damage;
        avatar.totalDamageDealt += damageEvent.damage;

        avatar.totalHealed += damageEvent.healed;
        lastDamageInstanceMs = damageEvent.nowMs;


    }
}