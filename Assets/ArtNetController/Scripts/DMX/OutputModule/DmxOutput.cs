using System.Linq;
using UnityEngine;

using sugi.cc.udp.artnet;

public class DmxOutputFloat : IDmxOutputModule, IDmxOutputUseFine
{
    public string Label { get; set; }
    public bool UseFine { get; set; }
    int m_startChannel;
    int IDmxOutputModule.StartChannel => m_startChannel;
    int IDmxOutputModule.NumChannels => UseFine ? 2 : 1;

    public float Value { get => m_value; set => m_value = Mathf.Clamp01(value); }
    float m_value;
    void IDmxOutputModule.SetChannel(int channel) => m_startChannel = channel;
    void IDmxOutputModule.SetDmx(ref byte[] dmx)
    {
        if (UseFine)
        {
            dmx[m_startChannel] = (byte)Mathf.Min(Value * 256, 255);
            dmx[m_startChannel + 1] = (byte)((Value * 256 - dmx[m_startChannel]) * 255);
        }
        else
            dmx[m_startChannel] = (byte)(Value * 255);
    }
}

public class DmxOutputInt : IDmxOutputModule
{
    public string Label { get; set; }
    int IDmxOutputModule.StartChannel => m_startChannel;
    int IDmxOutputModule.NumChannels => 1;
    int m_startChannel;

    public int Value { get => m_value; set => m_value = (byte)value; }
    int m_value;
    void IDmxOutputModule.SetChannel(int channel) => m_startChannel = channel;
    void IDmxOutputModule.SetDmx(ref byte[] dmx) => dmx[m_startChannel] = (byte)Value;
}

public class DmxOutputBool : IDmxOutputModule
{
    public string Label { get; set; }
    int IDmxOutputModule.StartChannel => m_startChannel;
    int IDmxOutputModule.NumChannels => 1;
    int m_startChannel;

    public bool Value { get => m_value; set => m_value = value; }
    bool m_value;
    void IDmxOutputModule.SetChannel(int channel) => m_startChannel = channel;
    void IDmxOutputModule.SetDmx(ref byte[] dmx) => dmx[m_startChannel] = (byte)(m_value ? 255 : 0);
}

public class DmxOutputXY : IDmxOutputModule, IDmxOutputUseFine
{
    public string Label { get; set; }
    public DmxOutputXY()
    {
        dmxOutputX = new DmxOutputFloat();
        dmxOutputY = new DmxOutputFloat();
        dmxOutputs = new[] { dmxOutputX, dmxOutputY as IDmxOutputModule, };
    }
    DmxOutputFloat dmxOutputX;
    DmxOutputFloat dmxOutputY;
    IDmxOutputModule[] dmxOutputs;

    bool IDmxOutputUseFine.UseFine
    {
        get => dmxOutputX.UseFine;
        set
        {
            dmxOutputX.UseFine = dmxOutputY.UseFine = value;
            (this as IDmxOutputModule).SetChannel(dmxOutputs[0].StartChannel);
        }
    }
    int IDmxOutputModule.StartChannel => dmxOutputs[0].StartChannel;
    int IDmxOutputModule.NumChannels => dmxOutputs.Sum(output => output.NumChannels);

    public (float x, float y) Value
    {
        get => m_value;
        set
        {
            dmxOutputX.Value = value.x;
            dmxOutputY.Value = value.y;
            m_value.x = dmxOutputX.Value;
            m_value.y = dmxOutputY.Value;
        }
    }
    (float x, float y) m_value;

    void IDmxOutputModule.SetChannel(int channel)
    {
        for (var i = 0; i < dmxOutputs.Length; i++)
        {
            dmxOutputs[i].SetChannel(channel);
            channel += dmxOutputs[i].NumChannels;
        }
    }
    void IDmxOutputModule.SetDmx(ref byte[] dmx)
    {
        foreach (var output in dmxOutputs)
            output.SetDmx(ref dmx);
    }
}

public class DmxOutputColor : IDmxOutputModule, IDmxOutputUseFine
{
    public string Label { get; set; }
    public DmxOutputColor()
    {
        dmxOutputR = new DmxOutputFloat();
        dmxOutputG = new DmxOutputFloat();
        dmxOutputB = new DmxOutputFloat();
        dmxOutputs = new[] { dmxOutputR, dmxOutputG, dmxOutputB as IDmxOutputModule };
    }
    DmxOutputFloat dmxOutputR;
    DmxOutputFloat dmxOutputG;
    DmxOutputFloat dmxOutputB;
    IDmxOutputModule[] dmxOutputs;

    bool IDmxOutputUseFine.UseFine
    {
        get => dmxOutputR.UseFine;
        set
        {
            dmxOutputR.UseFine = dmxOutputG.UseFine = dmxOutputB.UseFine = value;
            (this as IDmxOutputModule).SetChannel(dmxOutputs[0].StartChannel);
        }
    }

    int IDmxOutputModule.StartChannel => dmxOutputs[0].StartChannel;
    int IDmxOutputModule.NumChannels => dmxOutputs.Sum(output => output.NumChannels);

    public Color Value
    {
        get => m_value;
        set
        {
            m_value = value;
            dmxOutputR.Value = value.r;
            dmxOutputG.Value = value.g;
            dmxOutputB.Value = value.b;
        }
    }
    Color m_value;

    void IDmxOutputModule.SetChannel(int channel)
    {
        foreach(var output in dmxOutputs)
        {
            output.SetChannel(channel);
            channel += output.NumChannels;
        }
    }
    void IDmxOutputModule.SetDmx(ref byte[] dmx)
    {
        foreach (var output in dmxOutputs)
            output.SetDmx(ref dmx);
    }
}

public class DmxOutputEmpty : IDmxOutputModule
{
    public string Label { get => $"Empty_{m_size}"; set { } }
    public int Size { set => m_size = value; }
    int m_startChannel;
    int m_size;
    int IDmxOutputModule.StartChannel => m_startChannel;
    int IDmxOutputModule.NumChannels => m_size;
    void IDmxOutputModule.SetChannel(int channel) => m_startChannel = channel;
    void IDmxOutputModule.SetDmx(ref byte[] dmx) =>
        System.Buffer.BlockCopy(new byte[m_size], 0, dmx, m_startChannel, m_size);
}