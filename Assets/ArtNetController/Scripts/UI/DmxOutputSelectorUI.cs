using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputSelectorUI : DmxOutputUI<DmxOutputSelector>
{
    public DmxOutputSelectorUI(DmxOutputSelector dmxOutput) : base(dmxOutput) { }
    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        var label = controlUI.Q<Label>();
        var selector = controlUI.Q<IntSelector>();
        var textField = controlUI.Q<TextField>();

        label.text = targetDmxOutput.Label.Split('?')[0];
        selector.style.flexDirection = FlexDirection.ColumnReverse;
        selector.NumChoices = targetDmxOutput.SizeProp;
        selector.onValueChanged += (val) =>
        {
            targetDmxOutput.Value = val;
            textField.value = $"{Mathf.FloorToInt((targetDmxOutput.Value + 0.5f) / targetDmxOutput.SizeProp * 255)}";
            onValueChaned?.Invoke(val);
        };
        textField.SetEnabled(false);
        selector.Value = 0;
    }

    public event System.Action<int> onValueChaned;
}
