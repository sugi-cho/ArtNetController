using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputXYUI : DmxOutputUI<DmxOutputXY>
{
    public DmxOutputXYUI(DmxOutputXY dmxOutput) : base(dmxOutput) { }

    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        xyPad = controlUI.Q<XYPad>();
        var textFields = controlUI.Query<TextField>().ToList();

        xyPad.onValueChanged += (value) =>
        {
            for (var i = 0; i < 2; i++)
                textFields[i].value = value[i].ToString();
            targetDmxOutput.Value = value;
        };
        xyPad.onShiftKey += v2 => multiEditUIs.ForEach(ui => (ui as DmxOutputXYUI).SetValue(v2));
        for (var i = 0; i < 2; i++)
        {
            var idx = i;
            textFields[idx].RegisterValueChangedCallback(evt =>
            {
                float value;
                if (float.TryParse(evt.newValue, out value))
                {
                    var v2 = xyPad.Value;
                    v2[idx] = value;
                    xyPad.Value = v2;
                }
                else
                    textFields[idx].SetValueWithoutNotify(evt.previousValue);
            });
            textFields[idx].isDelayed = true;
        }

        SetValue(targetDmxOutput.Value);
    }

    XYPad xyPad;

    void SetValue(Vector2 value)
    {
        xyPad.Value = value;
    }
}
