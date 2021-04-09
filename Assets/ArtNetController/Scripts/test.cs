using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class test : MonoBehaviour
{
    public DmxOutputFixture fixture;
    public DmxOutputFixture newFixture;

    [SerializeReference]
    public List<IDmxOutput> dmxOutputList;

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
        dmxOutputList = new List<IDmxOutput>();

        dmxOutputList.Add(new DmxOutputEmpty { Label = "Empty", SizeProp = 3 });
        dmxOutputList.Add(new DmxOutputFloat { Label = "Dimmer", UseFine = true });
        dmxOutputList.Add(new DmxOutputInt { Label = "IntTest" });
        dmxOutputList.Add(new DmxOutputSelector { Label = "Selector", SizeProp = 5 });
        dmxOutputList.Add(new DmxOutputBool { Label = "Switch" });
        dmxOutputList.Add(new DmxOutputXY { Label = "XY Pad" });
        dmxOutputList.Add(new DmxOutputColor { Label = "Color", UseFine = true });

        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        var editorView = root.Q("EditorView");
        var controlView = root.Q("ControlView");

        foreach (var output in dmxOutputList)
        {
            var dmxOutputUI = DmxOutputUI.CreateUI(output);
            editorView.Add(dmxOutputUI.EditorUI);
            controlView.Add(dmxOutputUI.ControlUI);
        }
    }
}
