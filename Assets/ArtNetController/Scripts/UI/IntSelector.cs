using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class IntSelector : VisualElement
{
    public new class UxmlFactory : UxmlFactory<IntSelector, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlIntAttributeDescription m_numChoices = new UxmlIntAttributeDescription { name = "numChoices", defaultValue = 10 };
        UxmlIntAttributeDescription m_value = new UxmlIntAttributeDescription { name = "value", defaultValue = 0 };
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var selector = (IntSelector)ve;

            var numChoices = m_numChoices.GetValueFromBag(bag, cc);
            var value = m_value.GetValueFromBag(bag, cc);
            selector.NumChoices = numChoices;
            selector.Value = value;
        }
    }

    public int NumChoices
    {
        get => m_numChoices;
        set
        {
            m_numChoices = value;
            Clear();
            for (var i = 0; i < m_numChoices; i++)
            {
                var index = i;
                var button = new Button() { text = $"{index}" };
                button.clicked += () => Value = index;
                button.style.flexGrow = 1;
                Add(button);
            }
        }
    }
    int m_numChoices;
    public int Value
    {
        get => m_value;
        set
        {
            value = Mathf.Clamp(value, 0, NumChoices - 1);
            m_value = value;
            this.Query<Button>().ForEach(b =>
            {
                var idx = b.parent.IndexOf(b);
                b.SetEnabled(idx != m_value);
            });
            onValueChanged?.Invoke(m_value);
        }
    }
    int m_value;

    public System.Action<int> onValueChanged;

    public IntSelector() : base()
    {
        AddToClassList("int-selector-container");
    }
}
