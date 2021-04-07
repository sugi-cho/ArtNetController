using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct DmxOutputDefinition
{
    public DmxOutputType type;
    public string label;
    public int channel;
    public bool useFine;
    public int numChoices;
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

    public static IDmxOutputModule DefinitionToModule(DmxOutputDefinition definition)
    {
        if (definition.type == DmxOutputType.Fixture)
            return FixtureLibrary.LoadFixture(definition.label);

        var dmxOutput = System.Activator.CreateInstance(TypeMap[definition.type]) as IDmxOutputModule;
        dmxOutput.Label = definition.label;
        var useFine = dmxOutput as IUseFine;
        if (useFine != null)
            useFine.UseFine = definition.useFine;
        var numChoices = dmxOutput as INumChoices;
        if (numChoices != null)
            numChoices.NumChoices = definition.numChoices;
        return dmxOutput;
    }
    public static DmxOutputDefinition DefinitionFromModule(IDmxOutputModule output)
    {
        var type = output.GetType();
        var outputType = TypeMap.FirstOrDefault(pair => pair.Value == type).Key;
        var definition = new DmxOutputDefinition { type = outputType, label = output.Label, channel = output.StartChannel };
        var useFine = output as IUseFine;
        if (useFine != null)
            definition.useFine = useFine.UseFine;
        var numChoices = output as INumChoices;
        if (numChoices != null)
            definition.numChoices = numChoices.NumChoices;
        return definition;
    }
}