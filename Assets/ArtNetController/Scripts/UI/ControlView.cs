using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class ControlView
{
    VisualElement uiContainer;
    public void BuildUI(VisualElement view) => uiContainer = view.Q("ControlContainer");

    public void ClearUI() => uiContainer.Clear();

    public void AddUI(VisualElement controlUI) => uiContainer.Add(controlUI);
}
