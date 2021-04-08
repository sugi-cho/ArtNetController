using UnityEngine.UIElements;
using UnityEngine;

public class DmxOutputUI<T> : DmxOutputUI where T : IDmxOutput
{
    public DmxOutputUI(T dmxOutput)
    {
        targetDmxOutput = dmxOutput;
        BuildEditorUI();
        BuildControlUI();
    }

    internal T targetDmxOutput;
    string EditorUIBaseResourcePath => $"UI/DmxOutput/DmxOutput_Editor";
    internal string EditorUIResourcePath => $"UI/DmxOutput/{typeof(T).Name}_Editor";
    internal string ControlUIResourcePath => $"UI/DmxOutput/{typeof(T).Name}_Control";
    void BuildEditorUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(EditorUIBaseResourcePath);
        editorUI = tree?.CloneTree("");

    }
    protected virtual void BuildControlUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(ControlUIResourcePath);
        controlUI = tree?.CloneTree("");
        if (controlUI == null)
            Debug.LogWarning($"Invalid path: {ControlUIResourcePath}");
    }
}

public abstract class DmxOutputUI
{
    public static DmxOutputUI<DmxOutputEmpty> Create(DmxOutputEmpty dmxOutput) => new DmxOutputUI<DmxOutputEmpty>(dmxOutput);
    public static DmxOutputBoolUI Create(DmxOutputBool dmxOutput) => new DmxOutputBoolUI(dmxOutput);
    public static DmxOutputIntUI Create(DmxOutputInt dmxOutput) => new DmxOutputIntUI(dmxOutput);
    public static DmxOutputSelectorUI Create(DmxOutputSelector dmxOutput) => new DmxOutputSelectorUI(dmxOutput);
    public static DmxOutputFloatUI Create(DmxOutputFloat dmxOutput) => new DmxOutputFloatUI(dmxOutput);
    public static DmxOutputXYUI Create(DmxOutputXY dmxOutput) => new DmxOutputXYUI(dmxOutput);
    public static DmxOutputColorUI Create(DmxOutputColor dmxOutput) => new DmxOutputColorUI(dmxOutput);

    public static DmxOutputUI Create(IDmxOutput dmxOutput)
    {
        var outputBool = dmxOutput as DmxOutputBool;
        if (outputBool != null) return Create(outputBool);
        var outputInt = dmxOutput as DmxOutputInt;
        if (outputInt != null) return Create(outputInt);
        var outputSelector = dmxOutput as DmxOutputSelector;
        if (outputSelector != null) return Create(outputSelector);
        var outputFloat = dmxOutput as DmxOutputFloat;
        if (outputFloat != null) return Create(outputFloat);
        var outputXY = dmxOutput as DmxOutputXY;
        if (outputXY != null) return Create(outputXY);
        var outputColor = dmxOutput as DmxOutputColor;
        if (outputColor != null) return Create(outputColor);

        return new DmxOutputUI<IDmxOutput>(dmxOutput);
    }

    protected VisualElement editorUI;
    protected VisualElement controlUI;
    public VisualElement EditorUI => editorUI;
    public VisualElement ControlUI => controlUI;
}