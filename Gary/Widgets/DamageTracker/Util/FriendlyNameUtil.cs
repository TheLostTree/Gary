namespace Gary.Widgets.DamageTracker.Util;

public class FriendlyNameUtil
{
    public static Dictionary<uint, string> FriendlyNames = GetFriendlyNameLookups();

    private static Dictionary<uint, string> GetFriendlyNameLookups()
    {
        var dict = new Dictionary<uint, string>();
        foreach (var line in File.ReadAllLines("./IdList.csv"))
        {
            if(!line.Contains(',')) continue;

            var split = line.Split(",", 2);
            
            dict[uint.Parse(split[0])]= split[1];
        }

        return dict;
    }
    
    
}