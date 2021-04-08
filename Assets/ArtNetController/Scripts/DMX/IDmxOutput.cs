public interface IDmxOutput
{
    public string Label { get; set; }
    int StartChannel { get; set; }
    int NumChannels { get; }
    void SetDmx(ref byte[] dmx);
}
public interface IUseFine
{
    bool UseFine { get; set; }
}
public interface ISizeProp
{
    int SizeProp { get; set; }
}