using System;
using System.Linq;
using UnityEngine;
using UniRx;

public class DmxOutputFloat : DmxOutputBase<float>, IUseFine
{
    public override DmxOutputType Type => DmxOutputType.Float;
    public bool UseFine { get => m_useFineProp.Value; set => m_useFineProp.Value = value; }
    public IObservable<bool> OnUseFineChanged => m_useFineProp;
    ReactiveProperty<bool> m_useFineProp = new ReactiveProperty<bool>();
    public override int NumChannels => UseFine ? 2 : 1;

    public override float Value { get => base.Value; set => base.Value = Mathf.Clamp01(value); }
    public override void SetDmx(ref byte[] dmx)
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

public class DmxOutputInt : DmxOutputBase<int>
{
    public override DmxOutputType Type => DmxOutputType.Int;
    public override int NumChannels => 1;
    public override int Value { get => base.Value; set => base.Value = Mathf.Clamp(value, 0, 255); }
    public override void SetDmx(ref byte[] dmx) => dmx[StartChannel] = (byte)Value;
}

public class DmxOutputSelector : DmxOutputBase<int>, ISizeProp
{
    public override DmxOutputType Type => DmxOutputType.Selector;
    public override int NumChannels => 1;

    public int SizeProp { get => m_sizeProp.Value; set => m_sizeProp.Value = value; }
    public IObservable<int> OnSizePropChanged => m_sizeProp;
    ReactiveProperty<int> m_sizeProp = new ReactiveProperty<int>(5);

    public override int Value { get => base.Value; set => base.Value = Mathf.Clamp(value, 0, SizeProp - 1); }
    public override void SetDmx(ref byte[] dmx) => dmx[StartChannel] = (byte)((Value + 0.5f) / SizeProp * 255);

}

public class DmxOutputBool : DmxOutputBase<bool>
{
    public override DmxOutputType Type => DmxOutputType.Bool;
    public override int NumChannels => 1;
    public override void SetDmx(ref byte[] dmx) => dmx[StartChannel] = (byte)(Value ? 255 : 0);
}

public class DmxOutputXY : DmxOutputBase<Vector2>, IUseFine
{
    public override DmxOutputType Type => DmxOutputType.XY;
    public DmxOutputXY() : base()
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
        get => m_useFineProp.Value;
        set
        {
            m_useFineProp.Value = value;
            dmxOutputX.UseFine = dmxOutputY.UseFine = UseFine;
            StartChannel = dmxOutputs[0].StartChannel;
        }
    }
    public IObservable<bool> OnUseFineChanged => m_useFineProp;
    ReactiveProperty<bool> m_useFineProp = new ReactiveProperty<bool>();
    public override int StartChannel
    {
        get => base.StartChannel;
        set
        {
            base.StartChannel = value;
            foreach (var output in dmxOutputs)
            {
                output.StartChannel = value;
                value += output.NumChannels;
            }
        }
    }
    public override int NumChannels => dmxOutputs.Sum(output => output.NumChannels);

    public override Vector2 Value
    {
        get => base.Value;
        set
        {
            value.x = Mathf.Clamp(value.x, 0, 1f);
            value.y = Mathf.Clamp(value.y, 0, 1f);
            base.Value = value;
            dmxOutputX.Value = Value.x;
            dmxOutputY.Value = Value.y;
        }
    }

    public override void SetDmx(ref byte[] dmx)
    {
        foreach (var output in dmxOutputs)
            output.SetDmx(ref dmx);
    }
}

public class DmxOutputColor : DmxOutputBase<Color>, IUseFine
{
    public override DmxOutputType Type => DmxOutputType.Color;

    public DmxOutputColor() : base()
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
        get => m_useFineProp.Value;
        set
        {
            m_useFineProp.Value = value;
            dmxOutputR.UseFine = dmxOutputG.UseFine = dmxOutputB.UseFine = UseFine;
            StartChannel = dmxOutputs[0].StartChannel;
        }
    }
    public IObservable<bool> OnUseFineChanged => m_useFineProp;
    ReactiveProperty<bool> m_useFineProp = new ReactiveProperty<bool>();

    public override int StartChannel
    {
        get => base.StartChannel;
        set
        {
            base.StartChannel = value;
            foreach (var output in dmxOutputs)
            {
                output.StartChannel = value;
                value += output.NumChannels;
            }
        }
    }
    public override int NumChannels => dmxOutputs.Sum(output => output.NumChannels);

    public override Color Value
    {
        get => base.Value;
        set
        {
            base.Value = value;
            dmxOutputR.Value = Value.r;
            dmxOutputG.Value = Value.g;
            dmxOutputB.Value = Value.b;
        }
    }

    public override void SetDmx(ref byte[] dmx)
    {
        foreach (var output in dmxOutputs)
            output.SetDmx(ref dmx);
    }
}

public class DmxOutputEmpty : DmxOutputBase, ISizeProp
{
    public override DmxOutputType Type => DmxOutputType.Empty;
    public int SizeProp { get => m_sizeProp.Value; set => m_sizeProp.Value = value; }
    public IObservable<int> OnSizePropChanged => m_sizeProp;
    ReactiveProperty<int> m_sizeProp = new ReactiveProperty<int>(1);

    public override int NumChannels => SizeProp;
    public override void SetDmx(ref byte[] dmx) =>
        System.Buffer.BlockCopy(new byte[SizeProp], 0, dmx, StartChannel, SizeProp);
}


public abstract class DmxOutputBase<T> : DmxOutputBase
{
    public DmxOutputBase() : base() =>
        m_valueProp.Subscribe(_ => valueChangedSubject.OnNext(Unit.Default));

    public virtual T Value { get => m_valueProp.Value; set => m_valueProp.Value = value; }
    ReactiveProperty<T> m_valueProp = new ReactiveProperty<T>();
}
public abstract class DmxOutputBase : IDmxOutput
{
    public DmxOutputBase() => 
        m_startChannelProp.Subscribe(_ => editChannelSubject.OnNext(Unit.Default));

    public abstract DmxOutputType Type { get; }
    public string Label { get => m_labelProp.Value; set => m_labelProp.Value = value; }
    public IObservable<string> OnLabelChanged => m_labelProp;
    [SerializeField] protected ReactiveProperty<string> m_labelProp = new ReactiveProperty<string>();
    public IObservable<Unit> OnValueChanged => valueChangedSubject;
    protected Subject<Unit> valueChangedSubject = new Subject<Unit>();
    public virtual int StartChannel { get => m_startChannelProp.Value; set => m_startChannelProp.Value = value; }
    public IObservable<Unit> OnEditChannel => editChannelSubject;
    ReactiveProperty<int> m_startChannelProp = new ReactiveProperty<int>();
    protected Subject<Unit> editChannelSubject = new Subject<Unit>();
    public abstract int NumChannels { get; }
    public abstract void SetDmx(ref byte[] dmx);
}