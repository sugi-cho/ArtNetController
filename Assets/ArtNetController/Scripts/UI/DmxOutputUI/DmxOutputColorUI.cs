using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputColorUI : DmxOutputUI<DmxOutputColor>
{
    public DmxOutputColorUI(DmxOutputColor dmxOutput) : base(dmxOutput) { }
    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        label = controlUI.Q<Label>();
        background = ControlUI.Q("background");
        var inputAreas = controlUI.Query("input-area").ToList();
        valueVisualizes = controlUI.Query("value").ToList();
        textFields = controlUI.Query<TextField>().ToList();
        void PointerInput(IPointerEvent evt, int idx)
        {
            var pos = evt.localPosition;
            var value = pos.y / inputAreas[idx].localBound.height;
            value = Mathf.Clamp01(1f - value);
            var color = targetDmxOutput.Value;
            color[idx] = value;
            SetValue(color);
            if (evt.shiftKey)
                multiEditUIs.ForEach(ui => (ui as DmxOutputColorUI).SetValue(color));
        }

        for (var i = 0; i < 3; i++)
        {
            var idx = i;
            var inputArea = inputAreas[idx];
            inputArea.RegisterCallback<PointerDownEvent>(evt =>
            {
                inputArea.CapturePointer(evt.pointerId);
                PointerInput(evt, idx);
            });
            inputArea.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (inputArea.HasPointerCapture(evt.pointerId))
                    PointerInput(evt, idx);
            });
            inputArea.RegisterCallback<PointerUpEvent>(evt => inputArea.ReleasePointer(evt.pointerId));

            textFields[idx].RegisterValueChangedCallback(evt =>
            {
                float value;
                if (float.TryParse(evt.newValue, out value))
                {
                    var color = targetDmxOutput.Value;
                    color[idx] = value;
                    SetValue(color);
                }
                else
                    textFields[idx].SetValueWithoutNotify(evt.previousValue);
            });
            valueVisualizes[idx].style.backgroundColor = rgb[idx];
            textFields[idx].isDelayed = true;
            var color = rgb[idx] * 0.5f;
            color.a = 1f;
            textFields[idx].Q("unity-text-input").style.color = color;
        }
        SetValue(targetDmxOutput.Value);
    }

    Label label;
    VisualElement background;
    List<VisualElement> valueVisualizes;
    List<TextField> textFields;

    void SetValue(Color color)
    {
        for (var i = 0; i < 3; i++)
        {
            valueVisualizes[i].style.height = Length.Percent(color[i] * 100);
            textFields[i].value = color[i].ToString();
        }
        color.a = 1f;
        targetDmxOutput.Value = color;
        background.style.backgroundColor = color;
    }
    readonly Color[] rgb = new[] { Color.red, Color.green, Color.blue };
}