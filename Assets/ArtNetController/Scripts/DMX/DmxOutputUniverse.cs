using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DmxOutputUniverse : IDmxOutput
{
    public event System.Action<List<IDmxOutput>> onEditOutputList;
    public event System.Action<string> onLabelChanged;

    public DmxOutputType Type => DmxOutputType.Universe;
    public string Label
    {
        get => label; set
        {
            label = value;
            onLabelChanged?.Invoke(value);
        }
    }
    [SerializeField] string label;
    public int Universe { get => universe; set => universe = value; }
    [SerializeField] int universe;
    public int StartChannel
    {
        get => 0;
        set { Debug.Log("DmxOutputUniverse.StartChannel = 0, always"); }
    }
    public int NumChannels => OutputList
        .Select(o => o.StartChannel + o.NumChannels)
        .OrderBy(ch => ch)
        .LastOrDefault();
    public void SetDmx(ref byte[] dmx)
    {
        foreach (var output in OutputList)
            output.SetDmx(ref dmx);
    }

    public bool IsEmpty(int ch) => GetChannelOutput(ch) == null;
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
        m_initialized = true;
    }
    public void AddModule(IDmxOutput module)
    {
        OutputList.Add(module);
        BuildDefinitions();
        onEditOutputList?.Invoke(OutputList);
    }
    public void RemoveModule(IDmxOutput module)
    {
        OutputList.Remove(module);
        BuildDefinitions();
        onEditOutputList?.Invoke(OutputList);
    }
    void BuildDefinitions()
    {
        OutputList.Sort((a, b) => a.StartChannel - b.StartChannel);
        dmxOutputDefinitions = OutputList
            .Select(output => DmxOutputUtility.CreateDmxOutputDefinitioin(output))
            .ToArray();
    }
}
