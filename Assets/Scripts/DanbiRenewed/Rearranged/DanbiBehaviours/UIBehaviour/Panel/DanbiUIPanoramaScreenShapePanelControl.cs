﻿using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIPanoramaScreenShapePanelControl : DanbiUIPanelControl
    {
        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var panoramaTypeDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            panoramaTypeDropdown?.AddOptions(new List<string> { "Cube", "Cylinder" });
            panoramaTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    DanbiUIPanoramaScreenDimensionPanelControl.Call_OnTypeChanged?.Invoke(option);
                }
            );
        }
    };
};