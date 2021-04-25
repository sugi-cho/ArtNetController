using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputFixtureUI : DmxOutputUI<DmxOutputFixture>
{
    public DmxOutputFixtureUI(DmxOutputFixture dmxOutput) : base(dmxOutput) { }
    List<DmxOutputUI> DmxOutputUIList
    {
        get
        {
            if (m_dmxOutputUIList == null || m_dmxOutputUIList.Count != targetDmxOutput.OutputList.Count)
                BuildDmxOutputUIList();
            return m_dmxOutputUIList;
        }
    }
    List<DmxOutputUI> m_dmxOutputUIList = null;
    void BuildDmxOutputUIList() => m_dmxOutputUIList =
        targetDmxOutput.OutputList.Select(dmxOutput => CreateUI(dmxOutput)).ToList();
    void UpdateStructures()
    {
        targetDmxOutput.BuildDefinitions();
        editorUI.Q<Label>("info-label").text =
            $"Num Channels: {targetDmxOutput.NumChannels}";
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
            if (FixtureLibrary.FixtureLabelList.Contains(text))
            {
                labelField.SetValueWithoutNotify(targetDmxOutput.Label);
                return;
            }
            targetDmxOutput.Label = text;
            foreach (var ui in multiEditUIs)
                ui.TargetDmxOutput.Label = text;
            label.text = targetDmxOutput.Label;
        }
        label.text = targetDmxOutput.Label;
        labelField.RegisterValueChangedCallback(evt => SetLabel(evt.newValue));
        labelField.SetValueWithoutNotify(targetDmxOutput.Label);
        labelField.isDelayed = true;

        UpdateStructures();
        for (var i = 0; i < DmxOutputUIList.Count; i++)
        {
            var idx = i;
            var dmxOutput = targetDmxOutput.OutputList[i];
            var dmxOutputUI = DmxOutputUIList[i];
            uiContainer.Add(dmxOutputUI.EditorUI);
            dmxOutputUI.SetParent(targetDmxOutput);
            dmxOutputUI.TargetDmxOutput.onLabelChanged += (val) => UpdateStructures();
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
        var uiContainer = controlUI.Q("output-container");

        for (var i = 0; i < DmxOutputUIList.Count; i++)
        {
            var dmxOutputUI = DmxOutputUIList[i];
            uiContainer.Add(dmxOutputUI.ControlUI);
        }
    }
}
