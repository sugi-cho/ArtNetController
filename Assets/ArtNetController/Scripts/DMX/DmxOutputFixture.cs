using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

[System.Serializable]
public class DmxOutputFixture : DmxOutputBase
{
    public string FilePath { get; set; }
    #region IDmxOutput
    public override DmxOutputType Type => DmxOutputType.Fixture;
    public override int StartChannel
    {
        get => base.StartChannel;
        set
        {
            base.StartChannel = value;
            foreach (var output in OutputList)
            {
                output.StartChannel = value;
                value += output.NumChannels;
            }
        }
    }
    public override IObservable<Unit> OnValueChanged
        => Observable.Merge(base.OnValueChanged, m_onValueChanged).ThrottleFrame(1);
    Subject<Unit> m_onValueChanged = new Subject<Unit>();
    public override IObservable<Unit> OnEditChannel
        => Observable.Merge(base.OnEditChannel, m_onEditChannel).ThrottleFrame(1);
    Subject<Unit> m_onEditChannel = new Subject<Unit>();

    public override int NumChannels => OutputList.Sum(output => output.NumChannels);

    public override void SetDmx(ref byte[] dmx)
    {
        foreach (var output in OutputList)
            output.SetDmx(ref dmx);
    }
    #endregion

    #region Fixture methods
    public IList<IDmxOutput> OutputList
    {
        get
        {
            if (!m_initialized)
                Initialize();
            return m_outputList;
        }
    }
    ReactiveCollection<IDmxOutput> m_outputList;
    public DmxOutputDefinition[] dmxOutputDefinitions;
    bool m_initialized;
    public void Initialize()
    {
        if (dmxOutputDefinitions != null)
            m_outputList = new ReactiveCollection<IDmxOutput>(
                dmxOutputDefinitions
                .Select(d =>
                {
                    var o = DmxOutputUtility.CreateDmxOutput(d);
                    o.OnValueChanged.Subscribe(_ => m_onValueChanged.OnNext(_));
                    o.OnEditChannel.Subscribe(_ => m_onEditChannel.OnNext(_));
                    return o;
                })
            );
        if (m_outputList == null)
            m_outputList = new ReactiveCollection<IDmxOutput>();

        m_outputList.ObserveCountChanged().Subscribe(_ =>
        {
            StartChannel = 0;
            BuildDefinitions();
        });
        m_outputList.ObserveAdd().Subscribe(evt =>
        {
            var output = evt.Value;
            output.OnValueChanged.Subscribe(_ => m_onValueChanged.OnNext(_));
            output.OnEditChannel.Subscribe(_ => m_onEditChannel.OnNext(_));
        });
        m_initialized = true;
    }

    public void BuildDefinitions()
        => dmxOutputDefinitions = OutputList
        .OrderBy(output => output.StartChannel)
        .Select(output => DmxOutputUtility.CreateDmxOutputDefinitioin(output))
        .ToArray();

    #endregion
}
