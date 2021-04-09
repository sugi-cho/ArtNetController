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
        set { }
    }
    public int NumChannels => dmxOutputList
        .Select(o => o.StartChannel + o.NumChannels)
        .OrderBy(ch => ch)
        .LastOrDefault();
    public void SetDmx(ref byte[] dmx)
    {
        foreach (var output in dmxOutputList)
            output.SetDmx(ref dmx);
    }

    public void Initialize()
    {
        if (dmxOutputDefinitions != null)
            dmxOutputList = dmxOutputDefinitions
                .Select(d => DmxOutputUtility.CreateDmxOutput(d))
                .ToList();
        if (dmxOutputList == null)
            dmxOutputList = new List<IDmxOutput>();
    }
    public void AddModule(IDmxOutput module)
    {
        dmxOutputList.Add(module);
        BuildDefinitions();
    }
    public void RemoveModule(IDmxOutput module)
    {
        dmxOutputList.Remove(module);
        BuildDefinitions();
    }
    void BuildDefinitions()
    {
        dmxOutputList.Sort((a, b) => b.StartChannel - a.StartChannel);
        dmxOutputDefinitions = dmxOutputList
            .Select(output => DmxOutputUtility.CreateDmxOutputDefinitioin(output))
            .ToArray();
    }

    List<IDmxOutput> dmxOutputList;
    public DmxOutputDefinition[] dmxOutputDefinitions;
}
