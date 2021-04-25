using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputFloatUI : DmxOutputUI<DmxOutputFloat>
{
    public DmxOutputFloatUI(DmxOutputFloat dmxOutput) : base(dmxOutput) { }
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
            value = Mathf.Clamp01(1f - value);
            SetValue(value);
            if (evt.shiftKey)
                multiEditUIs.ForEach(ui => (ui as DmxOutputFloatUI).SetValue(value));
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
            float value;
            if (float.TryParse(evt.newValue, out value))
                SetValue(value);
            else
                textField.SetValueWithoutNotify(evt.previousValue);
        });
        textField.isDelayed = true;
        SetValue(targetDmxOutput.Value);
    }

    VisualElement valueVisualize;
    TextField textField;

    void SetValue(float value)
    {
        valueVisualize.style.height = Length.Percent(value * 100);
        textField.value = value.ToString();
        targetDmxOutput.Value = value;
    }
}