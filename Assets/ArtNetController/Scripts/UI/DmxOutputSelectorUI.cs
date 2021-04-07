using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputSelectorUI : DmxOutputUIBase<DmxOutputSelector>
{
    public DmxOutputSelectorUI(DmxOutputSelector dmxOutput) : base(dmxOutput) { }
    protected override void BuildEditorUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(EditorUIResourcePath);
        editorUI = tree?.CloneTree("");
    }

    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        var label = controlUI.Q<Label>();
        var selector = controlUI.Q<IntSelector>();
        var textField = controlUI.Q<TextField>();

        label.text = targetDmxOutput.Label.Split('?')[0];
        selector.style.flexDirection = FlexDirection.ColumnReverse;
        selector.NumChoices = targetDmxOutput.NumChoices;
        selector.onValueChanged += (val) =>
        {
            targetDmxOutput.Value = val;
            textField.value = $"{Mathf.FloorToInt((targetDmxOutput.Value + 0.5f) / targetDmxOutput.NumChoices * 255)}";
            onValueChaned?.Invoke(val);
        };
        textField.SetEnabled(false);
        selector.Value = 0;
    }

    public event System.Action<int> onValueChaned;
}
