using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

[System.Serializable]
public class DmxOutputUniverse : DmxOutputBase
{
    FixtureLibrary FixtureLibrary => FixtureLibrary.Instance;
    public override DmxOutputType Type => DmxOutputType.Universe;
    public short Universe { get => universe; set => universe = value; }
    [SerializeField] short universe;
    public override int StartChannel
    {
        get => 0;
        set { Debug.Log("DmxOutputUniverse.StartChannel = 0, always"); }
    }
    public override IObservable<Unit> OnValueChanged
        => Observable.Merge(base.OnValueChanged, m_onValueChanged).ThrottleFrame(1);
    Subject<Unit> m_onValueChanged = new Subject<Unit>();
    public override IObservable<Unit> OnEditChannel
        => Observable.Merge(base.OnEditChannel, m_onEditChannel).ThrottleFrame(1);
    Subject<Unit> m_onEditChannel = new Subject<Unit>();

    public override int NumChannels => OutputList
        .Select(o => o.StartChannel + o.NumChannels)
        .OrderBy(ch => ch)
        .LastOrDefault();
    public override void SetDmx(ref byte[] dmx)
    {
        foreach (var output in OutputList)
            output.SetDmx(ref dmx);
    }

    public void ReloadFixture(string label)
        => OutputList
            .Select(output => (output as DmxOutputFixture))
            .Where(fixture => fixture != null && fixture.Label == label)
            .ToList().ForEach(fixture => FixtureLibrary.ReloadFixture(fixture));
    public void ValidateOutputs()
    {
        var inValidOutputList = OutputList.Where(o => !IsValid(o)).ToList();
        inValidOutputList.ForEach(o => OutputList.Remove(o));
        BuildDefinitions();
    }
    public bool IsValid(IDmxOutput output) => IsValid(output.StartChannel, output.NumChannels, output);
    public bool IsValid(int startCh, int numCh, IDmxOutput output)
    {
        var fixture = output as DmxOutputFixture;
        if (fixture != null && !FixtureLibrary.FixtureLabelList.Contains(output.Label))
            return false;
        return Enumerable.Range(startCh, numCh).All(ch =>
        {
            if (ch < 0 || 511 < ch)
                return false;
            var existOutput = GetChannelOutput(ch);
            if (existOutput != null && existOutput != output)
                return false;
            else
                return true;
        });
    }
    public IDmxOutput GetChannelOutput(int ch) =>
        OutputList.FirstOrDefault(o => o.StartChannel <= ch && ch <= o.StartChannel + o.NumChannels - 1);
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
                .OrderBy(o => o.StartChannel));
        if (m_outputList == null)
            m_outputList = new ReactiveCollection<IDmxOutput>();

        m_outputList.ObserveCountChanged().Subscribe(_ =>
        {
            m_onEditChannel.OnNext(Unit.Default);
            BuildDefinitions();
        });
        m_outputList.ObserveAdd().Subscribe(evt =>
        {
            var output = evt.Value;
            output.OnValueChanged.Subscribe(_ => m_onValueChanged.OnNext(_));
            output.OnEditChannel.Subscribe(_ => m_onEditChannel.OnNext(_));
        });
        m_initialized = true;
        ValidateOutputs();
    }
    public void BuildDefinitions()
        => dmxOutputDefinitions = OutputList
        .OrderBy(output => output.StartChannel)
        .Select(output => DmxOutputUtility.CreateDmxOutputDefinitioin(output))
        .ToArray();
}
