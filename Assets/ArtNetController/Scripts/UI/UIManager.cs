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
        editorView.NoSelection();
        universeView.onSelectionChanged += (chList, outputList) =>
        {
            controlView.ClearUI();
            if (0 < outputList.Count)
            {
                editorView.DisplayOutputEditorUI();
                var groups = outputList.GroupBy(o => (o.Type, o.Label));
                foreach (var g in groups)
                {
                    var uiList = g.Select(o => DmxOutputUI.CreateUI(o)).ToList();
                    uiList.ForEach(ui =>
                    {
                        ui.AddMultiTargeUIs(uiList.Where(element => element != ui));
                        ui.SetParent(UniverseManager.Instance.ActiveUniverse);
                        controlView.AddUI(ui.ControlUI);
                    });
                }
            }
            else if (0 < chList.Count)
            {
                editorView.EmptyChannelsView(chList);
            }
            else
            {
                editorView.NoSelection();
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
