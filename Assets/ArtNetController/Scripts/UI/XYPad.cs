using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class XYPad : VisualElement
{
    public new class UxmlFactory : UxmlFactory<XYPad, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlFloatAttributeDescription m_paramX = new UxmlFloatAttributeDescription() { name = "paramX", defaultValue = 0.5f };
        UxmlFloatAttributeDescription m_paramY = new UxmlFloatAttributeDescription() { name = "paramY", defaultValue = 0.5f };
        UxmlColorAttributeDescription m_lineColor = new UxmlColorAttributeDescription() { name = "lineColor", defaultValue = Color.black };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var xyPad = (XYPad)ve;
            Vector2 v2;
            v2.x = m_paramX.GetValueFromBag(bag, cc);
            v2.y = m_paramY.GetValueFromBag(bag, cc);
            xyPad.Value = v2;
            xyPad.LineColor = m_lineColor.GetValueFromBag(bag, cc);
        }
    }

    public XYPad() : base()
    {
        var upSide = new VisualElement();
        var bottomSide = new VisualElement();
        var xParam0 = new VisualElement();
        var xParam1 = new VisualElement();

        Add(upSide);
        Add(bottomSide);
        upSide.Add(xParam0);
        bottomSide.Add(xParam1);

        style.backgroundColor = Color.white;

        upSide.style.flexDirection = bottomSide.style.flexDirection = FlexDirection.Row;
        upSide.style.flexGrow = upSide.style.flexShrink = 1;

        bottomSide.AddToClassList("paramY");
        bottomSide.style.flexGrow = bottomSide.style.flexShrink = 0;
        bottomSide.style.borderTopColor = Color.black;
        bottomSide.style.borderTopWidth = 1;
        bottomSide.style.height = Length.Percent(50);

        xParam0.AddToClassList("paramX");
        xParam1.AddToClassList("paramX");
        this.Query(null, "paramX").ForEach(e =>
        {
            e.style.flexGrow = e.style.flexShrink = 0;
            e.style.width = Length.Percent(50);
            e.style.borderRightColor = Color.black;
            e.style.borderRightWidth = 1;
        });

        RegisterCallback<PointerDownEvent>(SetPointerValue);
        RegisterCallback<PointerMoveEvent>(SetPointerValue);
    }
    void SetPointerValue(IPointerEvent p)
    {
        if ((p.pressedButtons & 1) == 0)
            return;
        var pos = p.localPosition;
        var (w, h) = (localBound.width, localBound.height);
        var v2 = Value;
        (v2.x, v2.y) = (pos.x / w, 1f - pos.y / h);
        Value = v2;
    }
    public Vector2 Value
    {
        get => m_value;
        set
        {
            value.x = Mathf.Clamp01(value.x);
            value.y = Mathf.Clamp01(value.y);
            m_value = value;
            this.Query(null, "paramX").ForEach(e => e.style.width = Length.Percent(value.x * 100));
            this.Q(null, "paramY").style.height = Length.Percent(value.y * 100);
            if (onChange != null)
                onChange.Invoke(m_value);
        }
    }
    Vector2 m_value;
    public System.Action<Vector2> onChange;
    public Color LineColor
    {
        set
        {
            this.Q(null, "paramY").style.borderTopColor = value;
            this.Query(null, "paramX").ForEach(e => e.style.borderRightColor = value);
        }
    }
}