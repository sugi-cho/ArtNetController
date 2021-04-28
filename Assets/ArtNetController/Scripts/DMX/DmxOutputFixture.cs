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

    internal void NotifyEditChannel() => editChannelSubject.OnNext(Unit.Default);

    public override int NumChannels => OutputList.Sum(output => output.NumChannels);

    public override void SetDmx(ref byte[] dmx)
    {
        foreach (var output in OutputList)
            output.SetDmx(ref dmx);
    }
    #endregion

    #region Fixture methods
    public List<IDmxOutput> OutputList
    {
        get
        {
            if (!m_initialized)
                Initialize();
            return m_outputList;
        }
    }
    List<IDmxOutput> m_outputList;
    public DmxOutputDefinition[] dmxOutputDefinitions;
    bool m_initialized;
    public void Initialize()
    {
        if (dmxOutputDefinitions != null)
            m_outputList = dmxOutputDefinitions
                .Select(d => DmxOutputUtility.CreateDmxOutput(d))
                .ToList();
        if (m_outputList == null)
            m_outputList = new List<IDmxOutput>();
        m_outputList.ForEach(output =>
            output.OnValueChanged.Subscribe(_ => valueChangedSubject.OnNext(_)));
        m_initialized = true;
    }

    public void AddOutput(IDmxOutput output)
    {
        output.OnValueChanged.Subscribe(_ => valueChangedSubject.OnNext(_));
        OutputList.Add(output);
        BuildDefinitions();
    }
    public void RemoveOutput(IDmxOutput output)
    {
        OutputList.Remove(output);
        BuildDefinitions();
    }
    public void BuildDefinitions()
    {
        var startChannel = StartChannel;
        StartChannel = 0;
        dmxOutputDefinitions = OutputList
            .Select(output => DmxOutputUtility.CreateDmxOutputDefinitioin(output))
            .ToArray();
        StartChannel = startChannel;
    }
    #endregion
}
