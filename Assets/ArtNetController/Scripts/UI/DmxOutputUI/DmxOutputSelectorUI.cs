using UnityEngine;
using UnityEngine.UIElements;
using UniRx;

public class DmxOutputSelectorUI : DmxOutputUI<DmxOutputSelector>
{
    public DmxOutputSelectorUI(DmxOutputSelector dmxOutput) : base(dmxOutput) { }
    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        selector = controlUI.Q<IntSelector>();
        var textField = controlUI.Q<TextField>();

        selector.NumChoices = targetDmxOutput.SizeProp;
        void SetSelectorSize(int size) => selector.NumChoices = size;
        var disposable = targetDmxOutput.OnSizePropChanged.Subscribe(SetSelectorSize);
        controlUI.RegisterCallback<DetachFromPanelEvent>(evt => disposable.Dispose());

        selector.onValueChanged += (val) =>
        {
            targetDmxOutput.Value = val;
            if (0 < targetDmxOutput.SizeProp)
                textField.value = $"{Mathf.FloorToInt((targetDmxOutput.Value + 0.5f) / targetDmxOutput.SizeProp * 255)}";
            else
                textField.value = "0";
        };
        selector.onShiftKey += val => multiEditUIs.ForEach(ui => (ui as DmxOutputSelectorUI).SetValue(val));
        textField.SetEnabled(false);
        SetValue(targetDmxOutput.Value);
    }

    IntSelector selector;

    void SetValue(int value) =>
        selector.Value = value;
}
