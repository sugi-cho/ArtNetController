using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class test : MonoBehaviour
{
    public List<string> list;
    public DmxOutputFixture fixture;

    [SerializeReference]
    public List<IDmxOutput> dmxOutputList;

    [ContextMenu("test")]
    void Test()
    {
        dmxOutputList.Add(new DmxOutputFloat { Label = "float" });
        dmxOutputList.Add(new DmxOutputFloat { Label = "float" });
        dmxOutputList.Add(new DmxOutputFloat { Label = "float" });
        dmxOutputList.Add(new DmxOutputInt { Label = "int" });
        dmxOutputList.Add(new DmxOutputInt { Label = "int" });
        dmxOutputList.Add(new DmxOutputInt { Label = "float" });

        var gs = dmxOutputList.GroupBy(o => (o.Type, o.Label));
        Debug.Log(gs.Count());
    }

    private void OnEnable()
    {
        dmxOutputList = new List<IDmxOutput>();
        dmxOutputList.Add(fixture);

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
