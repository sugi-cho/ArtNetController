using System.Linq;
using UnityEngine;

[System.Serializable]
public class DmxOutputFloat : IDmxOutput, IUseFine
{
    public DmxOutputType Type => DmxOutputType.Float;
    public string Label
    {
        get => m_label; set
        {
            m_label = value;
            onLabelChanged?.Invoke(value);
        }
    }
    string m_label;
    public event System.Action<string> onLabelChanged;
    public event System.Action onValueChanged;
    public bool UseFine
    {
        get => m_useFine; set
        {
            m_useFine = value;
            onUseFineChanged?.Invoke(value);
        }
    }
    bool m_useFine;
    public event System.Action<bool> onUseFineChanged;
    public int StartChannel { get; set; }
    public int NumChannels => UseFine ? 2 : 1;

    public float Value
    {
        get => m_value; set
        {
            m_value = Mathf.Clamp01(value);
            onValueChanged?.Invoke();
        }
    }
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

[System.Serializable]
public class DmxOutputInt : IDmxOutput
{
    public DmxOutputType Type => DmxOutputType.Int;
    public string Label
    {
        get => m_label; set
        {
            m_label = value;
            onLabelChanged?.Invoke(value);
        }
    }
    string m_label;
    public event System.Action<string> onLabelChanged;
    public event System.Action onValueChanged;
    public int StartChannel { get; set; }
    public int NumChannels => 1;

    public int Value
    {
        get => m_value; set
        {
            m_value = Mathf.Clamp(value, 0, 255);
            onValueChanged?.Invoke();
        }
    }
    int m_value;
    public void SetDmx(ref byte[] dmx) => dmx[StartChannel] = (byte)Value;
}

[System.Serializable]
public class DmxOutputSelector : IDmxOutput, ISizeProp
{
    public DmxOutputType Type => DmxOutputType.Selector;
    public string Label
    {
        get => m_label; set
        {
            m_label = value;
            onLabelChanged?.Invoke(value);
        }
    }
    string m_label;
    public event System.Action<string> onLabelChanged;
    public event System.Action onValueChanged;
    public int StartChannel { get; set; }
    public int NumChannels => 1;

    public int SizeProp
    {
        get => m_size; set
        {
            m_size = value;
            onSizePropChanged?.Invoke(value);
        }
    }
    int m_size;
    public event System.Action<int> onSizePropChanged;
    public int Value
    {
        get => m_value; set
        {
            m_value = Mathf.Clamp(value, 0, SizeProp - 1);
            onValueChanged?.Invoke();
        }
    }
    int m_value;
    public void SetDmx(ref byte[] dmx) => dmx[StartChannel] = (byte)((Value + 0.5f) / SizeProp * 255);

}

[System.Serializable]
public class DmxOutputBool : IDmxOutput
{
    public DmxOutputType Type => DmxOutputType.Bool;
    public string Label
    {
        get => m_label; set
        {
            m_label = value;
            onLabelChanged?.Invoke(value);
        }
    }
    string m_label;
    public event System.Action<string> onLabelChanged;
    public event System.Action onValueChanged;
    public int StartChannel { get; set; }
    public int NumChannels => 1;

    public bool Value
    {
        get => m_value; set
        {
            m_value = value;
            onValueChanged?.Invoke();
        }
    }
    bool m_value;
    public void SetDmx(ref byte[] dmx) => dmx[StartChannel] = (byte)(m_value ? 255 : 0);
}

[System.Serializable]
public class DmxOutputXY : IDmxOutput, IUseFine
{
    public DmxOutputType Type => DmxOutputType.XY;
    public string Label
    {
        get => m_label; set
        {
            m_label = value;
            onLabelChanged?.Invoke(value);
        }
    }
    string m_label;
    public event System.Action<string> onLabelChanged;
    public event System.Action onValueChanged;
    public DmxOutputXY()
    {
        dmxOutputX = new DmxOutputFloat();
        dmxOutputY = new DmxOutputFloat();
        dmxOutputs = new[] { dmxOutputX, dmxOutputY };
    }
    DmxOutputFloat dmxOutputX;
    DmxOutputFloat dmxOutputY;
    IDmxOutput[] dmxOutputs;

    public bool UseFine
    {
        get => dmxOutputX.UseFine;
        set
        {
            dmxOutputX.UseFine = dmxOutputY.UseFine = value;
            StartChannel = (dmxOutputs[0].StartChannel);
            onUseFineChanged?.Invoke(value);
        }
    }
    public event System.Action<bool> onUseFineChanged;
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

    public Vector2 Value
    {
        get => m_value;
        set
        {
            dmxOutputX.Value = value.x;
            dmxOutputY.Value = value.y;
            m_value.x = dmxOutputX.Value;
            m_value.y = dmxOutputY.Value;
            onValueChanged?.Invoke();
        }
    }
    Vector2 m_value;

    public void SetDmx(ref byte[] dmx)
    {
        foreach (var output in dmxOutputs)
            output.SetDmx(ref dmx);
    }
}

[System.Serializable]
public class DmxOutputColor : IDmxOutput, IUseFine
{
    public DmxOutputType Type => DmxOutputType.Color;
    public string Label
    {
        get => m_label; set
        {
            m_label = value;
            onLabelChanged?.Invoke(value);
        }
    }
    string m_label;
    public event System.Action<string> onLabelChanged;
    public event System.Action onValueChanged;
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
    IDmxOutput[] dmxOutputs;

    public bool UseFine
    {
        get => dmxOutputR.UseFine;
        set
        {
            dmxOutputR.UseFine = dmxOutputG.UseFine = dmxOutputB.UseFine = value;
            StartChannel = dmxOutputs[0].StartChannel;
            onUseFineChanged?.Invoke(value);
        }
    }
    public event System.Action<bool> onUseFineChanged;

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
            onValueChanged?.Invoke();
        }
    }
    Color m_value;

    public void SetDmx(ref byte[] dmx)
    {
        foreach (var output in dmxOutputs)
            output.SetDmx(ref dmx);
    }
}

[System.Serializable]
public class DmxOutputEmpty : IDmxOutput, ISizeProp
{
    public DmxOutputType Type => DmxOutputType.Empty;
    public event System.Action<string> onLabelChanged;
    public event System.Action onValueChanged;
    public string Label { get => $"Empty_{SizeProp}"; set => onLabelChanged?.Invoke(value); }
    public int SizeProp
    {
        get => m_size; set
        {
            m_size = value;
            onSizePropChanged?.Invoke(value);
        }
    }
    int m_size;
    public event System.Action<int> onSizePropChanged;
    public int StartChannel { get; set; }
    public int NumChannels => SizeProp;
    public void SetDmx(ref byte[] dmx) =>
        System.Buffer.BlockCopy(new byte[SizeProp], 0, dmx, StartChannel, SizeProp);
}