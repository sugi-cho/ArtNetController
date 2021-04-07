using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DmxOutputFixture : IDmxOutputModule
{
    public void Initialize()
    {
        if (dmxOutputDefinitions != null)
            dmxOutputList = dmxOutputDefinitions
                .Select(d => DmxOutputUtility.DefinitionToModule(d))
                .ToList();
        if (dmxOutputList == null)
            dmxOutputList = new List<IDmxOutputModule>();
    }

    public void AddModule(IDmxOutputModule module)
    {
        dmxOutputList.Add(module);
        BuildDefinitions();
    }
    public void RemoveModule(IDmxOutputModule module)
    {
        dmxOutputList.Remove(module);
        BuildDefinitions();
    }
    void BuildDefinitions()
    {
        dmxOutputList.Sort((a, b) => b.StartChannel - a.StartChannel);
        dmxOutputDefinitions = dmxOutputList
            .Select(output => DmxOutputUtility.DefinitionFromModule(output))
            .ToArray();
    }

    public string Label { get => label; set => label = value; }
    [SerializeField] string label;
    public int StartChannel
    {
        get => m_startChannel;
        set
        {
            m_startChannel = value;
            foreach (var output in dmxOutputList)
            {
                output.StartChannel = value;
                value += output.NumChannels;
            }
        }
    }
    int m_startChannel;
    public int NumChannels => dmxOutputList.Sum(output => output.NumChannels);
    public void SetDmx(ref byte[] dmx)
    {
        foreach (var output in dmxOutputList)
            output.SetDmx(ref dmx);
    }

    List<IDmxOutputModule> dmxOutputList;
    public DmxOutputDefinition[] dmxOutputDefinitions;
}
