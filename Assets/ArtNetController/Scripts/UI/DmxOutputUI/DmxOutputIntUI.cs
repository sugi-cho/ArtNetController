using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputIntUI : DmxOutputUI<DmxOutputInt>
{
    public DmxOutputIntUI(DmxOutputInt dmxOutput) : base(dmxOutput) { }
    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        var inputArea = controlUI.Q("input-area");
        valueVisualize = controlUI.Q("value");
        textField = controlUI.Q<TextField>();

        void PointerInput(IPointerEvent evt)
        {
            var pos = evt.localPosition;
            var value = pos.y / inputArea.localBound.height;
            value = Mathf.Clamp01(1f - value) * 255;
            SetValue((int)value);
            if (evt.shiftKey)
                multiEditUIs.ForEach(ui => (ui as DmxOutputIntUI).SetValue((int)value));
        }

        inputArea.RegisterCallback<PointerDownEvent>(evt =>
        {
            inputArea.CapturePointer(evt.pointerId);
            PointerInput(evt);
        });
        inputArea.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (inputArea.HasPointerCapture(evt.pointerId))
                PointerInput(evt);
        });
        inputArea.RegisterCallback<PointerUpEvent>(evt => inputArea.ReleasePointer(evt.pointerId));

        textField.RegisterValueChangedCallback(evt =>
        {
            int value;
            if (int.TryParse(evt.newValue, out value))
                SetValue(value);
            else
                textField.SetValueWithoutNotify(evt.previousValue);
        });
        textField.isDelayed = true;
        SetValue(targetDmxOutput.Value);
    }

    VisualElement valueVisualize;
    TextField textField;

    void SetValue(int value)
    {
        valueVisualize.style.height = Length.Percent(value * 100 / 255);
        textField.value = value.ToString();
        targetDmxOutput.Value = value;
    }
}