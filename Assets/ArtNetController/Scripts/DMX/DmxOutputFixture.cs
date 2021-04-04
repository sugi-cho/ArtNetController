using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DmxOutputFixture : IDmxOutputModule
{
    public bool IsEmpty => dmxOutputList[0] == emptyOne;
    public void Initialize()
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
        if (dmxOutputList.Count == 0)
            dmxOutputList.Add(emptyOne);
    }
    DmxOutputEmpty emptyOne = new DmxOutputEmpty { Size = 1 };

    public List<IDmxOutputModule> DmxOutputModules => dmxOutputList;
    public void AddModule(IDmxOutputModule module)
    {
        dmxOutputList.Add(module);
        BuildDefinitions();
    }
    public void RemoveModule(IDmxOutputModule module)
    {
        dmxOutputList.Remove(module);
        if (dmxOutputList.Count == 0)
            dmxOutputList.Add(emptyOne);
        BuildDefinitions();
    }
    void BuildDefinitions()
    {
        if (dmxOutputList.Contains(emptyOne) && 1 < dmxOutputList.Count)
            dmxOutputList.Remove(emptyOne);
        dmxOutputDefinitions = dmxOutputList.Select(output =>
        {
            var type = output.GetType();
            var outputType = TypeMap.FirstOrDefault(pair => pair.Value == type).Key;
            var definition = new DmxOutputDefinition { type = outputType, label = output.Label };
            var useFine = output as IDmxOutputUseFine;
            if (useFine != null)
                definition.useFine = useFine.UseFine;
            return definition;
        }).ToArray();
    }

    public string Label { get => label; set => label = value; }
    [SerializeField] string label;
    public int StartChannel {
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

    [System.Serializable]
    public struct DmxOutputDefinition
    {
        public DmxOutputType type;
        public string label;
        public bool useFine;
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
