using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputSelectorUI : DmxOutputUI<DmxOutputSelector>
{
    public DmxOutputSelectorUI(DmxOutputSelector dmxOutput) : base(dmxOutput) { }
    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        selector = controlUI.Q<IntSelector>();
        var textField = controlUI.Q<TextField>();

        selector.style.flexDirection = FlexDirection.ColumnReverse;
        selector.NumChoices = targetDmxOutput.SizeProp;
        void SetSelectorSize(int size) => selector.NumChoices = size;
        targetDmxOutput.onSizePropChanged += SetSelectorSize;
        controlUI.RegisterCallback<DetachFromPanelEvent>(evt => targetDmxOutput.onSizePropChanged -= SetSelectorSize);

        selector.onValueChanged += (val) =>
        {
            targetDmxOutput.Value = val;
            if (0 < targetDmxOutput.SizeProp)
                textField.value = $"{Mathf.FloorToInt((targetDmxOutput.Value + 0.5f) / targetDmxOutput.SizeProp * 255)}";
            else
                textField.value = "0";
        };
        textField.SetEnabled(false);
        SetValue(targetDmxOutput.Value);
    }

    IntSelector selector;

    void SetValue(int value) =>
        selector.Value = value;
}
