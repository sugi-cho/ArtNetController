using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputColorUI : DmxOutputUI<DmxOutputColor>
{
    public DmxOutputColorUI(DmxOutputColor dmxOutput) : base(dmxOutput) { }
    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        var label = controlUI.Q<Label>();
        var background = ControlUI.Q("background");
        var inputAreas = controlUI.Query("input-area").ToList();
        var valueVisualizes = controlUI.Query("value").ToList();
        var textFields = controlUI.Query<TextField>().ToList();

        label.text = targetDmxOutput.Label;
        
        void SetValue(Color color)
        {
            for (var i = 0; i < 3; i++)
            {
                valueVisualizes[i].style.height = Length.Percent(color[i] * 100);
                textFields[i].value = color[i].ToString();
            }
            color.a = 1f;
            targetDmxOutput.Value = color;
            onControlValueChanged?.Invoke(color);
            label.style.backgroundColor = background.style.backgroundColor = color;
            var labelColor = color;
            for (var i = 0; i < 3; i++)
                labelColor[i] = (1f - labelColor[i]);
            label.style.color = labelColor;
        }
        void PointerInput(IPointerEvent evt, int idx)
        {
            var pos = evt.localPosition;
            var value = pos.y / inputAreas[idx].localBound.height;
            value = Mathf.Clamp01(1f - value);
            var color = targetDmxOutput.Value;
            color[idx] = value;
            SetValue(color);
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
            });
            valueVisualizes[idx].style.backgroundColor = rgb[idx];
            textFields[idx].isDelayed = true;
            var color = rgb[idx] * 0.5f;
            color.a = 1f;
            textFields[idx].Q("unity-text-input").style.color = color;
        }
        SetValue(targetDmxOutput.Value);
    }

    public event System.Action<Color> onControlValueChanged;
    readonly Color[] rgb = new[] { Color.red, Color.green, Color.blue };
}