public interface IDmxOutputModule
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
public interface INumChoices
{
    int NumChoices { get; set; }
}