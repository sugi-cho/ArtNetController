using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class EditorView
{
    DmxOutputUniverse activeUniverse => UniverseManager.Instance.ActiveUniverse;

    ScrollView baseScroll;
    VisualElement addOutputView;
    VisualElement addFixtureView;
    VisualElement newFixtureView;
    VisualElement editorUIContainer;

    List<int> activeChannels;

    public void BuildUI(VisualElement view)
    {
        baseScroll = view.Q<ScrollView>("base-scroll");

        addOutputView = view.Q("AddOutputView");
        addFixtureView = view.Q("AddFixtureView");
        BuildAddOutputView();
        BuildAddFixtureView();

        newFixtureView = DmxOutputUI.CreateUI(FixtureLibrary.CreateFixture()).EditorUI;
        baseScroll.Add(newFixtureView);
        editorUIContainer = view.Q("EditorUIContainer");
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
                addButton.SetEnabled(true);
            }
            else
            {
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
                    activeUniverse.AddModule(output);
                ch += output.NumChannels;
            }
            activeUniverse.NotifyEditOutputList();
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
        var addButton = view.Q<Button>("add-button");

        void SetChoices()
        {
            fixtureSelectField.ClearChoices();
            fixtureSelectField.AddChoices(new[] { "Select Fixture..." });
            fixtureSelectField.AddChoices(FixtureLibrary.FixtureLabelList);
        }
        SetChoices();

        fixtureSelectField.onValueCanged += val => addButton.SetEnabled(FixtureLibrary.FixtureLabelList.Contains(val));

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
                        activeUniverse.AddModule(output);
                    ch += output.NumChannels;
                }
                activeUniverse.NotifyEditOutputList();
            }
        };

        FixtureLibrary.OnFixtureLabelListLoaded += SetChoices;
    }


    internal void DisplayOutputEditorUI()
    {
        addOutputView.style.display = DisplayStyle.None;
        addFixtureView.style.display = DisplayStyle.None;
        newFixtureView.style.display = DisplayStyle.None;
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
        newFixtureView.style.display = DisplayStyle.Flex;
        editorUIContainer.style.display = DisplayStyle.None;
        addOutputView.SetEnabled(true);
        addFixtureView.SetEnabled(true);
    }
    internal void NoSelection()
    {
        newFixtureView.style.display = DisplayStyle.Flex;
        editorUIContainer.style.display = DisplayStyle.None;
        addOutputView.SetEnabled(false);
        addFixtureView.SetEnabled(false);
    }
}
