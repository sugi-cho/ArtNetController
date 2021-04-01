using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using sugi.cc.udp.artnet;

[System.Serializable]
public class DmxOutputFixture : IDmxOutputModule
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
        if (dmxOutputList.Count == 0)
            dmxOutputList.Add(emptyZero);
    }
    DmxOutputEmpty emptyZero = new DmxOutputEmpty { Size = 0 };

    public void AddModule(IDmxOutputModule module)
    {
        dmxOutputList.Add(module);
        BuildDefinitions();
    }
    public void RemoveModule(IDmxOutputModule module)
    {
        dmxOutputList.Remove(module);
        if (dmxOutputList.Count == 0)
            dmxOutputList.Add(emptyZero);
        BuildDefinitions();
    }
    void BuildDefinitions()
    {
        if (dmxOutputList.Contains(emptyZero) && 1 < dmxOutputList.Count)
            dmxOutputList.Remove(emptyZero);
        dmxOutputDefinitions = dmxOutputList.Select(o =>
        {
            var type = o.GetType();
            var outputType = TypeMap.FirstOrDefault(pair => pair.Value == type).Key;
            var definition = new DmxOutputDefinition { type = outputType, label = o.Label };
            var useFine = o as IDmxOutputUseFine;
            if (useFine != null)
                definition.useFine = useFine.UseFine;
            return definition;
        }).ToArray();
    }

    public string Label { get => label; set => label = value; }
    [SerializeField] string label;
    int IDmxOutputModule.StartChannel => dmxOutputList[0].StartChannel;
    int IDmxOutputModule.NumChannels => dmxOutputList.Sum(output => output.NumChannels);
    void IDmxOutputModule.SetChannel(int channel)
    {
        (emptyZero as IDmxOutputModule).SetChannel(channel);
        foreach (var output in dmxOutputList)
        {
            output.SetChannel(channel);
            channel += output.NumChannels;
        }
    }
    void IDmxOutputModule.SetDmx(ref byte[] dmx)
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

    public enum DmxOutputType
    {
        Empty = 0,
        Bool,
        Int,
        Float,
        XY,
        Color,
    }

    readonly Dictionary<DmxOutputType, System.Type> TypeMap
        = new Dictionary<DmxOutputType, System.Type>
        {
            {DmxOutputType.Empty,typeof( DmxOutputEmpty)},
            {DmxOutputType.Bool,typeof( DmxOutputBool)},
            {DmxOutputType.Int,typeof( DmxOutputInt)},
            {DmxOutputType.Float,typeof( DmxOutputFloat)},
            {DmxOutputType.XY,typeof( DmxOutputXY)},
            {DmxOutputType.Color,typeof( DmxOutputColor)},
        };
}
