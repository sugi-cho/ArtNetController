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
    public DmxOutputXY outputXY;
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
        outputXY = new DmxOutputXY { Label = "XY Pad" };
        outputColor = new DmxOutputColor { Label = "Color", UseFine = true };

        var dmxOutputFloatUI = new DmxOutputFloatUI(outputFloat);
        var dmxOutputIntUI = new DmxOutputIntUI(outputInt);
        var dmxOutputSelectorUI = new DmxOutputSelectorUI(outputSelector);
        var dmxOutputBoolUI = new DmxOutputBoolUI(outputBool);
        var dmxOutputXYUI = new DmxOutputXYUI(outputXY);
        var dmxOutputColorUI = new DmxOutputColorUI(outputColor);

        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        var editorView = root.Q("EditorView");
        var controlView = root.Q("ControlView");

        editorView.Add(dmxOutputFloatUI.EditorUI);
        editorView.Add(dmxOutputIntUI.EditorUI);
        editorView.Add(dmxOutputSelectorUI.EditorUI);
        editorView.Add(dmxOutputBoolUI.EditorUI);
        editorView.Add(dmxOutputXYUI.EditorUI);
        editorView.Add(dmxOutputColorUI.EditorUI);

        controlView.style.flexDirection = FlexDirection.Row;
        controlView.Add(dmxOutputFloatUI.ControlUI);
        controlView.Add(dmxOutputIntUI.ControlUI);
        controlView.Add(dmxOutputSelectorUI.ControlUI);
        controlView.Add(dmxOutputBoolUI.ControlUI);
        controlView.Add(dmxOutputXYUI.ControlUI);
        controlView.Add(dmxOutputColorUI.ControlUI);
    }
}
