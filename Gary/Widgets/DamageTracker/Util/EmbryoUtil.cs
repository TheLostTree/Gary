using Newtonsoft.Json;

namespace Gary.Widgets.DamageTracker.Util;

public class EmbryoUtil
{
    public static Dictionary<uint, string> Embryos = GetEmbryoLookups();

    private static Dictionary<uint, string> GetEmbryoLookups()
    {
        var dict = new Dictionary<uint, string>();
        foreach (var line in File.ReadAllLines("./Embryos.csv"))
        {
            if(!line.Contains(',')) continue;

            var split = line.Split(",", 2);
            if (split.Length != 2)
            {
                Console.WriteLine(":(");
                continue;
            }
            dict[uint.Parse(split[1])] = split[0];
        }

        return dict;
    }
}