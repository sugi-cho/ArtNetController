using System.Linq;
using System.Collections.Generic;
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
    public List<DmxOutputUI<T>> multiEditUIs = new List<DmxOutputUI<T>>();
    public override void AddMultiTargeUIs(IEnumerable<DmxOutputUI> uis) =>
        multiEditUIs.AddRange(uis.Select(ui => ui as DmxOutputUI<T>));

    string EditorUIBaseResourcePath => $"UI/DmxOutput/DmxOutput_Editor";
    internal string EditorUIResourcePath => $"UI/DmxOutput/{typeof(T).Name}_Editor";
    internal string ControlUIResourcePath => $"UI/DmxOutput/{typeof(T).Name}_Control";
    protected virtual void BuildEditorUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(EditorUIBaseResourcePath);
        editorUI = tree?.CloneTree("");

        var uiBase = editorUI.Q("dmx-output-module");
        var typeLabel = uiBase.Q<Label>("Type");
        var labelField = uiBase.Q<TextField>("Label");
        var fineToggle = uiBase.Q<Toggle>();
        var sizeField = uiBase.Q<TextField>("SizeProp");
        var removeButton = uiBase.Q<Button>("remove-button");

        var outputType = DmxOutputUtility.GetDmxOutputType(targetDmxOutput);

        var useFine = targetDmxOutput as IUseFine;
        var sizeProp = targetDmxOutput as ISizeProp;

        typeLabel.text = outputType.ToString();
        labelField.value = targetDmxOutput.Label;
        labelField.isDelayed = true;
        labelField.RegisterValueChangedCallback(evt =>
        {
            targetDmxOutput.Label = evt.newValue;
            foreach (var ui in multiEditUIs)
            {
                ui.targetDmxOutput.Label = evt.newValue;
            }
            labelField.SetValueWithoutNotify(targetDmxOutput.Label);
        });

        if (useFine != null)
        {
            fineToggle.value = useFine.UseFine;
            fineToggle.RegisterValueChangedCallback(evt =>
                useFine.UseFine = evt.newValue);
            if (0 < multiEditUIs.Count)
                fineToggle.SetEnabled(false);
        }
        else
            fineToggle.style.display = DisplayStyle.None;
        if (sizeProp != null)
        {
            sizeField.value = sizeProp.SizeProp.ToString();
            sizeField.isDelayed = true;
            sizeField.RegisterValueChangedCallback(evt =>
            {
                int size;
                if (int.TryParse(evt.newValue, out size))
                {
                    sizeProp.SizeProp = size;
                    foreach (var ui in multiEditUIs)
                        (ui.targetDmxOutput as ISizeProp).SizeProp = size;
                    labelField.value = targetDmxOutput.Label;
                }
                else
                    sizeField.SetValueWithoutNotify(evt.previousValue);
            });
        }
        else
            sizeField.style.display = DisplayStyle.None;

        removeButton.clicked += () => { onRemoveButtonClicked?.Invoke(); };

    }
    protected virtual void BuildControlUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(ControlUIResourcePath);
        controlUI = tree?.CloneTree("");
        if (controlUI == null)
            Debug.LogWarning($"Invalid path: {ControlUIResourcePath}");

        var label = controlUI.Q<Label>();
        label.text = targetDmxOutput.Label;
        void OnLabelChanged(string val) => label.text = val;
        targetDmxOutput.onLabelChanged += OnLabelChanged;
        controlUI.RegisterCallback<DetachFromPanelEvent>(evt => targetDmxOutput.onLabelChanged -= OnLabelChanged);
    }
}

public abstract class DmxOutputUI
{
    static DmxOutputBoolUI Create(DmxOutputBool dmxOutput) => new DmxOutputBoolUI(dmxOutput);
    static DmxOutputIntUI Create(DmxOutputInt dmxOutput) => new DmxOutputIntUI(dmxOutput);
    static DmxOutputSelectorUI Create(DmxOutputSelector dmxOutput) => new DmxOutputSelectorUI(dmxOutput);
    static DmxOutputFloatUI Create(DmxOutputFloat dmxOutput) => new DmxOutputFloatUI(dmxOutput);
    static DmxOutputXYUI Create(DmxOutputXY dmxOutput) => new DmxOutputXYUI(dmxOutput);
    static DmxOutputColorUI Create(DmxOutputColor dmxOutput) => new DmxOutputColorUI(dmxOutput);
    static DmxOutputFixtureUI Create(DmxOutputFixture dmxOutput) => new DmxOutputFixtureUI(dmxOutput);

    public static DmxOutputUI CreateUI(IDmxOutput dmxOutput)
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
        var outputFixture = dmxOutput as DmxOutputFixture;
        if (outputFixture != null) return Create(outputFixture);

        return new DmxOutputUI<IDmxOutput>(dmxOutput);
    }
    public abstract void AddMultiTargeUIs(IEnumerable<DmxOutputUI> uis);
    protected VisualElement editorUI;
    protected VisualElement controlUI;
    public VisualElement EditorUI => editorUI;
    public VisualElement ControlUI => controlUI;

    public System.Action onRemoveButtonClicked;
}