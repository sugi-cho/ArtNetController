public interface IDmxOutput
{
    public DmxOutputType Type { get; }
    public string Label { get; set; }
    int StartChannel { get; set; }
    int NumChannels { get; }
    void SetDmx(ref byte[] dmx);
    event System.Action<string> onLabelChanged;
    event System.Action onValueChanged;
}
public interface IUseFine
{
    bool UseFine { get; set; }
    event System.Action<bool> onUseFineChanged;
}
public interface ISizeProp
{
    int SizeProp { get; set; }
    event System.Action<int> onSizePropChanged;
}