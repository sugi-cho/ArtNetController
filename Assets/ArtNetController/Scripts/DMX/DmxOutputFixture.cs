using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DmxOutputFixture : IDmxOutput
{
    public event System.Action<List<IDmxOutput>> onEditOutputList;
    public event System.Action<string> onLabelChanged;

    public string FilePath { get; set; }
    public DmxOutputType Type => DmxOutputType.Fixture;
    #region IDmxOutput
    public string Label
    {
        get => label; set
        {
            label = value;
            onLabelChanged?.Invoke(value);
        }
    }
    [SerializeField] string label;
    public int StartChannel
    {
        get => m_startChannel;
        set
        {
            m_startChannel = value;
            foreach (var output in OutputList)
            {
                output.StartChannel = value;
                value += output.NumChannels;
            }
        }
    }
    int m_startChannel;
    public int NumChannels => OutputList.Sum(output => output.NumChannels);

    public void SetDmx(ref byte[] dmx)
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
        m_initialized = true;
    }

    public void AddModule(IDmxOutput module)
    {
        OutputList.Add(module);
        onEditOutputList?.Invoke(m_outputList);
        BuildDefinitions();
    }
    public void RemoveModule(IDmxOutput module)
    {
        OutputList.Remove(module);
        onEditOutputList?.Invoke(m_outputList);
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
