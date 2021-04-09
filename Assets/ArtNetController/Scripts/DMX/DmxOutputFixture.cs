using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DmxOutputFixture : IDmxOutput
{
    public DmxOutputType Type => DmxOutputType.Fixture;
    #region IDmxOutput
    public string Label { get => label; set => label = value; }
    [SerializeField] string label;
    public int StartChannel
    {
        get
        {
            if (!m_initialized)
                Initialize();
            return m_startChannel;
        }
        set
        {
            m_startChannel = value;
            foreach (var output in DmxOutputList)
            {
                output.StartChannel = value;
                value += output.NumChannels;
            }
        }
    }
    int m_startChannel;
    public int NumChannels => DmxOutputList.Sum(output => output.NumChannels);

    public void SetDmx(ref byte[] dmx)
    {
        foreach (var output in m_dmxOutputList)
            output.SetDmx(ref dmx);
    }
    #endregion

    #region Fixture methods
    public List<IDmxOutput> DmxOutputList
    {
        get
        {
            if (!m_initialized)
                Initialize();
            return m_dmxOutputList;
        }
    }
    List<IDmxOutput> m_dmxOutputList;
    public DmxOutputDefinition[] dmxOutputDefinitions;
    bool m_initialized;
    public void Initialize()
    {
        if (dmxOutputDefinitions != null)
            m_dmxOutputList = dmxOutputDefinitions
                .Select(d => DmxOutputUtility.CreateDmxOutput(d))
                .ToList();
        if (m_dmxOutputList == null)
            m_dmxOutputList = new List<IDmxOutput>();
        m_initialized = true;
    }

    public void AddModule(IDmxOutput module)
    {
        DmxOutputList.Add(module);
        BuildDefinitions();
    }
    public void RemoveModule(IDmxOutput module)
    {
        DmxOutputList.Remove(module);
        BuildDefinitions();
    }
    public void BuildDefinitions()
    {
        var startChannel = StartChannel;
        StartChannel = 0;
        dmxOutputDefinitions = DmxOutputList
            .Select(output => DmxOutputUtility.CreateDmxOutputDefinitioin(output))
            .ToArray();
        StartChannel = startChannel;
    }
    #endregion
}
