using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] UniverseView universeView;
    [SerializeField] ControlView controlView;
    [SerializeField] EditorView editorView;
    private void OnEnable()
    {
        universeView = new UniverseView();
        controlView = new ControlView();
        editorView = new EditorView();

        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        var universe = root.Q("UniverseView");
        var control = root.Q("ControlView");
        var editor = root.Q("EditorView");
        var editorCloseButton = root.Q("close-button");

        universeView.BuildUI(universe);
        controlView.BuildUI(control);
        editorView.BuildUI(editor);

        var outputUIList = new List<IDmxOutput>();
        universeView.onSelectionChanged += (chList, outputList) =>
         {
             if(0 < outputList.Count)
             {
                 var groups = outputList.GroupBy(o => (o.Type, o.Label));
                 foreach(var g in groups)
                 {
                     var uiList = g.Select(o => DmxOutputUI.CreateUI(o)).ToList();
                     uiList[0].AddMultiTargeUIs(uiList.Where(ui => ui != uiList[0]));
                     editorView.Add(uiList[0]);
                     uiList.ForEach(ui => controlView.Add(ui));
                 }
             }
         };

        var closed = false;
        editorCloseButton.RegisterCallback<PointerDownEvent>(evt =>
        {
            closed = !closed;
            editor.style.marginRight = closed ? -editor.localBound.width : 0;
        });
    }
}
