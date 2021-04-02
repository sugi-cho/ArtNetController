using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[System.Serializable]
public class DmxOutputUniverse : IDmxOutputModule
{
    public void SetModuleList()
    {
        if (dmxOutputDefinitions != null)
            dmxOutputList = dmxOutputDefinitions.Select(d =>
            {
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
        dmxOutputDefinitions = dmxOutputList.Select(o =>
        {
            var type = o.GetType();
            var outputType = TypeMap.FirstOrDefault(pair => pair.Value == type).Key;
            var definition = new DmxOutputDefinition { type = outputType, label = o.Label, channel = o.StartChannel };
            var useFine = o as IDmxOutputUseFine;
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

    public enum DmxOutputType
    {
        Bool,
        Int,
        Float,
        XY,
        Color,
        Fixture,
    }

    readonly Dictionary<DmxOutputType, System.Type> TypeMap
        = new Dictionary<DmxOutputType, System.Type>
        {
            {DmxOutputType.Bool,typeof( DmxOutputBool)},
            {DmxOutputType.Int,typeof( DmxOutputInt)},
            {DmxOutputType.Float,typeof( DmxOutputFloat)},
            {DmxOutputType.XY,typeof( DmxOutputXY)},
            {DmxOutputType.Color,typeof( DmxOutputColor)},
            {DmxOutputType.Fixture,typeof(DmxOutputFixture)},
        };
}
