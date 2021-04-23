using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class EditableDropdownField : VisualElement
{

    public new class UxmlFactory : UxmlFactory<EditableDropdownField, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlStringAttributeDescription m_Choices = new UxmlStringAttributeDescription() { name = "choices", defaultValue = "element1,element2,element3" };
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var field = (EditableDropdownField)ve;
            var choices = m_Choices.GetValueFromBag(bag, cc).Split(',').Select(e => e.Trim());
            if (0 < choices.Count())
            {
                field.ClearChoices();
                field.AddChoices(choices);
            }
        }
    }
    public event System.Action<string> onValueCanged;
    public System.Func<string, string> formatSelectedValueCallback;
    public System.Func<string, string> formatListItemCallback;
    List<string> m_choices;
    public void ClearChoices() => m_choices.Clear();
    public void AddChoices(IEnumerable<string> choices)
    {
        m_choices.AddRange(choices);
        m_dropdownField.value = m_choices[m_dropdownField.index];
    }
    DropdownField m_dropdownField;
    public string Value => m_dropdownField.value;

    public EditableDropdownField()
    {
        m_choices = new List<string> { "dummy" };

        string FormatSelectedValue(string val) => formatSelectedValueCallback != null ? formatSelectedValueCallback(val) : val;
        string FormatListItem(string val) => formatListItemCallback != null ? formatListItemCallback(val) : val;
        m_dropdownField = new DropdownField(m_choices, 0, FormatSelectedValue, FormatListItem);
        m_dropdownField.RegisterValueChangedCallback(evt => onValueCanged?.Invoke(evt.newValue));

        Add(m_dropdownField);
    }
}
