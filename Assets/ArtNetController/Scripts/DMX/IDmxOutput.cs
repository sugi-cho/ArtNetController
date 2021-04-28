using System;
using UniRx;

public interface IDmxOutput
{
    public DmxOutputType Type { get; }
    public string Label { get; set; }
    int StartChannel { get; set; }
    int NumChannels { get; }
    void SetDmx(ref byte[] dmx);

    IObservable<string> OnLabelChanged { get; }
    IObservable<Unit> OnValueChanged { get; }
    IObservable<Unit> OnEditChannel { get; } 
}
public interface IUseFine
{
    bool UseFine { get; set; }

    IObservable<bool> OnUseFineChanged { get; }
}
public interface ISizeProp
{
    int SizeProp { get; set; }

    IObservable<int> OnSizePropChanged { get; }
}