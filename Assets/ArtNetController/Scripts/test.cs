using UnityEngine;
using UnityEngine.UIElements;

using sugi.cc.udp.artnet;

public class test : MonoBehaviour
{
    public DmxOutputFixture fixture;
    public DmxOutputFixture newFixture;
    public DmxOutputFloat outputFloat;
    public DmxOutputInt outputInt;

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
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        root.style.flexDirection = FlexDirection.Row;
        root.Add(new DmxOutputFloatUI(outputFloat).ControlUI);
        root.Add(new DmxOutputIntUI(outputInt).ControlUI);
    }
}
