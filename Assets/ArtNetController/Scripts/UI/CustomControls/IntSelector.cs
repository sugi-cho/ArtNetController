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
            VisualElement SelectElement()
            {
                var vel = new VisualElement { name = "select-element" };
                vel.style.flexGrow = 1;
                return vel;
            }
            m_numChoices = value;
            Clear();
            for (var i = 0; i < m_numChoices; i++)
            {
                var index = i;
                var select = SelectElement();
                select.RegisterCallback<PointerUpEvent>(evt =>
                {
                    Value = index;
                    if (evt.shiftKey)
                        onShiftKey?.Invoke(Value);
                });
                Add(select);
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
            this.Query("select-element").ForEach(e =>
            {
                var idx = e.parent.IndexOf(e);
                if (idx == m_value)
                    e.AddToClassList("selected");
                else
                    e.RemoveFromClassList("selected");
            });
            onValueChanged?.Invoke(m_value);
        }
    }
    int m_value;

    public event System.Action<int> onValueChanged;
    public event System.Action<int> onShiftKey;

    public IntSelector() : base()
    {
        AddToClassList("int-selector-container");
    }
}
