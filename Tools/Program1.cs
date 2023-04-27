using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace Tools;

public class Program1
{
    private ConcurrentBag<string> names = new ConcurrentBag<string>();
    public void Run()
    {
        String path = """C:\Users\admin\Downloads\binoutputraw3.6\Persistent\MiHoYoBinData""";
        // Console.WriteLine(MakeEmbryo("Avatar_Itto_ElementalArt_CreateGadget") == 121507964);
        // foreach (var enumerateFile in Directory.EnumerateFiles(path))
        // {
        //     DoStuff(enumerateFile);
        // }

        Parallel.ForEach(Directory.EnumerateFiles(path), s =>
        {
            DoStuff(s);
        });
        
        Console.WriteLine("found " + names.Count + " strings lol");

        StringBuilder sb = new StringBuilder();
        foreach (var str in names.ToHashSet())
        {
            sb.Append(str).Append(",").Append(MakeEmbryo(str)).AppendLine();
        }
        
        File.WriteAllText("Embryos.csv",sb.ToString());
        Console.WriteLine(Directory.GetCurrentDirectory() + "/" + "Embryos.csv");
        // DoStuff("""C:\Users\admin\Downloads\binoutputraw3.6\Persistent\MiHoYoBinData\44935541.txt""");


    }

    // start with letter, rest can be letter, number, etc
    public static Regex regex1 = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");


    public void DoStuff(string path)
    {
        var stream = File.OpenRead(path);
        Span<byte> bytes = new byte[(int)stream.Length];

        var read = stream.Read(bytes);
        if (read != bytes.Length)
        {
            throw new Exception("wtf");
        }
        int pos = 4;
        
        
        while(true)
        {
            int len = pos >= bytes.Length ? short.MaxValue : bytes[pos];
            if (len == 0)
            {
                pos++;
                continue;
            }
            if (pos + 1 + len > bytes.Length)
            {
                break;
            }
            Span<byte> slc = bytes.Slice(pos + 1, len);

            var str = Encoding.Default.GetString(slc);

            if (regex1.IsMatch(str))
            {
                names.Add(str);
                pos += len;
            }
            else
            {
                pos++;
            }
        }
    }

    public uint MakeEmbryo(string name)
    {
        uint h = 0;
        for (var i = 0; i < name.Length; i++)
        {
            h = 0x83 * h + name[i];
            h &= 0xffffffff;
        }

        return h;
    }
}