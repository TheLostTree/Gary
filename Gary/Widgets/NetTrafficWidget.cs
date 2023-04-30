using System.Collections;
using System.Globalization;
using System.Reflection;
using DNToolKit.AnimeGame;
using Gary.Interfaces;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using ImGuiNET;
using Veldrid.Sdl2;

namespace Gary.Widgets;

using ui = ImGuiNET.ImGui;

public class NetTrafficWidget : IPacketConsumer, IWidget
{
    private List<AnimeGamePacket> _gamePackets = new();
    private List<(string, string, string, string)> processed = new();
    private int count = 0;

    public NetTrafficWidget()
    {
        WidgetName = "Packet Viewer";
    }

    private ImGuiTableFlags _tableFlags = ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable;
    private int focusedPacketTuple = -1;

    private static readonly Dictionary<Type, Dictionary<object, string>> dictionaries = new();

    private bool showNullMessages = true;
    
    private string searchText = String.Empty;

    private void TreeOutType(object? value)
    {
        // JsonFormatter.Default.Format()
        // I STOLE THIS FROM THE JSON FORMATTER LMAOOO
        if (value is IMessage message)
        {
            TreeOutMessage(message);
        }
        else if (value is null)
        {
            ui.Text("null");
        }
        else if (value is bool b)
        {
            ui.Text(b.ToString());
        }
        else if (value is ByteString byteString)
        {
            ui.Text(byteString.ToBase64());
        }
        else if (value is string s)
        {
            if (s.Length != 0)
            {
                ui.Text(s);
            }
        }
        else if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                if (ui.TreeNode(entry.Key.ToString()))
                {
                    TreeOutType(entry.Value);
                    ui.TreePop();
                }
            }
        }
        else if (value is IList list)
        {
            int i = 0;
            foreach (var item in list)
            {
                if (ui.TreeNode(i.ToString()))
                {
                    TreeOutType(item);
                    ui.TreePop();
                }

                i++;
            }
        }
        else if (value is int || value is uint || value is long || value is ulong)
        {
            IFormattable formattable = (IFormattable)value;
            ui.Text(formattable.ToString("d", CultureInfo.InvariantCulture));
        }
        else if (value is Enum)
        {
            var enumType = value.GetType();
            
            // lol!
            Dictionary<object, string> nameMapping;
            if (!dictionaries.TryGetValue(enumType, out nameMapping))
            {
                nameMapping = enumType.GetTypeInfo().DeclaredFields.Where(f => f.IsStatic)
                    .Where(f => f.GetCustomAttributes<OriginalNameAttribute>().FirstOrDefault()?.PreferredAlias ?? true)
                    .ToDictionary(f => f.GetValue(null), f => f.GetCustomAttributes<OriginalNameAttribute>()
                        .FirstOrDefault()
                        // If the attribute hasn't been applied, fall back to the name of the field.
                        ?.Name ?? f.Name);
                dictionaries[enumType] = nameMapping;
            }

            string originalName = value.ToString();
            nameMapping.TryGetValue(value, out originalName);
            ui.Text(originalName);
        }
        else if (value is float || value is double)
        {
            ui.Text(((IFormattable)value).ToString("r", CultureInfo.InvariantCulture));
        }
    }

    private void TreeOutMessage(IMessage msg)
    {
        foreach (var p in msg.Descriptor.Fields.InDeclarationOrder())
        {
            if (ui.TreeNode(p.JsonName))
            {
                var accessor = p.Accessor;
                var value = accessor.GetValue(msg);
                TreeOutType(value);

                //"number" vals
                //string vals
                //array vals
                //other nested objects
                ui.TreePop();
            }
        }
    }


    private string title = "Data Viewer###DATAVIEWERWINDOW";
    private bool updated = false;
    private bool scrollToBottom = false;

    private static string messageIsNullText = "Message is null (probably missing definition)";
    public void DoUI()
    {
        
        
        if (ui.Begin("Packet Viewer"))
        {

            ui.Checkbox("Show null items", ref showNullMessages);
            
            ui.SameLine();
            ui.Checkbox("Scroll to Bottom", ref scrollToBottom);
            
            
            ui.Text("Search");
            ui.InputText("inputboxsearchlol", ref searchText, 100);
            
            if (ui.BeginTable("packet view table", 4, _tableFlags))
            {
                
                ui.TableSetupScrollFreeze(0, 1);
                ui.TableSetupColumn("#");
                ui.TableSetupColumn("Sender");
                ui.TableSetupColumn("CmdId");
                ui.TableSetupColumn("Data");
                ui.TableHeadersRow();
                unsafe
                {
                    // ImGuiListClipper clipper = new();
                
                
                    ImGuiListClipperPtr ptr = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
                    ptr.Begin(processed.Count);

                    while (ptr.Step())
                    {
                        for (int row = ptr.DisplayStart; row < ptr.DisplayEnd; row++)
                        {
                            var (a1, a2, a3, a4) = processed[row];
                            if (a4.Equals(messageIsNullText) && !showNullMessages)
                            {
                                continue;
                            }
                    

                            if (searchText != String.Empty)
                            {
                                //filter by opcode basically?
                                if(!a3.Contains(searchText)) continue;
                            }
                            ui.TableNextRow();
                            // var item = processed[row];
                            List<string> f = new List<string>() { a1, a2, a3, a4 };
                            int i = 0;
                            foreach (string s in f)
                            {
                                ui.TableSetColumnIndex(i);
                                if (ui.Selectable(s, row == focusedPacketTuple, ImGuiSelectableFlags.SpanAllColumns))
                                {
                                    focusedPacketTuple = row;
                                    updated = false;
                                    Sdl2Native.SDL_SetClipboardText(a4);
                                }
                        
                                i++;
                            }
                        }
                    }
                }   
                
                
                if (scrollToBottom)
                {
                    ui.SetScrollHereY(0.5f);
                }
                ui.EndTable();
            }

            ui.End();
        }
        
        

        if (ui.Begin(title))
        {
            //some cursed tree view shenanigans

            ShowDataViewer();
            ui.End();
        }
    }

    private void ShowDataViewer()
    {
        if (focusedPacketTuple == -1) return;
        var pkt = _gamePackets[focusedPacketTuple];

        if (!updated)
        {
            title = $"Data Viewer: {pkt.PacketType.ToString()}###DATAVIEWERWINDOW";
        }
        
        var msg = pkt.ProtoBuf;
        if (msg is null)
        {
            ui.Text("Message is null");
        }
        else
        {
            var pb = msg!;
            TreeOutMessage(pb);
        }
    }

    public void InsertPacket(AnimeGamePacket p)
    {
        _gamePackets.Add(p);
        var repr = p.ProtoBuf is null ? messageIsNullText : JsonFormatter.Default.Format(p.ProtoBuf);
        processed.Add(((++count).ToString(), p.Sender.ToString(), p.PacketType.ToString(), repr));
    }

    public string WidgetName { get; }
    public bool isShow { get; set; }
    
}