using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

[System.Serializable]
public class DmxOutputUniverse : DmxOutputBase
{
    public override DmxOutputType Type => DmxOutputType.Universe;
    public short Universe { get => universe; set => universe = value; }
    [SerializeField] short universe;
    public override int StartChannel
    {
        get => 0;
        set { Debug.Log("DmxOutputUniverse.StartChannel = 0, always"); }
    }
    public override int NumChannels => OutputList
        .Select(o => o.StartChannel + o.NumChannels)
        .OrderBy(ch => ch)
        .LastOrDefault();
    public override void SetDmx(ref byte[] dmx)
    {
        foreach (var output in OutputList)
            output.SetDmx(ref dmx);
    }

    public void ValidateOutputs()
    {
        var inValidOutputList = OutputList.Where(o => !IsValid(o)).ToList();
        inValidOutputList.ForEach(o => OutputList.Remove(o));
        BuildDefinitions();
        NotifyEditChannel();
    }
    public bool IsValid(IDmxOutput output) => Enumerable.Range(output.StartChannel, output.NumChannels).All(ch => {
        if (ch < 0 || 511 < ch)
            return false;
        var existOutput = GetChannelOutput(ch);
        if (existOutput != null || existOutput != output)
            return false;
        else
            return true;
    });
    public IDmxOutput GetChannelOutput(int ch) =>
        OutputList.FirstOrDefault(o => o.StartChannel <= ch && ch <= o.StartChannel + o.NumChannels - 1);
    public List<IDmxOutput> OutputList
    {
        get
        {
            if (!m_initialized)
                Initialize();
            m_outputList.Sort((a, b) => a.StartChannel - b.StartChannel);
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
                .OrderBy(o => o.StartChannel)
                .ToList();
        if (m_outputList == null)
            m_outputList = new List<IDmxOutput>();

        m_outputList.ForEach(output =>
            output.OnValueChanged.Subscribe(_ => valueChangedSubject.OnNext(_)));

        m_initialized = true;
        ValidateOutputs();
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
    public void NotifyEditChannel() => editChannelSubject.OnNext(Unit.Default);
    public void BuildDefinitions()
    {
        OutputList.Sort((a, b) => a.StartChannel - b.StartChannel);
        dmxOutputDefinitions = OutputList
            .Select(output => DmxOutputUtility.CreateDmxOutputDefinitioin(output))
            .ToArray();
    }
}
