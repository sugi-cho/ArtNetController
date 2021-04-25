using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputBoolUI : DmxOutputUI<DmxOutputBool>
{
    public DmxOutputBoolUI(DmxOutputBool dmxOutput) : base(dmxOutput) { }

    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        area = controlUI.Q("input-area");
        var toggle = controlUI.Q("toggle-switch");
        var trigger = controlUI.Q("trigger");
        textField = controlUI.Q<TextField>();


        toggle.RegisterCallback<PointerUpEvent>(evt =>
        {
            var shift = evt.shiftKey;
            var val = !targetDmxOutput.Value;
            SetValue(val);
            if (shift)
                multiEditUIs.ForEach(ui => (ui as DmxOutputBoolUI).SetValue(val));
        });
        trigger.RegisterCallback<PointerDownEvent>(evt =>
        {
            var shift = evt.shiftKey;
            trigger.CapturePointer(evt.pointerId);
            SetValue(true);
            if (shift)
                multiEditUIs.ForEach(ui => (ui as DmxOutputBoolUI).SetValue(true));
        });
        trigger.RegisterCallback<PointerUpEvent>(evt =>
        {
            var shift = evt.shiftKey;
            trigger.ReleasePointer(evt.pointerId);
            SetValue(false);
            if (shift)
                multiEditUIs.ForEach(ui => (ui as DmxOutputBoolUI).SetValue(false));
        });
        textField.isDelayed = true;
        textField.SetEnabled(false);
        SetValue(targetDmxOutput.Value);
    }

    VisualElement area;
    TextField textField;

    void SetValue(bool val)
    {
        if (val)
        {
            area.RemoveFromClassList("switch-off");
            area.AddToClassList("switch-on");
        }
        else
        {
            area.RemoveFromClassList("switch-on");
            area.AddToClassList("switch-off");
        }
        targetDmxOutput.Value = val;
        textField.value = (val ? 255 : 0).ToString();
    }
}
