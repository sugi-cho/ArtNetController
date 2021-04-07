using UnityEngine;
using UnityEngine.UIElements;

public class DmxOutputBoolUI : DmxOutputUIBase<DmxOutputBool>
{
    public DmxOutputBoolUI(DmxOutputBool dmxOutput) : base(dmxOutput) { }
    protected override void BuildEditorUI()
    {
        var tree = Resources.Load<VisualTreeAsset>(EditorUIResourcePath);
        editorUI = tree?.CloneTree("");
    }

    protected override void BuildControlUI()
    {
        base.BuildControlUI();

        var label = controlUI.Q<Label>();
        var area = controlUI.Q("input-area");
        var toggle = controlUI.Q("toggle-switch");
        var trigger = controlUI.Q("trigger");
        var textField = controlUI.Q<TextField>();

        void SetValue(bool val)
        {
            if (val)
            {
                area.RemoveFromClassList("switch-off");
                area.AddToClassList("switch-on");
            }
            else
            {
                area.RemoveFromClassList("switch-on");
                area.AddToClassList("switch-off");
            }
            targetDmxOutput.Value = val;
            textField.value = (val ? 255 : 0).ToString();
            onValueChanged?.Invoke(val);
        }

        label.text = targetDmxOutput.Label;
        toggle.RegisterCallback<PointerUpEvent>(evt => SetValue(!targetDmxOutput.Value));
        trigger.RegisterCallback<PointerDownEvent>(evt =>
        {
            trigger.CapturePointer(evt.pointerId);
            SetValue(true);
        });
        trigger.RegisterCallback<PointerUpEvent>(evt =>
        {
            trigger.ReleasePointer(evt.pointerId);
            SetValue(false);
        });
        textField.isDelayed = true;
        textField.SetEnabled(false);
        SetValue(false);
    }

    public event System.Action<bool> onValueChanged;
}
