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

    public Vector2 Value
    {
        get => m_value;
        set
        {
            value.x = Mathf.Clamp01(value.x);
            value.y = Mathf.Clamp01(value.y);
            lineX.style.left = (localBound.width - 1) * value.x;
            lineY.style.bottom = (localBound.height - 1) * value.y;
            m_value = value;
            onChange?.Invoke(m_value);
        }
    }
    public void SetValueWithoutNotify(Vector2 v2) => m_value = v2;
    Vector2 m_value;
    public event System.Action<Vector2> onChange;
    public Color LineColor
    {
        set => lineX.style.backgroundColor = lineY.style.backgroundColor = value;
    }

    VisualElement lineX;
    VisualElement lineY;

    public XYPad() : base()
    {
        lineX = new VisualElement();
        lineY = new VisualElement();
        lineX.style.width = lineY.style.height = 1;
        lineX.style.height = lineY.style.width = Length.Percent(100f);
        lineX.style.position = lineY.style.position = Position.Absolute;
        lineX.style.backgroundColor = lineY.style.backgroundColor = Color.black;
        Add(lineX);
        Add(lineY);

        RegisterCallback<PointerDownEvent>(evt =>
        {
            this.CapturePointer(evt.pointerId);
            SetPointerValue(evt);
        });
        RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (this.HasPointerCapture(evt.pointerId))
                SetPointerValue(evt);
        });
        RegisterCallback<PointerUpEvent>(evt => this.ReleasePointer(evt.pointerId));
    }
    void SetPointerValue(IPointerEvent evt)
    {
        var pos = evt.localPosition;
        var (w, h) = (localBound.width, localBound.height);
        var v2 = Value;
        (v2.x, v2.y) = (pos.x / w, 1f - pos.y / h);
        Value = v2;
    }
}