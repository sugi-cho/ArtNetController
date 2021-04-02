public interface IDmxOutputModule
{
    public string Label { get; set; }
    int StartChannel { get; set; }
    int NumChannels { get; }
    void SetDmx(ref byte[] dmx);
}
public interface IDmxOutputUseFine
{
    bool UseFine { get; set; }
}
