using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputFloatUI : DmxOutputUI<DmxOutputFloat>
{
    public DmxOutputFloatUI(DmxOutputFloat dmxOutput) : base(dmxOutput) { }
    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        var inputArea = controlUI.Q("input-area");
        var valueVisualize = controlUI.Q("value");
        var textField = controlUI.Q<TextField>();

        void SetValue(float value)
        {
            valueVisualize.style.height = Length.Percent(value * 100);
            textField.value = value.ToString();
            targetDmxOutput.Value = value;
        }
        void PointerInput(IPointerEvent evt)
        {
            var pos = evt.localPosition;
            var value = pos.y / inputArea.localBound.height;
            value = Mathf.Clamp01(1f - value);
            SetValue(value);
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
}