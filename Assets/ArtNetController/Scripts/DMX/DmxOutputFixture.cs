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
    {
        get
        {
            var onValueChanged = base.OnValueChanged;
            OutputList.ToList().ForEach(output => onValueChanged = Observable.Merge(onValueChanged, output.OnValueChanged));
            return onValueChanged;
        }
    }
    public override IObservable<Unit> OnEditChannel
    {
        get
        {
            var onEditChannel = Observable.Merge(base.OnEditChannel, m_outputList.ObserveCountChanged().AsUnitObservable());
            OutputList.ToList().ForEach(output => onEditChannel = Observable.Merge(onEditChannel, output.OnEditChannel));
            return onEditChannel;
        }
    }

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
                .Select(d => DmxOutputUtility.CreateDmxOutput(d)));
        if (m_outputList == null)
            m_outputList = new ReactiveCollection<IDmxOutput>();
        m_initialized = true;
    }

    public void AddOutput(IDmxOutput output)
    {
        OutputList.Add(output);
        BuildDefinitions();
    }
    public void RemoveOutput(IDmxOutput output)
    {
        OutputList.Remove(output);
        BuildDefinitions();
    }
    public void BuildDefinitions()
        => dmxOutputDefinitions = OutputList
        .OrderBy(output => output.StartChannel)
        .Select(output => DmxOutputUtility.CreateDmxOutputDefinitioin(output))
        .ToArray();

    #endregion
}
