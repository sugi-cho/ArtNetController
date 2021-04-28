using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct DmxOutputDefinition
{
    public DmxOutputType type;
    public string label;
    public int channel;
    public bool useFine;
    public int size;
}

public enum DmxOutputType
{
    Empty,
    Bool,
    Int,
    Selector,
    Float,
    XY,
    Color,
    Fixture,
    Universe,
}

public static class DmxOutputUtility
{
    static FixtureLibrary FixtureLibrary => FixtureLibrary.Instance;

    public readonly static Dictionary<DmxOutputType, System.Type> TypeMap
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

    public static DmxOutputType GetDmxOutputType(IDmxOutput output) =>
        TypeMap.FirstOrDefault(pair => pair.Value == output.GetType()).Key;
    public static IDmxOutput CreateDmxOutput(DmxOutputType type)
    {
        var output = System.Activator.CreateInstance(TypeMap[type]) as IDmxOutput;
        output.Label = type.ToString();
        return output;
    }
    public static IDmxOutput CreateDmxOutput(DmxOutputDefinition definition)
    {
        IDmxOutput dmxOutput;
        if (definition.type == DmxOutputType.Fixture)
            dmxOutput = FixtureLibrary.LoadFixture(definition.label);
        else
            dmxOutput = CreateDmxOutput(definition.type);

        dmxOutput.Label = definition.label;
        dmxOutput.StartChannel = definition.channel;
        var useFine = dmxOutput as IUseFine;
        if (useFine != null)
            useFine.UseFine = definition.useFine;
        var numChoices = dmxOutput as ISizeProp;
        if (numChoices != null)
            numChoices.SizeProp = definition.size;
        return dmxOutput;
    }
    public static DmxOutputDefinition CreateDmxOutputDefinitioin(IDmxOutput output)
    {
        var outputType = GetDmxOutputType(output);
        var definition = new DmxOutputDefinition { type = outputType, label = output.Label, channel = output.StartChannel };
        var useFine = output as IUseFine;
        if (useFine != null)
            definition.useFine = useFine.UseFine;
        var numChoices = output as ISizeProp;
        if (numChoices != null)
            definition.size = numChoices.SizeProp;
        return definition;
    }
}