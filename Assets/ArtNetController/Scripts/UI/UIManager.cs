using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;

        var universeView = root.Q("UniverseView");
        var controlView = root.Q("ControlView");
        var editorView = root.Q("EditorView");
        var closeButton = root.Q("close-button");

        var closed = false;
        closeButton.RegisterCallback<PointerDownEvent>(evt =>
        {
            closed = !closed;
            editorView.style.marginRight = closed ? -editorView.localBound.width : 0;
        });
    }
}
