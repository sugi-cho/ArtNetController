using UnityEngine;
using UnityEngine.UIElements;

public class test : MonoBehaviour
{
    public DmxOutputFixture fixture;
    public DmxOutputFixture newFixture;

    public DmxOutputFloat outputFloat;
    public DmxOutputInt outputInt;
    public DmxOutputSelector outputSelector;
    public DmxOutputBool outputBool;
    public DmxOutputColor outputColor;

    [ContextMenu("test")]
    void Test()
    {
        fixture = FixtureLibrary.CreateFixture();
        FixtureLibrary.SaveFixture(fixture);
        newFixture = FixtureLibrary.CreateFixture();
        FixtureLibrary.SaveFixture(newFixture);
    }

    private void OnEnable()
    {
        outputFloat = new DmxOutputFloat { Label = "Dimmer", UseFine = true, };
        outputInt = new DmxOutputInt { Label = "IntTest" };
        outputSelector = new DmxOutputSelector { Label = "Selector"};
        outputBool = new DmxOutputBool { Label = "Switch" };
        outputColor = new DmxOutputColor { Label = "Color", UseFine = true };
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        root.style.flexDirection = FlexDirection.Row;
        root.Add(new DmxOutputFloatUI(outputFloat).ControlUI);
        root.Add(new DmxOutputIntUI(outputInt).ControlUI);
        root.Add(new DmxOutputSelectorUI(outputSelector).ControlUI);
        root.Add(new DmxOutputBoolUI(outputBool).ControlUI);
        root.Add(new DmxOutputColorUI(outputColor).ControlUI);
    }
}
