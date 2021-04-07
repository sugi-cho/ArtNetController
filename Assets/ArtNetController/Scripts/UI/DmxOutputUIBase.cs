using UnityEngine.UIElements;
using UnityEngine;

public abstract class DmxOutputUIBase<T> where T : IDmxOutputModule
{
    public DmxOutputUIBase(T dmxOutput)
    {
        targetDmxOutput = dmxOutput;
        BuildEditorUI();
        BuildControlUI();
    }
    internal T targetDmxOutput;
    string EditorUIBaseResourcePath => $"UI/DmxOutput/DmxOutput_Editor";
    internal string EditorUIResourcePath => $"UI/DmxOutput/{typeof(T).Name}_Editor";
    internal string ControlUIResourcePath => $"UI/DmxOutput/{typeof(T).Name}_Control";
    protected virtual void BuildEditorUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(EditorUIBaseResourcePath);
        editorUI = tree?.CloneTree("");
        if (controlUI == null)
            Debug.LogWarning($"Invalid path: {EditorUIBaseResourcePath}");
    }
    protected virtual void BuildControlUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(ControlUIResourcePath);
        controlUI = tree?.CloneTree("");
        if (controlUI == null)
            Debug.LogWarning($"Invalid path: {ControlUIResourcePath}");
    }
    protected VisualElement editorUI;
    protected VisualElement controlUI;
    public VisualElement EditorUI => editorUI;
    public VisualElement ControlUI => controlUI;
}