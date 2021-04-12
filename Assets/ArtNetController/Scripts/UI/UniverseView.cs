using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UniverseView
{
    public void BuildUI(VisualElement view)
    {
        var universeManager = UniverseManager.Instance;

        var tabGroup = view.Q<RadioButtonGroup>("TabGroup");
        var matrixSelector = view.Q<MatrixSelector>();
        var clearButton = view.Q<Button>("ClearButton");

        var activeUniverse = universeManager.ActiveUniverse;
        var outputList = activeUniverse.OutputList;

        tabGroup.choices = universeManager.Universes.Select(u => u.Label);
        tabGroup.SetValueWithoutNotify(universeManager.ActiveUniverseIdx);


        (IDmxOutput output, int startCh, int endCh) GetChannelInfo(int ch)
        {
            (IDmxOutput output, int startCh, int endCh) info = outputList
                .Select(o => (o, o.StartChannel, o.StartChannel + o.NumChannels - 1))
                .FirstOrDefault(info => info.StartChannel <= ch && ch <= info.Item3);
            if (info.output == null)
                info.startCh = info.endCh = ch;
            return info;
        }

        var selectElements = matrixSelector.Query("select-element").ToList();
        var groups = selectElements.Select((vle, idx) =>
        {
            var info = GetChannelInfo(idx);
            return (info.output, info.startCh, info.endCh, idx, vle);
        }).Where(info =>
        {
            if (info.output == null)
                info.vle.AddToClassList("null-channel");
            return info.output != null;
        }).GroupBy(info => info.output, info => (info.startCh, info.endCh, info.idx, info.vle));
        foreach (var group in groups)
        {
            var output = group.Key;
            group.FirstOrDefault(g => g.idx == g.startCh).vle.AddToClassList("start-channel");
            group.FirstOrDefault(g => g.idx == g.endCh).vle.AddToClassList("end-channel");
            var start = output.StartChannel;
            var end = output.StartChannel + output.NumChannels - 1;
            matrixSelector.onValueChanged += (idx, val) =>
            {
                if (start <= idx && idx <= end)
                {
                    matrixSelector.SetValueFromTo(start, end, val);
                    universeManager.SelectOutput(output);
                }
            };
        }
        clearButton.clicked += () => {
            matrixSelector.SetAllValues(false);
            universeManager.ClearSelections();
        };
    }

}
