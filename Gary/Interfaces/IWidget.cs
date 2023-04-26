namespace Gary.Interfaces;

public interface IWidget
{
    public string WidgetName { get; }
    public bool isShow { get; set; }

    public void DoUI();

}