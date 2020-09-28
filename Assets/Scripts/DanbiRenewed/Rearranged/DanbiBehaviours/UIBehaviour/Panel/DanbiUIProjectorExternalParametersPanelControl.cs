﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Danbi
{
    public class DanbiUIProjectorExternalParametersPanelControl : DanbiUIPanelControl
    {
        [HideInInspector]
        public DanbiCameraExternalData externalData;
        public bool useExternalParameters = false;
        public string loadPath;
        public string savePath;
        Dictionary<string, Selectable> PanelElementsDic = new Dictionary<string, Selectable>();
        string playerPrefsKeyRoot = "ProjectorExternalParameters-";
        void OnDisable()
        {
            PlayerPrefs.SetInt(playerPrefsKeyRoot + "useExternalParameters", useExternalParameters == true ? 1 : 0);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-X", externalData.radialCoefficientX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-Y", externalData.radialCoefficientY);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-Z", externalData.radialCoefficientZ);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-X", externalData.tangentialCoefficientX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Y", externalData.tangentialCoefficientY);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Z", externalData.tangentialCoefficientZ);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "principalPoint-X", externalData.principalPointX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "principalPoint-Y", externalData.principalPointY);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "focalLength-X", externalData.focalLengthX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "focalLength-Y", externalData.focalLengthY);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "skewCoefficient", externalData.skewCoefficient);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // Create a bew external data instance to save.            
            externalData = new DanbiCameraExternalData();
            var panel = Panel.transform;

            // bind the "External parameters" toggle.
            var useExternalParametersToggle = panel.GetChild(0).GetComponent<Toggle>();
            bool prevUseExternalParamters = PlayerPrefs.GetInt(playerPrefsKeyRoot + "useExternalParameters", default) == 1;
            useExternalParameters = prevUseExternalParamters;
            useExternalParametersToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    useExternalParameters = isOn;
                    foreach (var i in PanelElementsDic)
                    {
                        i.Value.interactable = isOn;
                    }
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );
            // useExternalParametersToggle

            // bind the select camera external parameter button.
            var selectCameraExternalParameterButton = panel.GetChild(1).GetComponent<Button>();
            selectCameraExternalParameterButton.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_LoadCameraExternalParametersSelector(panel));
                }
            );
            PanelElementsDic.Add("selectCameraExternalParam", selectCameraExternalParameterButton);

            // bind the "External parameters" buttons.
            // bind the radial Coefficient X
            var radialCoefficient = panel.GetChild(3);

            var radialCoefficientXInputField = radialCoefficient.GetChild(0).GetComponent<InputField>();
            float prevRadialCoefficientX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-X", default);
            radialCoefficientXInputField.text = prevRadialCoefficientX.ToString();
            externalData.radialCoefficientX = prevRadialCoefficientX;
            radialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficientX = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("radialCoefficientX", radialCoefficientXInputField);

            // bind the radial Coefficient Y
            var radialCoefficientYInputField = radialCoefficient.GetChild(1).GetComponent<InputField>();
            float prevRadialCoefficientY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-Y", default);
            radialCoefficientYInputField.text = prevRadialCoefficientY.ToString();
            externalData.radialCoefficientY = prevRadialCoefficientY;
            radialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficientY = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("radialCoefficientY", radialCoefficientYInputField);

            // bind the radial Coefficient Z
            var radialCoefficientZInputField = radialCoefficient.GetChild(2).GetComponent<InputField>();
            float prevRadialCoefficientZ = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-Z", default);
            radialCoefficientZInputField.text = prevRadialCoefficientZ.ToString();
            externalData.radialCoefficientZ = prevRadialCoefficientZ;
            radialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficientZ = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("radialCoefficientZ", radialCoefficientZInputField);

            // bind the tangential Coefficient X
            var tangentialCoefficient = panel.GetChild(4);

            var tangentialCoefficientXInputField = tangentialCoefficient.GetChild(0).GetComponent<InputField>();
            float prevTangentialCoefficientX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentialCoefficient-X", default);
            tangentialCoefficientXInputField.text = prevTangentialCoefficientX.ToString();
            externalData.tangentialCoefficientX = prevTangentialCoefficientX;
            tangentialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficientX = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("tangentialCoefficientX", tangentialCoefficientXInputField);

            // bind the tangential Coefficient Y
            var tangentialCoefficientYInputField = tangentialCoefficient.GetChild(1).GetComponent<InputField>();
            float prevTangentialCoefficientY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Y", default);
            tangentialCoefficientYInputField.text = prevTangentialCoefficientY.ToString();
            externalData.tangentialCoefficientY = prevTangentialCoefficientY;
            tangentialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficientY = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("tangentialCoefficientY", tangentialCoefficientYInputField);

            // bind the tangential Coefficient Z
            var tangentialCoefficientZInputField = tangentialCoefficient.GetChild(2).GetComponent<InputField>();
            float prevTangentialCoefficientZ = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentailCoefficient-Z", default);
            tangentialCoefficientZInputField.text = prevTangentialCoefficientZ.ToString();
            externalData.tangentialCoefficientZ = prevTangentialCoefficientZ;
            tangentialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficientZ = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("tangentialCoefficientZ", tangentialCoefficientZInputField);

            // bind the principal point X
            var principalPoint = panel.GetChild(5);

            var principalPointXInputField = principalPoint.GetChild(0).GetComponent<InputField>();
            float prevPrincipalPointX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "principalPoint-X", default);
            principalPointXInputField.text = prevPrincipalPointX.ToString();
            externalData.principalPointX = prevPrincipalPointX;
            principalPointXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.principalPointX = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("principalPointX", principalPointXInputField);

            // bind the principal point Y
            var principalPointYInputField = principalPoint.GetChild(1).GetComponent<InputField>();
            float prevPrincipalPointY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "principalPoint-Y", default);
            principalPointYInputField.text = prevPrincipalPointY.ToString();
            externalData.principalPointY = prevPrincipalPointY;
            principalPointYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.principalPointY = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("principalPointY", principalPointYInputField);

            // bind the focal length X
            var focalLength = panel.GetChild(6);

            var focalLengthXInputField = focalLength.GetChild(0).GetComponent<InputField>();
            float prevFocalLengthX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "focalLength-X", default);
            focalLengthXInputField.text = prevFocalLengthX.ToString();
            externalData.focalLengthX = prevFocalLengthX;
            focalLengthXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.focalLengthX = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("focalLengthX", focalLengthXInputField);

            // bind the principal point Y
            var focalLengthYInputField = focalLength.GetChild(1).GetComponent<InputField>();
            float prevFocalLengthY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "focalLength-Y", default);
            focalLengthYInputField.text = prevFocalLengthY.ToString();
            externalData.focalLengthY = prevFocalLengthY;
            focalLengthYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.focalLengthY = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("focalLengthY", focalLengthYInputField);

            // bind the skew coefficient
            var skewCoefficientInputField = panel.GetChild(7).GetComponent<InputField>();
            float prevSkewCoefficient = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "skewCoefficient", default);
            skewCoefficientInputField.text = prevSkewCoefficient.ToString();
            externalData.skewCoefficient = prevSkewCoefficient;
            skewCoefficientInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.skewCoefficient = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("skewCoefficient", skewCoefficientInputField);

            // bind the create camera external parameter button.
            var saveCameraExternalParameterButton = panel.GetChild(8).GetComponent<Button>();
            saveCameraExternalParameterButton.onClick.AddListener(
                () => { StartCoroutine(Coroutine_SaveNewCameraExternalParameter()); }
            );
            PanelElementsDic.Add("saveCameraExternalParam", saveCameraExternalParameterButton);

            useExternalParametersToggle.isOn = prevUseExternalParamters;
        }

        IEnumerator Coroutine_SaveNewCameraExternalParameter()
        {
            var filters = new string[] { ".dat", ".DAT" };
            string startingPath = Application.dataPath + "/Resources/";
            yield return DanbiFileSys.OpenSaveDialog(startingPath,
                                                         filters,
                                                         "Save Camera External Parameters",
                                                         "Save");
            // forward the path to save the external parameters as a file.
            DanbiFileSys.GetResourcePathIntact(out savePath, out _);

            var bf = new BinaryFormatter();
            var file = File.Open(savePath, FileMode.OpenOrCreate);
            bf.Serialize(file, externalData);
            file.Close();
        }

        IEnumerator Coroutine_LoadCameraExternalParametersSelector(Transform panel)
        {
            var filters = new string[] { ".dat", ".DAT" };
            string startingPath = Application.dataPath + "/Resources/";
            yield return DanbiFileSys.OpenLoadDialog(startingPath, filters, "Load Camera Externel Paramaters (Scriptable Object)", "Select");
            DanbiFileSys.GetResourcePathIntact(out loadPath, out _);

            // Load the External parameters.            
            var loaded = default(DanbiCameraExternalData);
            if (File.Exists(loadPath))
            {
                var bf = new BinaryFormatter();
                var file = File.Open(loadPath, FileMode.Open);
                loaded = bf.Deserialize(file) as DanbiCameraExternalData;
            }

            yield return new WaitUntil(() => !(loaded is null));            
            externalData = loaded;
            
            panel.GetChild(2).GetComponent<Text>().text = loadPath;

            (PanelElementsDic["radialCoefficientX"] as InputField).text = externalData.radialCoefficientX.ToString();
            (PanelElementsDic["radialCoefficientY"] as InputField).text = externalData.radialCoefficientY.ToString();
            (PanelElementsDic["radialCoefficientZ"] as InputField).text = externalData.radialCoefficientZ.ToString();

            (PanelElementsDic["tangentialCoefficientX"] as InputField).text = externalData.tangentialCoefficientX.ToString();
            (PanelElementsDic["tangentialCoefficientY"] as InputField).text = externalData.tangentialCoefficientY.ToString();
            (PanelElementsDic["tangentialCoefficientZ"] as InputField).text = externalData.tangentialCoefficientZ.ToString();

            (PanelElementsDic["principalPointX"] as InputField).text = externalData.principalPointX.ToString();
            (PanelElementsDic["principalPointY"] as InputField).text = externalData.principalPointY.ToString();

            (PanelElementsDic["focalLengthX"] as InputField).text = externalData.focalLengthX.ToString();
            (PanelElementsDic["focalLengthY"] as InputField).text = externalData.focalLengthY.ToString();

            (PanelElementsDic["skewCoefficient"] as InputField).text = externalData.skewCoefficient.ToString();
            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

    };
};
