﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIInteractionSwipeToRightPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public float detectionConfiance;

        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("InteractionSwipeLeftToRight-detectionIntensity", detectionConfiance);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            var prevDetectionConfiance = PlayerPrefs.GetFloat("InteractionSwipeLeftToRight-detectionIntensity", default);
            detectionConfiance = prevDetectionConfiance;
            (uiElements[0] as TMP_InputField).text = detectionConfiance.ToString();
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var detectionIntensityInputField = panel.GetChild(0).GetComponent<TMP_InputField>();
            detectionIntensityInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        detectionConfiance = asFloat;
                        DanbiUISync.onPanelUpdated?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues(detectionIntensityInputField);
        }
    };
};