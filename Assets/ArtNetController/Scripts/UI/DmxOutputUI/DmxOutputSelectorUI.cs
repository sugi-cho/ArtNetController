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

        label.text = targetDmxOutput.Label;
        onLabelChanged += (val) => label.text = val;
        selector.style.flexDirection = FlexDirection.ColumnReverse;
        selector.NumChoices = targetDmxOutput.SizeProp;
        onSizePropChanged += (val) => selector.NumChoices = val;
        selector.onValueChanged += (val) =>
        {
            targetDmxOutput.Value = val;
            if (0 < targetDmxOutput.SizeProp)
                textField.value = $"{Mathf.FloorToInt((targetDmxOutput.Value + 0.5f) / targetDmxOutput.SizeProp * 255)}";
            else
                textField.value = "0";
        };
        textField.SetEnabled(false);
        selector.Value = 0;
    }
}
