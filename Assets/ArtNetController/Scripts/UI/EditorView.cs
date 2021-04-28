using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UniRx;

[System.Serializable]
public class EditorView
{
    DmxOutputUniverse activeUniverse => UniverseManager.Instance.ActiveUniverse;

    ScrollView baseScroll;
    VisualElement addOutputView;
    VisualElement addFixtureView;
    VisualElement fixtureEditorView;
    VisualElement editorUIContainer;

    List<int> activeChannels;

    public void BuildUI(VisualElement view)
    {
        baseScroll = view.Q<ScrollView>("base-scroll");

        addOutputView = view.Q("AddOutputView");
        addFixtureView = view.Q("AddFixtureView");
        fixtureEditorView = view.Q("FixtureEditorView");
        editorUIContainer = view.Q("EditorUIContainer");
        BuildAddOutputView();
        BuildAddFixtureView();
        BuildFixtureEditorView();
    }
    void BuildAddOutputView()
    {
        var definition = new DmxOutputDefinition();

        var view = addOutputView;
        var outputTypeField = view.Q<EditableDropdownField>();
        var labelField = view.Q<TextField>("label-field");
        var useFineField = view.Q<Toggle>("use-fine");
        var sizePropField = view.Q<TextField>("size-prop");
        var addButton = view.Q<Button>("add-button");

        outputTypeField.onValueCanged += (val) =>
        {
            DmxOutputType outputType;
            if (System.Enum.TryParse(val, out outputType))
            {
                definition.type = outputType;
                try
                {
                    DmxOutputUtility.TypeMap[outputType].GetInterfaceMap(typeof(IUseFine));
                    useFineField.SetEnabled(true);
                }
                catch
                {
                    useFineField.SetEnabled(false);
                }
                try
                {
                    DmxOutputUtility.TypeMap[outputType].GetInterfaceMap(typeof(ISizeProp));
                    sizePropField.SetEnabled(true);
                }
                catch
                {
                    sizePropField.SetEnabled(false);
                }
                labelField.value = outputType.ToString();
                addButton.SetEnabled(true);
            }
            else
            {
                labelField.value = "Name";
                useFineField.SetEnabled(false);
                sizePropField.SetEnabled(false);
                addButton.SetEnabled(false);
            }
        };

        labelField.RegisterValueChangedCallback(evt => definition.label = evt.newValue);
        useFineField.RegisterValueChangedCallback(evt => definition.useFine = evt.newValue);
        sizePropField.RegisterValueChangedCallback(evt =>
        {
            int size;
            if (int.TryParse(evt.newValue, out size))
                definition.size = size;
            else
                sizePropField.SetValueWithoutNotify(evt.previousValue);
        });
        addButton.clicked += () =>
        {
            var ch = activeChannels[0];
            while (activeChannels.Contains(ch))
            {
                var output = DmxOutputUtility.CreateDmxOutput(definition);
                output.StartChannel = ch;
                if (Enumerable.Range(ch, output.NumChannels).All(idx => activeUniverse.IsEmpty(idx)))
                    activeUniverse.AddOutput(output);
                ch += output.NumChannels;
            }
            activeUniverse.NotifyEditChannel();
        };

        labelField.value = "Name";
        useFineField.SetEnabled(false);
        sizePropField.SetEnabled(false);
        addButton.SetEnabled(false);
    }
    void BuildAddFixtureView()
    {
        var view = addFixtureView;
        var fixtureSelectField = view.Q<EditableDropdownField>();
        var fixtureInfo = view.Q<Label>("fixture-info");
        var addButton = view.Q<Button>("add-button");
        DmxOutputFixture fixture = null;

        void SetChoices()
        {
            fixtureSelectField.ClearChoices();
            fixtureSelectField.AddChoices(new[] { "Select Fixture..." });
            fixtureSelectField.AddChoices(FixtureLibrary.FixtureLabelList);
        }
        SetChoices();

        fixtureSelectField.onValueCanged += val =>
        {
            if (FixtureLibrary.FixtureLabelList.Contains(val))
            {
                addButton.SetEnabled(true);
                fixture = FixtureLibrary.LoadFixture(val);
                fixtureInfo.text = $"Fixture Label: {fixture.Label}\nNum Channels: {fixture.NumChannels}";
            }
            else
            {
                addButton.SetEnabled(false);
                fixture = null;
                fixtureInfo.text = $"Fixture Label: Null\nNum Channels: 0";
            }
        };

        addButton.clicked += () =>
        {
            var label = "";
            var ch = activeChannels[0];
            if (FixtureLibrary.FixtureLabelList.Contains(label = fixtureSelectField.Value))
            {
                while (activeChannels.Contains(ch))
                {
                    var output = FixtureLibrary.LoadFixture(label);
                    output.StartChannel = ch;
                    if (Enumerable.Range(ch, output.NumChannels).All(idx => activeUniverse.IsEmpty(idx)))
                        activeUniverse.AddOutput(output);
                    ch += output.NumChannels;
                }
                activeUniverse.NotifyEditChannel();
            }
        };

        FixtureLibrary.OnFixtureLabelListLoaded.Subscribe(_ => SetChoices());
    }

