using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DmxOutputUniverse : IDmxOutput
{
    public DmxOutputType Type => DmxOutputType.Universe;
    public string Label { get => label; set => label = value; }
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
        m_initialized = true;
    }
    public void AddModule(IDmxOutput module)
    {
        OutputList.Add(module);
        BuildDefinitions();
    }
    public void RemoveModule(IDmxOutput module)
    {
        OutputList.Remove(module);
        BuildDefinitions();
    }
    void BuildDefinitions()
    {
        OutputList.Sort((a, b) => b.StartChannel - a.StartChannel);
        dmxOutputDefinitions = OutputList
            .Select(output => DmxOutputUtility.CreateDmxOutputDefinitioin(output))
            .ToArray();
    }
}
