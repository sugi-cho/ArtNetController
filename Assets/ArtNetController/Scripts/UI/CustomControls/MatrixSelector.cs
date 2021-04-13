using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MatrixSelector : VisualElement
{
    public new class UxmlFactory : UxmlFactory<MatrixSelector, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlIntAttributeDescription m_row = new UxmlIntAttributeDescription { name = "row", defaultValue = 4 };
        UxmlIntAttributeDescription m_column = new UxmlIntAttributeDescription { name = "column", defaultValue = 4 };
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var matrixSelector = (MatrixSelector)ve;

            var row = m_row.GetValueFromBag(bag, cc);
            var column = m_column.GetValueFromBag(bag, cc);

            matrixSelector.SetMatrix(row, column);
        }
    }

    public void SetMatrix(int row, int column, System.Func<int, VisualElement> onBindFunc = null, System.Action<VisualElement, int> onBindAction = null)
    {
        Clear();
        m_selectElementValues = new (VisualElement, bool)[row * column];

        for (var i = 0; i < column; i++)
        {
            var rowContainer = new VisualElement();
            rowContainer.AddToClassList($"row_{i}");
            rowContainer.name = $"row_{i}";
            rowContainer.style.flexDirection = FlexDirection.Row;
            rowContainer.style.width = Length.Percent(100);
            rowContainer.style.height = Length.Percent(100f / column);
            rowContainer.style.marginTop = rowContainer.style.marginBottom = 2;
            Add(rowContainer);
            for (var j = 0; j < row; j++)
            {
                var idx = i * row + j;
                VisualElement vle;
                if (onBindFunc != null)
                    vle = onBindFunc.Invoke(idx);
                else
                {
                    vle = new Label { text = $"{idx}" };
                    vle.name = "select-element";
                    m_selectElementValues[idx] = (vle, false);
                    vle.RegisterCallback<PointerUpEvent>(evt =>
                    {
                        var vleValue = m_selectElementValues[idx];
                        var shift = evt.shiftKey;
                        vleValue.value = !vleValue.value;
                        if (shift && m_selectElementValues[m_lastActiveIdx].value == vleValue.value)
                            SetValueFromTo(m_lastActiveIdx, idx, vleValue.value);
                        else
                            SetValue(idx, vleValue.value, notify: true, singleSelect: true);
                        m_lastActiveIdx = idx;
                    });
                }
                vle.AddToClassList($"element_{idx}");
                vle.style.width = Length.Percent(100f / row);
                vle.style.height = Length.Percent(100);
                vle.style.flexShrink = 1;
                rowContainer.Add(vle);
                onBindAction?.Invoke(vle, idx);
            }
        }
    }
    public void SetValueFromTo(int from, int to, bool value)
    {
        var minMax = (Mathf.Min(from, to), Mathf.Max(from, to));
        for (var i = minMax.Item1; i <= minMax.Item2; i++)
            SetValue(i, value, notify: true, singleSelect: false);
        onSelectComplete?.Invoke();
    }
    public void SetValueFromToWithoutNotify(int from, int to, bool value)
    {
        var minMax = (Mathf.Min(from, to), Mathf.Max(from, to));
        for (var i = minMax.Item1; i <= minMax.Item2; i++)
            SetValue(i, value, notify: false, singleSelect: false);
    }
    public void SetAllValues(bool value)
    {
        for (var i = 0; i < m_selectElementValues.Length; i++)
            SetValue(i, value, false, false);
    }
    public void SetValue(int idx, bool value, bool notify, bool singleSelect)
    {
        var vle = m_selectElementValues[idx].vle;
        m_selectElementValues[idx].value = value;
        if (value)
            vle.AddToClassList("checked");
        else
            vle.RemoveFromClassList("checked");
        if (notify)
            onValueChanged?.Invoke(idx, value);
        if (singleSelect)
            onSelectComplete?.Invoke();
    }
    int m_lastActiveIdx;
    (VisualElement vle, bool value)[] m_selectElementValues;

    public event System.Action<int, bool> onValueChanged;
    public event System.Action onSelectComplete;

    public MatrixSelector() : base()
    {

    }
}
