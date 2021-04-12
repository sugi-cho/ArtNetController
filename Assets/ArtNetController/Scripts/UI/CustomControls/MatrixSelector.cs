using System.Collections;
using System.Collections.Generic;
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
                VisualElement ve;
                if (onBindFunc != null)
                    ve = onBindFunc.Invoke(idx);
                else
                    ve = new Toggle { text = $"{idx}" };
                ve.AddToClassList($"element_{idx}");
                ve.name = $"element_{idx}";
                ve.style.width = Length.Percent(100f / row);
                ve.style.height = Length.Percent(100);
                ve.style.flexShrink = 1;
                rowContainer.Add(ve);
                onBindAction?.Invoke(ve, idx);
            }
        }
    }
    public MatrixSelector() : base() {

    }
}