    void BuildFixtureEditorView()
    {
        DmxOutputFixture editingFixture = null;
        VisualElement editUI = null;

        var view = fixtureEditorView;
        var fixtureSelectField = view.Q<EditableDropdownField>();
        var container = view.Q("container");
        var deleteButton = view.Q<Button>("delete-button");
        var saveButton = view.Q<Button>("save-button");

        void SetChoices()
        {
            fixtureSelectField.ClearChoices();
            fixtureSelectField.AddChoices(new[] { "New Fixture" });
            fixtureSelectField.AddChoices(FixtureLibrary.FixtureLabelList);
        }
        void SelectFixture(string label)
        {
            fixtureSelectField.Value = label;
            if (editUI != null)
                editUI.RemoveFromHierarchy();
            editingFixture = FixtureLibrary.LoadFixture(label);
            var fixtureUI = DmxOutputUI.CreateUI(editingFixture);
            editUI = fixtureUI.EditorUI;
            container.Add(editUI);

            deleteButton.SetEnabled(!string.IsNullOrEmpty(editingFixture.FilePath));
            saveButton.SetEnabled(0 < editingFixture.NumChannels);
            editingFixture.OnEditChannel.Subscribe(_ => saveButton.SetEnabled(0 < editingFixture.NumChannels));
        }

        fixtureSelectField.onValueCanged += SelectFixture;

        deleteButton.clicked += () =>
        {
            FixtureLibrary.DeleteFixture(editingFixture);
            SelectFixture("New Fixture");
        };
        saveButton.clicked += () =>
        {
            if (editingFixture != null)
                FixtureLibrary.SaveFixture(editingFixture);
            SelectFixture(editingFixture.Label);
        };

        SetChoices();
        SelectFixture(fixtureSelectField.Value);
        FixtureLibrary.OnFixtureLabelListLoaded.Subscribe(_ => SetChoices());
    }

    internal void DisplayOutputEditorUI()
    {
        addOutputView.style.display = DisplayStyle.None;
        addFixtureView.style.display = DisplayStyle.None;
        fixtureEditorView.style.display = DisplayStyle.None;
        editorUIContainer.style.display = DisplayStyle.Flex;
        editorUIContainer.Clear();
    }
    internal void AddOutputEditorUI(VisualElement editorUI) => editorUIContainer.Add(editorUI);

    internal void EmptyChannelsView(List<int> chList)
    {
        activeChannels = chList;
        activeChannels.Sort();
        addOutputView.style.display = DisplayStyle.Flex;
        addFixtureView.style.display = DisplayStyle.Flex;
        fixtureEditorView.style.display = DisplayStyle.Flex;
        editorUIContainer.style.display = DisplayStyle.None;
        addOutputView.SetEnabled(true);
        addFixtureView.SetEnabled(true);
    }
    internal void NoSelection()
    {
        fixtureEditorView.style.display = DisplayStyle.Flex;
        editorUIContainer.style.display = DisplayStyle.None;
        addOutputView.style.display = DisplayStyle.None;
        addFixtureView.style.display = DisplayStyle.None;
    }
}
