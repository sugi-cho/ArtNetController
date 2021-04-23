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

        void SetTabChoices()
        {
            tabGroup.choices = universeManager.Universes.Select(u => u.Label);
            tabGroup.SetValueWithoutNotify(universeManager.ActiveUniverseIdx);
        }
        void ResetChoices(string dummy) => SetTabChoices();
        SetTabChoices();
        tabGroup.RegisterValueChangedCallback(evt =>
        {
            activeUniverse.onLabelChanged -= ResetChoices;
            universeManager.ActiveUniverseIdx = evt.newValue;
            activeUniverse.onLabelChanged += ResetChoices;
        });
        newButton.clicked += () =>
        {
            universeManager.CreateUniverse();
            universeManager.ActiveUniverseIdx = universeManager.Universes.Count - 1;
            SetTabChoices();
        };
    }

    void BuildChannelsView()
    {
        var view = channelsView;
        matrixSelector = view.Q<MatrixSelector>();
        var clearButton = view.Q<Button>("ClearButton");

        void SetupMatrixSelector()
        {
            matrixSelector.SetMatrix(32, 16);
            matrixSelector.Dispose();

            var selectElements = matrixSelector.Query("select-element").ToList();
            var groups = selectElements.Select((vle, ch) =>
            {
                var output = activeUniverse.GetChannelOutput(ch);
                if (output == null)
                {
                    vle.AddToClassList("null-channel");
                    vle.RemoveFromClassList("start-channel");
                    vle.RemoveFromClassList("end-channel");
                    vle.RemoveFromClassList("output");
                    vle.style.backgroundColor = UIConfig.GetTypeColor(DmxOutputType.Empty);
                    matrixSelector.onValueChanged += (idx, val) =>
                    {
                        if (idx == ch)
                            if (val)
                                SelectChannel(idx);
                            else
                                ReleaseChannel(idx);
                    };
                }
                else
                    vle.RemoveFromClassList("null-channel");
                return (output, ch, vle);
            }).Where(info => info.output != null)
            .GroupBy(info => info.output, info => (info.ch, info.vle));

            foreach (var group in groups)
            {
                var output = group.Key;
                group.First(g => g.ch == output.StartChannel).vle.AddToClassList("start-channel");
                group.First(g => g.ch == output.StartChannel + output.NumChannels - 1).vle.AddToClassList("end-channel");
                group.ToList().ForEach(info =>
                {
                    info.vle.AddToClassList("output");
                    info.vle.style.backgroundColor = UIConfig.GetTypeColor(output.Type);
                });
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
        }
        universeManager.onActiveUniverseChanged += univ => SetupMatrixSelector();
        activeUniverse.onEditOutputList += list => SetupMatrixSelector();
        SetupMatrixSelector();


        void Clear()
        {
            matrixSelector.SetAllValues(false);
            ClearSelections();
        }
        clearButton.clicked -= Clear;
        clearButton.clicked += Clear;
        Clear();
        matrixSelector.onSelectComplete += () => onSelectionChanged?.Invoke(selectChannelList, selectOutputList);
    }
    void BuildInfoView()
    {
        var view = infoView;
        var nameField = view.Q<TextField>("info-name");
        var universeField = view.Q<SliderInt>("info-universe");
        var controllerContainer = view.Q("container");
        var saveButton = view.Q<Button>("save-button");

        void SetupInfoFields(DmxOutputUniverse universe)
        {
            nameField.value = universe.Label;
            universeField.value = universe.Universe;
        }
        universeManager.onActiveUniverseChanged += SetupInfoFields;
        SetupInfoFields(activeUniverse);
        nameField.RegisterValueChangedCallback(evt => activeUniverse.Label = evt.newValue);
        universeField.RegisterValueChangedCallback(evt => activeUniverse.Universe = evt.newValue);

        void SetupControllerContainer(List<IDmxOutput> outputList)
        {
            controllerContainer.Clear();
            foreach (var output in outputList)
                controllerContainer.Add(UniverseControllerView(output));
        }
        universeManager.onActiveUniverseChanged += univ => SetupControllerContainer(univ.OutputList);
        activeUniverse.onEditOutputList += SetupControllerContainer;
        SetupControllerContainer(activeUniverse.OutputList);

        saveButton.clicked += () =>
            universeManager.SaveUniverse(activeUniverse);
    }
    void ClearChannelSelections()
    {
        selectChannelList.ForEach(ch => matrixSelector.SetValue(ch, false, false, false));
        selectChannelList.Clear();
    }
    void ClearOutputSelections()
    {
        selectOutputList.ForEach(output =>
            matrixSelector.SetValueFromToWithoutNotify(output.StartChannel, output.StartChannel + output.NumChannels - 1, false));
        selectOutputList.Clear();
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
        ClearOutputSelections();
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
        matrixSelector.SetValueFromToWithoutNotify(output.StartChannel, output.StartChannel + output.NumChannels - 1, true);
        ClearChannelSelections();
    }
    void ReleaseOutput(IDmxOutput output)
    {
        if (selectOutputList.Contains(output))
            selectOutputList.Remove(output);
        matrixSelector.SetValueFromToWithoutNotify(output.StartChannel, output.StartChannel + output.NumChannels - 1, false);
    }
    VisualElement UniverseControllerView(IDmxOutput output)
    {
        var tree = Resources.Load<VisualTreeAsset>("UI/DmxOutput/UniverseControllerView");
        var view = tree.CloneTree("");

        var chField = view.Q<TextField>("info-channel__input");
        var label = view.Q<Label>("info-label");
        var removeButton = view.Q<Button>("remove-button");

        bool selected = false;

        chField.value = output.StartChannel.ToString();
        label.text = output.Label;
        removeButton.clicked += () => activeUniverse.RemoveModule(output);
        view.RegisterCallback<PointerDownEvent>(evt => view.CapturePointer(evt.pointerId));

        void Select(bool select)
        {
            if (select)
            {
                view.AddToClassList("selected");
                SelectOutput(output);
            }
            else
            {
                view.RemoveFromClassList("selected");
                ReleaseOutput(output);
            }
            selected = select;
        }
        view.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (view.HasPointerCapture(evt.pointerId))
            {
                Select(!selected);
                onSelectionChanged?.Invoke(selectChannelList, selectOutputList);
                view.ReleasePointer(evt.pointerId);
            }
        });
        void OnSelectionChanged(List<int> cs, List<IDmxOutput> os)
        {
            var select = os.Contains(output);
            Select(select);
        }
        onSelectionChanged += OnSelectionChanged;
        view.RegisterCallback<DetachFromPanelEvent>(evt => onSelectionChanged -= OnSelectionChanged);

        return view;
    }
}
