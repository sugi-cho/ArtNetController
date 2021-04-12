using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] UniverseView universeView;
    private void OnEnable()
    {
        universeView = new UniverseView();

        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        var universe = root.Q("UniverseView");
        var control = root.Q("ControlView");
        var editor = root.Q("EditorView");
        var editorCloseButton = root.Q("close-button");

        universeView.BuildUI(universe);

        var closed = false;
        editorCloseButton.RegisterCallback<PointerDownEvent>(evt =>
        {
            closed = !closed;
            editor.style.marginRight = closed ? -editor.localBound.width : 0;
        });
    }
}
