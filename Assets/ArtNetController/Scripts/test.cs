using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class test : MonoBehaviour
{
    public DmxOutputFixture fixture;

    [SerializeReference]
    public List<IDmxOutput> dmxOutputList;

    [ContextMenu("test")]
    void Test()
    {

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
