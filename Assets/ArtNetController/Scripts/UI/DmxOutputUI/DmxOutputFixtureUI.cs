using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputFixtureUI : DmxOutputUI<DmxOutputFixture>
{
    public DmxOutputFixtureUI(DmxOutputFixture dmxOutput) : base(dmxOutput) { }

    public DmxOutputUniverse TargetUniverse { get; set; }
    List<DmxOutputUI> DmxOutputUIList
    {
        get
        {
            if (m_dmxOutputUIList == null | m_dmxOutputUIList.Count != targetDmxOutput.DmxOutputList.Count)
                BuildDmxOutputUIList();
            return m_dmxOutputUIList;
        }
    }
    List<DmxOutputUI> m_dmxOutputUIList = null;
    void BuildDmxOutputUIList() => m_dmxOutputUIList = targetDmxOutput.DmxOutputList.Select(dmxOutput => CreateUI(dmxOutput)).ToList();

    public void RebuildUI()
    {
        var editorParent = editorUI.parent;
        editorUI.RemoveFromHierarchy();
        editorUI.Clear();
        BuildEditorUI();
        editorParent.Add(editorUI);

        var controlParent = controlUI.parent;
        controlUI.RemoveFromHierarchy();
        controlUI.Clear();
        BuildControlUI();
        controlParent.Add(controlUI);
    }
    protected override void BuildEditorUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(EditorUIResourcePath);
        editorUI = tree.CloneTree("");

        foreach (var dmxOutputUI in DmxOutputUIList)
        {

        }
    }
    protected override void BuildControlUI()
    {
        base.BuildControlUI();
    }
}
