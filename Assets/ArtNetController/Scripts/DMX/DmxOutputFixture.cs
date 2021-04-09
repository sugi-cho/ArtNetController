using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DmxOutputFixture : IDmxOutput
{
    #region IDmxOutput
    public string Label { get; set; }
    public int StartChannel
    {
        get => m_startChannel;
        set
        {
            m_startChannel = value;
            foreach (var output in m_dmxOutputList)
            {
                output.StartChannel = value;
                value += output.NumChannels;
            }
        }
    }
    int m_startChannel;
    public int NumChannels => m_dmxOutputList.Sum(output => output.NumChannels);

    public void SetDmx(ref byte[] dmx)
    {
        foreach (var output in m_dmxOutputList)
            output.SetDmx(ref dmx);
    }
    #endregion

    #region Fixture methods
    public List<IDmxOutput> DmxOutputList => m_dmxOutputList;
    List<IDmxOutput> m_dmxOutputList;
    public DmxOutputDefinition[] dmxOutputDefinitions;
    public void Initialize()
    {
        if (dmxOutputDefinitions != null)
            m_dmxOutputList = dmxOutputDefinitions
                .Select(d => DmxOutputUtility.DefinitionToModule(d))
                .ToList();
        if (m_dmxOutputList == null)
            m_dmxOutputList = new List<IDmxOutput>();
    }

    public void AddModule(IDmxOutput module)
    {
        m_dmxOutputList.Add(module);
        BuildDefinitions();
    }
    public void RemoveModule(IDmxOutput module)
    {
        m_dmxOutputList.Remove(module);
        BuildDefinitions();
    }
    void BuildDefinitions()
    {
        m_dmxOutputList.Sort((a, b) => b.StartChannel - a.StartChannel);
        dmxOutputDefinitions = m_dmxOutputList
            .Select(output => DmxOutputUtility.DefinitionFromModule(output))
            .ToArray();
    }
    #endregion
}
