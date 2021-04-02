using System.Linq;
using UnityEngine;

public class DmxOutputFloat : IDmxOutputModule, IDmxOutputUseFine
{
    public string Label { get; set; }
    public bool UseFine { get; set; }
    public int StartChannel { get; set; }
    public int NumChannels => UseFine ? 2 : 1;

    public float Value { get => m_value; set => m_value = Mathf.Clamp01(value); }
    float m_value;
    public void SetDmx(ref byte[] dmx)
    {
        if (UseFine)
        {
            dmx[StartChannel] = (byte)Mathf.Min(Value * 256, 255);
            dmx[StartChannel + 1] = (byte)((Value * 256 - dmx[StartChannel]) * 255);
        }
        else
            dmx[StartChannel] = (byte)(Value * 255);
    }
}

public class DmxOutputInt : IDmxOutputModule
{
    public string Label { get; set; }
    public int StartChannel { get; set; }
    public int NumChannels => 1;

    public int Value { get => m_value; set => m_value = (byte)value; }
    int m_value;
    public void SetDmx(ref byte[] dmx) => dmx[StartChannel] = (byte)Value;
}

public class DmxOutputBool : IDmxOutputModule
{
    public string Label { get; set; }
    public int StartChannel { get; set; }
    public int NumChannels => 1;

    public bool Value { get => m_value; set => m_value = value; }
    bool m_value;
    public void SetDmx(ref byte[] dmx) => dmx[StartChannel] = (byte)(m_value ? 255 : 0);
}

public class DmxOutputXY : IDmxOutputModule, IDmxOutputUseFine
{
    public string Label { get; set; }
    public DmxOutputXY()
    {
        dmxOutputX = new DmxOutputFloat();
        dmxOutputY = new DmxOutputFloat();
        dmxOutputs = new[] { dmxOutputX, dmxOutputY };
    }
    DmxOutputFloat dmxOutputX;
    DmxOutputFloat dmxOutputY;
    IDmxOutputModule[] dmxOutputs;

    public bool UseFine
    {
        get => dmxOutputX.UseFine;
        set
        {
            dmxOutputX.UseFine = dmxOutputY.UseFine = value;
            StartChannel = (dmxOutputs[0].StartChannel);
        }
    }
    public int StartChannel
    {
        get => dmxOutputs[0].StartChannel;
        set
        {
            foreach (var output in dmxOutputs)
            {
                output.StartChannel = value;
                value += output.NumChannels;
            }
        }
    }
    public int NumChannels => dmxOutputs.Sum(output => output.NumChannels);

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

    public void SetDmx(ref byte[] dmx)
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
        dmxOutputs = new[] { dmxOutputR, dmxOutputG, dmxOutputB };
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
            StartChannel = dmxOutputs[0].StartChannel;
        }
    }

    public int StartChannel
    {
        get => dmxOutputs[0].StartChannel;
        set
        {
            foreach (var output in dmxOutputs)
            {
                output.StartChannel = value;
                value += output.NumChannels;
            }
        }
    }
    public int NumChannels => dmxOutputs.Sum(output => output.NumChannels);

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

    public void SetDmx(ref byte[] dmx)
    {
        foreach (var output in dmxOutputs)
            output.SetDmx(ref dmx);
    }
}

public class DmxOutputEmpty : IDmxOutputModule
{
    public string Label { get => $"Empty_{m_size}"; set { } }
    public int Size { set => m_size = value; }

    int m_size;
    public int StartChannel { get; set; }
    public int NumChannels => m_size;
    public void SetDmx(ref byte[] dmx) =>
        System.Buffer.BlockCopy(new byte[m_size], 0, dmx, StartChannel, m_size);
}