using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DmxOutputUniverse : IDmxOutputModule
{
    public void Initialize()
    {
        if (dmxOutputDefinitions != null)
            dmxOutputList = dmxOutputDefinitions.Select(d =>
            {
                if (d.type == DmxOutputType.Fixture)
                    return FixtureLibrary.LoadFixture(d.label);

                var dmxOutput = System.Activator.CreateInstance(TypeMap[d.type]) as IDmxOutputModule;
                dmxOutput.Label = d.label;
                var usefine = dmxOutput as IDmxOutputUseFine;
                if (usefine != null)
                    usefine.UseFine = d.useFine;
                return dmxOutput;
            }).ToList();
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
        dmxOutputDefinitions = dmxOutputList.Select(output =>
        {
            var type = output.GetType();
            var outputType = TypeMap.FirstOrDefault(pair => pair.Value == type).Key;
            var definition = new DmxOutputDefinition { type = outputType, label = output.Label, channel = output.StartChannel };
            var useFine = output as IDmxOutputUseFine;
            if (useFine != null)
                definition.useFine = useFine.UseFine;
            return definition;
        }).ToArray();
    }

    public int Universe { get => universe; set => universe = value; }
    [SerializeField] int universe;
    public string Label { get => label; set => label = value; }
    [SerializeField] string label;

    public int StartChannel
    {
        get => 0;
        set => m_startChannel = 0;
    }
    int m_startChannel;
    public int NumChannels => dmxOutputList
        .Select(o => o.StartChannel + o.NumChannels)
        .OrderBy(ch => ch)
        .LastOrDefault();
    public void SetDmx(ref byte[] dmx)
    {
        foreach (var output in dmxOutputList)
            output.SetDmx(ref dmx);
    }

    List<IDmxOutputModule> dmxOutputList;
    public DmxOutputDefinition[] dmxOutputDefinitions;

    [System.Serializable]
    public struct DmxOutputDefinition
    {
        public DmxOutputType type;
        public string label;
        public bool useFine;
        public int channel;
    }

    readonly Dictionary<DmxOutputType, System.Type> TypeMap
        = new Dictionary<DmxOutputType, System.Type>
        {
            {DmxOutputType.Empty,typeof( DmxOutputEmpty)},
            {DmxOutputType.Bool,typeof( DmxOutputBool)},
            {DmxOutputType.Int,typeof( DmxOutputInt)},
            {DmxOutputType.Selector,typeof(DmxOutputSelector)},
            {DmxOutputType.Float,typeof( DmxOutputFloat)},
            {DmxOutputType.XY,typeof( DmxOutputXY)},
            {DmxOutputType.Color,typeof( DmxOutputColor)},
            {DmxOutputType.Fixture,typeof(DmxOutputFixture)},
        };
}
