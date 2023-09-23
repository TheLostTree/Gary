using System.Collections;
using System.Globalization;
using System.Reflection;
using Common;
using Common.Protobuf;
using DNToolKit;
using DNToolKit.AnimeGame;
using Gary.Interfaces;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Gary.Widgets;
using ui = ImGuiNET.ImGui;
public class EntityListWidget: IPacketConsumer, IWidget
{
    
    public void InsertPacket(TrafficInstance.TrafficPacket p)
    {

        if(!p.ParseResult.Success) return;
        
        var msg = p.ParseResult.Packet!;
        switch (p.code)
        {
            case Opcode.SceneEntityDisappearNotify:
                var sedn = (SceneEntityDisappearNotify)msg;
                foreach (var entityid in sedn.EntityList)
                {
                    _entities.Remove(entityid);
                }
                break;
            case Opcode.SceneEntityAppearNotify:
                var sean = (SceneEntityAppearNotify)msg;
                foreach (var entity in sean.EntityList)
                {
                    if (_entities.ContainsKey(entity.EntityId!))
                    {
                        _entities[entity.EntityId!] = entity;
                    }
                    else
                    {
                        _entities.Add(entity.EntityId!, entity!);
                    }
                }
                break;
        }
    }

    public EntityListWidget()
    {
        this.WidgetName = "Entity List";
    }

    private Dictionary<uint,SceneEntityInfo> _entities = new();

    public string WidgetName { get; }
    public bool isShow { get; set; }

    private ProtEntityType filterEntity;

    private int filterChoice = 0;

    private string[] entityTypes =
        ProtEntityTypeReflection.Descriptor.EnumTypes.First().Values.Select(x => x.Name).ToArray();
    private static readonly Dictionary<System.Type, Dictionary<object, string>> dictionaries = new();
    
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
    public void DoUI()
    {
        if (ui.Begin("Entity List"))
        {
            ui.Text($"{_entities.Count} entities...");
            if (ui.Combo("EntityType", ref filterChoice, entityTypes, entityTypes.Length))
            {
                
                //this only works bc the enum is contiguous from 0 onwards
                filterEntity = (ProtEntityType)filterChoice;
                // Console.WriteLine(filterEntity);
            }

            if (filterEntity == ProtEntityType.None)
            {
                //show all
                for (int i = 0; i < entityTypes.Length; i++)
                {
                    if (ui.TreeNode(entityTypes[i]))
                    {
                        foreach (var sei in _entities.Values.Where(x => x.EntityType == (ProtEntityType)i))
                        {
                            if (ui.TreeNode(sei.EntityId.ToString()))
                            {
                                TreeOutMessage(sei);
                                ui.TreePop();
                            }
                        }
                        ui.TreePop();
                    }
                }
            }
            else
            {
                int i = (int)filterEntity;
                if (ui.TreeNode(entityTypes[i]))
                {
                    foreach (var sei in _entities.Values.Where(x => x.EntityType == (ProtEntityType)i))
                    {
                        if (ui.TreeNode(sei.EntityId.ToString()))
                        {
                            TreeOutMessage(sei);
                            ui.TreePop();
                        }
                    }
                    ui.TreePop();
                }
            }
            
            
        }
    }
}