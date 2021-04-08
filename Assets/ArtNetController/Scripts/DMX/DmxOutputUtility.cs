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
}

public static class DmxOutputUtility
{
    readonly static Dictionary<DmxOutputType, System.Type> TypeMap
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

    public static IDmxOutput DefinitionToModule(DmxOutputDefinition definition)
    {
        if (definition.type == DmxOutputType.Fixture)
            return FixtureLibrary.LoadFixture(definition.label);

        var dmxOutput = System.Activator.CreateInstance(TypeMap[definition.type]) as IDmxOutput;
        dmxOutput.Label = definition.label;
        var useFine = dmxOutput as IUseFine;
        if (useFine != null)
            useFine.UseFine = definition.useFine;
        var numChoices = dmxOutput as ISizeProp;
        if (numChoices != null)
            numChoices.SizeProp = definition.size;
        return dmxOutput;
    }
    public static DmxOutputDefinition DefinitionFromModule(IDmxOutput output)
    {
        var type = output.GetType();
        var outputType = TypeMap.FirstOrDefault(pair => pair.Value == type).Key;
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