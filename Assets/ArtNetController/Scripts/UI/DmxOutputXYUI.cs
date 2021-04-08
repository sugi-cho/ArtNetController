using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputXYUI : DmxOutputUI<DmxOutputXY>
{
    public DmxOutputXYUI(DmxOutputXY dmxOutput) : base(dmxOutput) { }

    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        var label = controlUI.Q<Label>();
        var xyPad = controlUI.Q<XYPad>();
        var textFields = controlUI.Query<TextField>().ToList();

        label.text = targetDmxOutput.Label;
        xyPad.onValueChanged += (v2) =>
        {
            for (var i = 0; i < 2; i++)
                textFields[i].value = v2[i].ToString();
            onValueChanged?.Invoke(v2);
        };
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
            });
            textFields[idx].isDelayed = true;
        }

        xyPad.Value = targetDmxOutput.Value;
    }

    public event System.Action<Vector2> onValueChanged;
}
