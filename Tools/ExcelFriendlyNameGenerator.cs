
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Tools;

public class ExcelFriendlyNameGenerator
{
    public static string RepoLink = """https://gitlab.com/Dimbreath/AnimeGameData/-/tree/master/""";
    public static string RawFilesLink = """https://gitlab.com/Dimbreath/AnimeGameData/-/raw/master/""";

    private static string[] RequiredFiles =
    {
        "TextMap/TextMapEN.json",
        "ExcelBinOutput/AvatarExcelConfigData.json",
        "ExcelBinOutput/GadgetExcelConfigData.json",
        "ExcelBinOutput/MonsterExcelConfigData.json",
        "ExcelBinOutput/GatherExcelConfigData.json",
        "ExcelBinOutput/MaterialExcelConfigData.json",
        "ExcelBinOutput/NpcExcelConfigData.json",
        "ExcelBinOutput/ReliquaryExcelConfigData.json",
        "ExcelBinOutput/WeaponExcelConfigData.json",
    };
    public static Dictionary<uint, string> nameMap = new();
    public static SortedDictionary<uint, string> outputIdList = new SortedDictionary<uint, string>();
    public void Run()
    {
        EnsureResourcesExist();
        nameMap = JsonConvert.DeserializeObject<Dictionary<uint, string>>(File.ReadAllText("Data/TextMapEN.json"))!;
        DoAvatars();
        DoMonsters();
        DoGadgets();

        using (StreamWriter file = new StreamWriter("IdList.csv"))
        {
            foreach (KeyValuePair<uint, string> kvp in outputIdList)
            {
                file.WriteLine($"{kvp.Key},{kvp.Value}");
            }
        }
    }

    private void DoGadgets()
    {
        var fpath = "Data/GadgetExcelConfigData.json";
        List<GadgetExcelConfigEntry> gadgetExcelConfigEntry = JsonConvert.DeserializeObject<List<GadgetExcelConfigEntry>>(File.ReadAllText(fpath))!;
        foreach (GadgetExcelConfigEntry i in gadgetExcelConfigEntry)
        {
            if (i.id != 0)
            {
                string? name = null;
                switch (i.type)
                {
                    case GadgetType.Chest:
                        //idk
                        if (i.interactNameTextMapHash != 0 && i.inteeIconName == "UI_Icon_Intee_TreasureBox" && nameMap.ContainsKey(i.interactNameTextMapHash))
                        {
                            name = nameMap[i.interactNameTextMapHash];
                        } else
                        {
                            goto default;
                        }
                        break;
                    default:
                        if (i.nameTextMapHash != 0 && nameMap.ContainsKey(i.nameTextMapHash))
                        {
                            //name set by TryGetValue
                            name = nameMap[i.nameTextMapHash];
                        }
                        else if (i.jsonName != "")
                        {
                            name = i.jsonName;
                        }

                        break;
                }
                if (string.IsNullOrEmpty(name))
                {
                    name = "Unnamed Gadget";
                }
                outputIdList.Add(i.id, name);
            }
        }
    }

    private void DoMonsters()
    {
        var fpath = "Data/MonsterExcelConfigData.json";
        if (File.Exists(fpath))
        {
            JArray fromFile = JsonConvert.DeserializeObject<JArray>(File.ReadAllText(fpath))!;
            foreach(var jToken in fromFile)
            {
                var from = (JObject)jToken;
                if (from.ContainsKey("id"))
                {
                    uint id = (uint)(from["id"] ?? 0);
                    if (id == 0)
                        continue;

                    string? name = null;
                    if (from.ContainsKey("nameTextMapHash") && nameMap.TryGetValue((uint)from["nameTextMapHash"], out name))
                    {
                        //name set by TryGetValue
                    }
                    else if (from.ContainsKey("monsterName"))
                    {
                        name = (string?)(from["monsterName"] ?? null);
                        if (String.IsNullOrEmpty(name))
                            name = "Unnamed Monster";
                    }
                    if (!String.IsNullOrEmpty(name))
                    {
                        outputIdList.Add(id, name);
                    } else
                    {
                        // Trace.WriteLine($"Dropped un-named Id {from["id"]}");
                    }
                }
            }
        }
    }
    private void DoAvatars()
    {
        var fpath = "Data/AvatarExcelConfigData.json";
        JArray fromFile = JsonConvert.DeserializeObject<JArray>(File.ReadAllText(fpath))!;
        foreach(var jToken in fromFile)
        {
            var from = (JObject)jToken;
            if (!from.ContainsKey("id") || !from.ContainsKey("nameTextMapHash")) continue;
            var id = (uint)(from["id"] ?? 0);
            if (id == 0)
                continue;
            if (nameMap.TryGetValue((uint)(from["nameTextMapHash"] ?? -1), out var name))
            {
                outputIdList.Add(id, name);
            }
        }
    }

    public void EnsureResourcesExist()
    {
        if (!Directory.Exists("./Data"))
        {
            Directory.CreateDirectory("./Data");
        }

        foreach (var filename in RequiredFiles)
        {
            var name = filename.Split("/")[1];
            if (!File.Exists("./Data/" + name))
            {
                string cmd = $"\"{RawFilesLink}{filename}\" -s -o ./Data/{name}";
                System.Diagnostics.Process.Start("curl.exe",cmd);
            }
        }
    }
    

}

public enum GadgetType
{
    UNKNOWN,
    Gear,
    Field,
    Bullet,
    Gadget,
    AirflowField,
    SpeedupField,
    GatherObject,
    Platform,
    MonsterEquip,
    Equip,
    SubEquip,
    Grass,
    Water,
    Tree,
    Bush,
    Lightning,
    CustomTile,
    TransPointFirst,
    TransPointSecond,
    Chest,
    GeneralRewardPoint,
    AmberWind,
    OfferingGadget,
    Worktop,
    CustomGadget,
    BlackMud,
    RoguelikeOperatorGadget,
    NightCrowGadget,
    MpPlayRewardPoint,
    DeshretObeliskGadget,
    RewardStatue,
    RewardPoint,
    Foundation,
    Projector,
    Screen,
    CoinCollectLevelGadget,
    EchoShell,
    GatherPoint,
    MiracleRing,
    EnvAnimal,
    EnergyBall,
    SealGadget,
    ViewPoint,
    QuestGadget,
    FishPool,
    ElemCrystal,
    WindSeed,
    Camera,
    UIInteractGadget,
    Vehicle,
    FishRod
}

public class GadgetExcelConfigEntry
{
    [JsonProperty] public readonly uint id;
    [JsonProperty] public readonly uint nameTextMapHash;
    [JsonProperty] public readonly uint interactNameTextMapHash;
    [JsonProperty] public readonly string jsonName;
    [JsonProperty] public readonly string itemJsonName;
    [JsonProperty] public readonly string inteeIconName;
    [JsonProperty, JsonConverter(typeof(StringEnumConverter))] public readonly GadgetType type;
    //[JsonProperty] public readonly string type;
}