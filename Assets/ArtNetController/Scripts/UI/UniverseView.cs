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

        var chToggles = matrixSelector.Query<Toggle>().ToList();
        var groups = chToggles.Select((toggle, ch) =>
        {
            var info = GetChannelInfo(ch);
            var targetCh = ch;
            return (info.output, info.startCh, info.endCh, targetCh, toggle);
        }).Where(info =>
        {
            if (info.output == null)
            {
                info.toggle.AddToClassList("null-channel");
                info.toggle.RegisterCallback<MouseDownEvent>(evt => Debug.Log("down"));
                info.toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        universeManager.SelectChannel(info.targetCh);
                        matrixSelector.Focus();
                    }
                });
            }
            return info.output != null;
        }).GroupBy(info => info.output, info => (info.startCh, info.endCh, info.targetCh, info.toggle));
        foreach (var group in groups)
        {
            var output = group.Key;
            var toggles = group.Select(info => info.toggle).ToList();
            foreach (var info in group)
            {
                var toggle = info.toggle;
                if (info.startCh == info.targetCh)
                    toggle.AddToClassList("start-channel");
                if (info.endCh == info.targetCh)
                    toggle.AddToClassList("end-channel");
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        universeManager.SelectOutput(output);
                    }
                    toggles.ForEach(t => t.SetValueWithoutNotify(evt.newValue));
                });
            }
        }
        clearButton.clicked += () => {
            chToggles.ForEach(t => t.SetValueWithoutNotify(false));
            universeManager.ClearSelections();
        };
    }

}
