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
            field.AddChoices(m_Choices.GetValueFromBag(bag, cc).Split(',').Select(e => e.Trim()));
        }
    }
    List<string> m_choices;
    public void ClearChoices() => m_choices.Clear();
    public void AddChoices(IEnumerable<string> choices) => m_choices.AddRange(choices);
    DropdownField m_dropdownField;

    public EditableDropdownField()
    {
        m_choices = new List<string> { "first",};
        m_dropdownField = new DropdownField(m_choices, 0);
        Add(m_dropdownField);
    }
}
