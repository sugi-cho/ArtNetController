using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UniRx;

public class DmxOutputUI<T> : DmxOutputUI where T : IDmxOutput
{
    public override IDmxOutput TargetDmxOutput => targetDmxOutput;
    protected T targetDmxOutput;
    public List<DmxOutputUI<T>> multiEditUIs = new List<DmxOutputUI<T>>();

    string EditorUIBaseResourcePath => $"UI/DmxOutput/DmxOutput_Editor";
    internal string EditorUIResourcePath => $"UI/DmxOutput/{typeof(T).Name}_Editor";
    internal string ControlUIResourcePath => $"UI/DmxOutput/{typeof(T).Name}_Control";

    Label typeLabel;
    Toggle fineToggle;
    TextField sizeField;
    Button removeButton;

    #region Constructor
    public DmxOutputUI(T dmxOutput)
    {
        targetDmxOutput = dmxOutput;
        BuildEditorUI();
        BuildControlUI();
    }

    protected virtual void BuildEditorUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(EditorUIBaseResourcePath);
        editorUI = tree?.CloneTree("");

        var uiBase = editorUI.Q("dmx-output-module");
        typeLabel = uiBase.Q<Label>("Type");
        var labelField = uiBase.Q<TextField>("Label");
        fineToggle = uiBase.Q<Toggle>();
        sizeField = uiBase.Q<TextField>("SizeProp");
        removeButton = uiBase.Q<Button>("remove-button");

        var outputType = DmxOutputUtility.GetDmxOutputType(TargetDmxOutput);

        var useFine = TargetDmxOutput as IUseFine;
        var sizeProp = TargetDmxOutput as ISizeProp;

        uiBase.style.backgroundColor = UIConfig.GetTypeColor(TargetDmxOutput.Type);
        typeLabel.text = $"{targetDmxOutput.StartChannel}_{outputType}";

        labelField.value = TargetDmxOutput.Label;
        labelField.isDelayed = true;
        labelField.RegisterValueChangedCallback(evt =>
        {
            TargetDmxOutput.Label = evt.newValue;
            foreach (var ui in multiEditUIs)
                ui.TargetDmxOutput.Label = evt.newValue;
            labelField.SetValueWithoutNotify(TargetDmxOutput.Label);
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
                    foreach (var output in multiEditUIs)
                        (output as ISizeProp).SizeProp = size;
                    labelField.value = TargetDmxOutput.Label;
                }
                else
                    sizeField.SetValueWithoutNotify(evt.previousValue);
            });
        }
        else
            sizeField.style.display = DisplayStyle.None;

    }

    protected virtual void BuildControlUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(ControlUIResourcePath);
        controlUI = tree?.CloneTree("");
        if (controlUI == null)
        {
            controlUI = new VisualElement();
            return;
        }

        var uiBase = controlUI.Q("dmx-output-module");
        var label = controlUI.Q<Label>();

        uiBase.style.backgroundColor = UIConfig.GetTypeColor(TargetDmxOutput.Type);
        label.text = $"{TargetDmxOutput.Label} ({TargetDmxOutput.StartChannel})";
        void OnLabelChanged(string val) => label.text = $"{TargetDmxOutput.Label} ({TargetDmxOutput.StartChannel})";
        var disposable = TargetDmxOutput.OnLabelChanged.Subscribe(OnLabelChanged);
        controlUI.RegisterCallback<DetachFromPanelEvent>(evt => disposable.Dispose());
    }
    #endregion

    public override void AddMultiTargeUIs(IEnumerable<DmxOutputUI> uis) =>
        multiEditUIs.AddRange(uis.Select(ui => ui as DmxOutputUI<T>));

    public override void SetParent(IDmxOutput parentOutput)
    {
        var universe = parentOutput as DmxOutputUniverse;
        var fixture = parentOutput as DmxOutputFixture;
        if (universe != null)
        {
            removeButton.clicked += () =>
            {
                universe.OutputList.Remove(TargetDmxOutput);
                foreach (var ui in multiEditUIs)
                    universe.OutputList.Remove(ui.TargetDmxOutput);
            };
            fineToggle.SetEnabled(false);
        }
        if (fixture != null)
        {
            removeButton.clicked += () =>
            {
                fixture.OutputList.Remove(TargetDmxOutput);
                editorUI.RemoveFromHierarchy();
                controlUI.RemoveFromHierarchy();
            };
            fineToggle.SetEnabled(true);

            var outputType = DmxOutputUtility.GetDmxOutputType(TargetDmxOutput);
            fixture.OnEditChannel.Subscribe(_ =>
                typeLabel.text = $"{TargetDmxOutput.StartChannel}_{outputType}");
        }
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
    public abstract IDmxOutput TargetDmxOutput { get; }
    protected VisualElement editorUI;
    protected VisualElement controlUI;
    public VisualElement EditorUI => editorUI;
    public VisualElement ControlUI => controlUI;
    public abstract void SetParent(IDmxOutput parentOutput);
}