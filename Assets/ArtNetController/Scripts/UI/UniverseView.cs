using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UniRx;

[System.Serializable]
public class UniverseView
{
    UniverseManager UniverseManager => UniverseManager.Instance;
    DmxOutputUniverse ActiveUniverse => UniverseManager.ActiveUniverse;

    VisualElement tabView;
    VisualElement channelsView;
    VisualElement infoView;

    MatrixSelector matrixSelector;
    [SerializeField] List<int> selectChannelList;
    [SerializeReference] List<IDmxOutput> selectOutputList;

    public event System.Action<List<int>, List<IDmxOutput>> onSelectionChanged;
    void OnSelectionChanged() => onSelectionChanged?.Invoke(selectChannelList, selectOutputList);

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

    #region TabView
    void BuildTabView()
    {
        var view = tabView;
        var tabGroup = view.Q<RadioButtonGroup>("TabGroup");
        var newButton = view.Q<Button>("new-button");

        void SetTabChoices()
        {
            tabGroup.choices = UniverseManager.UniverseList.Select(u => u.Label);
            tabGroup.SetValueWithoutNotify(UniverseManager.ActiveUniverseIdx);
        }
        void ResetChoices(string dummy) => SetTabChoices();
        SetTabChoices();
        tabGroup.RegisterValueChangedCallback(evt =>
        {
            UniverseManager.ActiveUniverseIdx = evt.newValue;
            ActiveUniverse.OnLabelChanged.Subscribe(ResetChoices);
        });
        newButton.clicked += () =>
        {
            UniverseManager.CreateUniverse();
            UniverseManager.ActiveUniverseIdx = UniverseManager.UniverseList.Count - 1;
            SetTabChoices();
        };
    }
    #endregion

