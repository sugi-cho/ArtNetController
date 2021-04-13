using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class UniverseView
{
    UniverseManager universeManager => UniverseManager.Instance;
    DmxOutputUniverse activeUniverse => universeManager.ActiveUniverse;
    List<IDmxOutput> outputList => universeManager.ActiveUniverse.OutputList;

    VisualElement tabView;
    VisualElement channelsView;
    VisualElement infoView;

    MatrixSelector matrixSelector;
    List<int> selectChannelList;
    List<IDmxOutput> selectOutputList;

    public event System.Action<List<int>, List<IDmxOutput>> onSelectionChanged;

    public void BuildUI(VisualElement view)
    {
        selectChannelList = new List<int>();
        selectOutputList = new List<IDmxOutput>();

        tabView = view.Q("TabView");
        channelsView = view.Q("ChannelsView");
        infoView = view.Q("UniverseInfo");
        BuildTabView();
        BuildChannelsView();
        BuildInfoView();
    }
    void BuildTabView()
    {
        var view = tabView;
        var tabGroup = view.Q<RadioButtonGroup>("TabGroup");
        var newButton = view.Q<Button>("new-button");

        tabGroup.choices = universeManager.Universes.Select(u => u.Label);
        tabGroup.SetValueWithoutNotify(universeManager.ActiveUniverseIdx);
        tabGroup.RegisterValueChangedCallback(evt =>
        {
            universeManager.ActiveUniverseIdx = evt.newValue;

            BuildChannelsView();
            BuildInfoView();
        });
        newButton.clicked += () =>
        {
            universeManager.CreateUniverse();
            universeManager.ActiveUniverseIdx = universeManager.Universes.Count - 1;
            tabGroup.choices = universeManager.Universes.Select(u => u.Label);
            tabGroup.SetValueWithoutNotify(universeManager.ActiveUniverseIdx);

            BuildChannelsView();
            BuildInfoView();
        };
    }
    void BuildChannelsView()
    {
        var view = channelsView;
        matrixSelector = view.Q<MatrixSelector>();
        var clearButton = view.Q<Button>("ClearButton");

        (IDmxOutput output, int startCh, int endCh) GetChannelInfo(int ch) => outputList
                .Select(o => (o, o.StartChannel, o.StartChannel + o.NumChannels - 1))
                .FirstOrDefault(info => info.StartChannel <= ch && ch <= info.Item3);

        var selectElements = matrixSelector.Query("select-element").ToList();
        var groups = selectElements.Select((vle, idx) =>
        {
            var info = GetChannelInfo(idx);
            return (info.output, info.startCh, info.endCh, idx, vle);
        }).Where(info =>
        {
            if (info.output == null)
            {
                info.vle.AddToClassList("null-channel");
                matrixSelector.onValueChanged += (idx, val) =>
                {
                    if (idx == info.idx)
                        if (val)
                            SelectChannel(idx);
                        else
                            ReleaseChannel(idx);
                };
            }
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
                    matrixSelector.SetValueFromToWithoutNotify(start, end, val);
                    if (val)
                        SelectOutput(output);
                    else
                        ReleaseOutput(output);
                }
            };
        }
        void Clear()
        {
            matrixSelector.SetAllValues(false);
            ClearSelections();
        }
        clearButton.clicked += Clear;
        Clear();
    }
    void BuildInfoView()
    {
        var view = infoView;
        var nameField = view.Q<TextField>("info-name");
        var universeField = view.Q<SliderInt>("info-universe");
        var controllerContainer = view.Q("container");
        var saveButton = view.Q<Button>("save-button");

        nameField.value = activeUniverse.Label;
        nameField.RegisterValueChangedCallback(evt => activeUniverse.Label = evt.newValue);
        universeField.value = activeUniverse.Universe;
        universeField.RegisterValueChangedCallback(evt => activeUniverse.Universe = evt.newValue);
        saveButton.clicked += () =>
        {
            universeManager.SaveUniverse(activeUniverse);
            BuildTabView();
        };

        controllerContainer.Clear();
        foreach (var output in outputList)
        {
            controllerContainer.Add(UniverseControllerView(output));
        }
    }

    void ClearSelections()
    {
        selectChannelList.Clear();
        selectOutputList.Clear();
        onSelectionChanged?.Invoke(selectChannelList, selectOutputList);
    }
    void SelectChannel(int channel)
    {
        if (!selectChannelList.Contains(channel))
            selectChannelList.Add(channel);
    }
    void ReleaseChannel(int channel)
    {
        if (selectChannelList.Contains(channel))
            selectChannelList.Remove(channel);
    }
    void SelectOutput(IDmxOutput output)
    {
        if (!selectOutputList.Contains(output))
            selectOutputList.Add(output);
    }
    void ReleaseOutput(IDmxOutput output)
    {
        if (selectOutputList.Contains(output))
            selectOutputList.Remove(output);
    }
    VisualElement UniverseControllerView(IDmxOutput output)
    {
        var tree = Resources.Load<VisualTreeAsset>("UI/DmxOutput/UniverseControllerView");
        var view = tree.CloneTree("");

        var chField = view.Q<TextField>("info-channel__input");
        var label = view.Q<Label>("info-label");
        var removeButton = view.Q<Button>("remove-button");

        chField.value = output.StartChannel.ToString();
        label.text = output.Label;
        removeButton.clicked += () => activeUniverse.RemoveModule(output);

        return view;
    }

}
