using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputFixtureUI : DmxOutputUI<DmxOutputFixture>
{
    public DmxOutputFixtureUI(DmxOutputFixture dmxOutput) : base(dmxOutput) { }

    public DmxOutputUniverse TargetUniverse { get; set; }
    List<DmxOutputUI> DmxOutputUIList
    {
        get
        {
            if (m_dmxOutputUIList == null || m_dmxOutputUIList.Count != targetDmxOutput.DmxOutputList.Count)
                BuildDmxOutputUIList();
            return m_dmxOutputUIList;
        }
    }
    List<DmxOutputUI> m_dmxOutputUIList = null;
    void BuildDmxOutputUIList() => m_dmxOutputUIList =
        targetDmxOutput.DmxOutputList.Select(dmxOutput => CreateUI(dmxOutput)).ToList();
    void UpdateStructures()
    {
        targetDmxOutput.BuildDefinitions();
        editorUI.Q<Label>("info-label").text =
            $"Start Channel: {targetDmxOutput.StartChannel:000}\nNum Channels: {targetDmxOutput.NumChannels}";
    }

    void RebuildUIEditorUI()
    {
        var editorParent = editorUI?.parent;
        editorUI?.RemoveFromHierarchy();
        editorUI?.Clear();
        BuildEditorUI();
        editorParent?.Add(editorUI);
    }
    void RebuildControlUI()
    {
        var controlParent = controlUI?.parent;
        controlUI?.RemoveFromHierarchy();
        controlUI?.Clear();
        BuildControlUI();
        controlParent?.Add(controlUI);
    }
    protected override void BuildEditorUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(EditorUIResourcePath);
        editorUI = tree.CloneTree("");

        var label = editorUI.Q<Label>();
        var labelField = editorUI.Q<TextField>("label");
        var infoLabel = editorUI.Q<Label>("info-label");
        var uiContainer = editorUI.Q("output-container");
        var dropdownEdit = editorUI.Q<EditableDropdownField>();
        var saveButton = editorUI.Q<Button>("save-button");

        void SetLabel(string text)
        {
            targetDmxOutput.Label = text;
            label.text = targetDmxOutput.Label;
            onLabelChanged?.Invoke(text);
        }
        SetLabel(targetDmxOutput.Label);
        labelField.RegisterValueChangedCallback(evt => SetLabel(evt.newValue));
        labelField.value = targetDmxOutput.Label;
        labelField.isDelayed = true;

        UpdateStructures();
        for (var i = 0; i < DmxOutputUIList.Count; i++)
        {
            var idx = i;
            var dmxOutput = targetDmxOutput.DmxOutputList[i];
            var dmxOutputUI = DmxOutputUIList[i];
            uiContainer.Add(dmxOutputUI.EditorUI);
            dmxOutputUI.onRemoveButtonClicked += () =>
            {
                targetDmxOutput.RemoveModule(dmxOutput);
                RebuildUIEditorUI();
                RebuildControlUI();
            };
            dmxOutputUI.onValueChanged += UpdateStructures;
        }

        dropdownEdit.onValueCanged += (val) =>
        {
            DmxOutputType outputType;
            if (System.Enum.TryParse(val, out outputType))
            {
                var output = DmxOutputUtility.CreateDmxOutput(outputType);
                targetDmxOutput.AddModule(output);
                dropdownEdit.Q<DropdownField>().index = 0;
                RebuildUIEditorUI();
                RebuildControlUI();
            }
        };

        saveButton.clicked += () =>
         {
             targetDmxOutput.BuildDefinitions();
             FixtureLibrary.SaveFixture(targetDmxOutput);
         };
    }

    protected override void BuildControlUI()
    {
        base.BuildControlUI();
        var label = controlUI.Q<Label>();
        var uiContainer = controlUI.Q("output-container");

        label.text = targetDmxOutput.Label;
        onLabelChanged += (val) => label.text = val;
        for (var i = 0; i < DmxOutputUIList.Count; i++)
        {
            var dmxOutputUI = DmxOutputUIList[i];
            uiContainer.Add(dmxOutputUI.ControlUI);
        }
    }
}
