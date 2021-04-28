using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UniRx;

public class DmxOutputFixtureUI : DmxOutputUI<DmxOutputFixture>
{
    public override void AddMultiTargeUIs(IEnumerable<DmxOutputUI> uis)
    {
        for(var i = 0; i < DmxOutputUIList.Count; i++)
        {
            var dmxOutputUI = DmxOutputUIList[i];
            dmxOutputUI.AddMultiTargeUIs(uis.Select(ui => (ui as DmxOutputFixtureUI).DmxOutputUIList[i]));
        }
    }
    public override void SetParent(IDmxOutput parentOutput)
    {
        var universe = parentOutput as DmxOutputUniverse;
        if (universe != null)
            editorUI.SetEnabled(false);
    }
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
        editorUI.Q<Label>("info-label").text =
            $"Num Channels: {targetDmxOutput.NumChannels}";
    }

    protected override void BuildEditorUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(EditorUIResourcePath);
        editorUI = tree.CloneTree("");

        var labelField = editorUI.Q<TextField>("label");
        var infoLabel = editorUI.Q<Label>("info-label");
        var uiContainer = editorUI.Q("output-container");
        var dropdownEdit = editorUI.Q<EditableDropdownField>();

        void SetLabel(string text)
        {
            if (0 < FixtureLibrary.FixtureLabelList.Where(l => l.ToLower() == text.ToLower()).Count())
            {
                labelField.SetValueWithoutNotify(targetDmxOutput.Label);
                return;
            }
            targetDmxOutput.Label = text;
            foreach (var ui in multiEditUIs)
                ui.TargetDmxOutput.Label = text;
        }

        labelField.RegisterValueChangedCallback(evt => SetLabel(evt.newValue));
        labelField.SetValueWithoutNotify(targetDmxOutput.Label);
        labelField.isDelayed = true;

        UpdateStructures();
        for (var i = 0; i < DmxOutputUIList.Count; i++)
        {
            var idx = i;
            var dmxOutputUI = DmxOutputUIList[i];
            uiContainer.Add(dmxOutputUI.EditorUI);
            dmxOutputUI.SetParent(targetDmxOutput);
        }

        dropdownEdit.onValueCanged += (val) =>
        {
            DmxOutputType outputType;
            if (System.Enum.TryParse(val, out outputType))
            {
                var output = DmxOutputUtility.CreateDmxOutput(outputType);
                targetDmxOutput.AddOutput(output);
                dropdownEdit.Q<DropdownField>().index = 0;
                targetDmxOutput.NotifyEditChannel();

                var ui = CreateUI(output);
                uiContainer.Add(ui.EditorUI);
                controlUI.Q("output-container").Add(ui.ControlUI);
                ui.SetParent(targetDmxOutput);
            }
        };
        targetDmxOutput.OnEditChannel.Subscribe(_ => UpdateStructures());
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