    #region ChannelView
    void BuildChannelsView()
    {
        var view = channelsView;
        matrixSelector = view.Q<MatrixSelector>();
        var broadcastToggle = view.Q<Toggle>("broadcast-toggle");
        var ipField = view.Q<TextField>("remoteIP-field");
        var clearButton = view.Q<Button>("ClearButton");

        void SetupMatrixSelector()
        {
            matrixSelector.SetMatrix(32, 16);
            matrixSelector.Dispose();

            var selectingMultiOutput = false;
            var selectElements = matrixSelector.Query("select-element").ToList();
            var groups = selectElements.Select((vle, ch) =>
            {
                var output = ActiveUniverse.GetChannelOutput(ch);
                if (output == null)
                {
                    vle.AddToClassList("null-channel");
                    vle.RemoveFromClassList("start-channel");
                    vle.RemoveFromClassList("end-channel");
                    vle.RemoveFromClassList("output");
                    vle.style.backgroundColor = UIConfig.GetTypeColor(DmxOutputType.Empty);
                    matrixSelector.onValueChanged += (idx, val) =>
                    {
                        if (selectingMultiOutput)
                        {
                            matrixSelector.SetValue(idx, false, notify: false, singleSelect: false);
                            return;
                        }
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
                        selectingMultiOutput = true;
                        matrixSelector.SetValueFromToWithoutNotify(start, end, val);
                        if (val)
                            SelectOutput(output);
                        else
                            ReleaseOutput(output);
                    }
                };
                if (selectOutputList.Contains(output))
                    matrixSelector.SetValue(output.StartChannel, true, true, true);
            }
            matrixSelector.onSelectComplete += () =>
            {
                selectingMultiOutput = false;
                OnSelectionChanged();
            };
        }
        UniverseManager.OnActiveUniverseChanged.Subscribe(_ =>
        {
            SetupMatrixSelector();
            Clear();
        });
        UniverseManager.UniverseList.ToList().ForEach(univ =>
        {
            univ.OnEditChannel.Subscribe(_ =>
            {
                var list = ActiveUniverse.OutputList;
                SetupMatrixSelector();
                var removes = selectOutputList.Where(output => !list.Contains(output)).ToList();
                foreach (var rem in removes)
                    selectOutputList.Remove(rem);
                OnSelectionChanged();
            });
        });
        SetupMatrixSelector();

        var artnetController = Object.FindObjectOfType<ArtNetController>();
        broadcastToggle.RegisterValueChangedCallback(evt =>
        {
            artnetController.UseBroadCast = evt.newValue;
            ipField.SetEnabled(!artnetController.UseBroadCast);
        });
        ipField.RegisterValueChangedCallback(evt => artnetController.RemoteIp = evt.newValue);
        broadcastToggle.value = artnetController.UseBroadCast;
        ipField.value = artnetController.RemoteIp;
        ipField.SetEnabled(!artnetController.UseBroadCast);

        void Clear()
        {
            matrixSelector.SetAllValues(false);
            ClearSelections();
        }
        clearButton.clicked -= Clear;
        clearButton.clicked += Clear;
        Clear();
    }
    #endregion

    #region InfoView
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
        UniverseManager.OnActiveUniverseChanged.Subscribe(_ => SetupInfoFields(ActiveUniverse));
        SetupInfoFields(ActiveUniverse);
        nameField.RegisterValueChangedCallback(evt => ActiveUniverse.Label = evt.newValue);
        universeField.RegisterValueChangedCallback(evt => ActiveUniverse.Universe = (short)evt.newValue);

        void SetupControllerContainer()
        {
            controllerContainer.Clear();
            foreach (var output in ActiveUniverse.OutputList)
                controllerContainer.Add(UniverseControllerView(output));
        }
        UniverseManager.UniverseList.ToList().ForEach(univ => univ.OnEditChannel.Subscribe(_ => SetupControllerContainer()));
        UniverseManager.OnActiveUniverseChanged.Subscribe(_ => SetupControllerContainer());
        SetupControllerContainer();

        saveButton.clicked += () =>
            UniverseManager.SaveUniverse(ActiveUniverse);
    }
    VisualElement UniverseControllerView(IDmxOutput output)
    {
        var tree = Resources.Load<VisualTreeAsset>("UI/DmxOutput/UniverseControllerView");
        var view = tree.CloneTree("");

        var area = view.Q("universe-controller");
        var chField = view.Q<TextField>("info-channel__input");
        var label = view.Q<Label>("info-label");
        var removeButton = view.Q<Button>("remove-button");

        bool selected = false;

        area.style.backgroundColor = UIConfig.GetTypeColor(output.Type);

        chField.value = output.StartChannel.ToString();
        chField.RegisterValueChangedCallback(evt =>
        {
            int ch;
            if (int.TryParse(evt.newValue, out ch))
            {
                var checkCh = ActiveUniverse.IsValid(ch, output.NumChannels, output);
                if (checkCh)
                    output.StartChannel = ch;
                else
                    chField.SetValueWithoutNotify(evt.previousValue);
            }
            else
                chField.SetValueWithoutNotify(evt.previousValue);
        });
        chField.isDelayed = true;

        label.text = output.Label;
        removeButton.clicked += () =>
        {
            ActiveUniverse.OutputList.Remove(output);
            ReleaseOutput(output);
        };
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
                this.OnSelectionChanged();
                view.ReleasePointer(evt.pointerId);
            }
        });
        void OnSelectionChanged(List<int> cs, List<IDmxOutput> os)
        {
            var select = os.Contains(output);
            Select(select);
        }
        onSelectionChanged += OnSelectionChanged;
        void OnLabelChanged(string val) => label.text = val;
        var disposable = output.OnLabelChanged.Subscribe(OnLabelChanged);
        view.RegisterCallback<DetachFromPanelEvent>(evt =>
        {
            onSelectionChanged -= OnSelectionChanged;
            disposable.Dispose();
        });

        if (selectOutputList.Contains(output))
            Select(true);

        return view;
    }
    #endregion

    #region Select Channels,Outputs
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
    #endregion
}
