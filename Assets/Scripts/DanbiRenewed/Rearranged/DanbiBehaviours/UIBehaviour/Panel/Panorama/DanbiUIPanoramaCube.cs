using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    [System.Serializable]
    public class DanbiUIPanoramaCube
    {
        [Readonly]
        public float width;
        [Readonly]
        public float depth;
        [Readonly]
        public float ch;
        [Readonly]
        public float cl;
        readonly DanbiUIPanoramaScreenDimensionPanelControl Owner;

        public DanbiUIPanoramaCube(DanbiUIPanoramaScreenDimensionPanelControl owner)
        {
            Owner = owner;
        }

        void LoadPreviousValues(params ILayoutElement[] uiElements)
        {
            float prevWidth = PlayerPrefs.GetFloat("PanoramaCube-width", default);
            width = prevWidth;
            (uiElements[0] as InputField).text = prevWidth.ToString();

            var prevDepth = PlayerPrefs.GetFloat("PanoramaCube-depth", default);
            depth = prevDepth;
            (uiElements[1] as InputField).text = prevDepth.ToString();

            var prevCh = PlayerPrefs.GetFloat("PanoramaCube-ch", default);
            ch = prevCh;
            (uiElements[2] as InputField).text = prevCh.ToString();

            float prevCl = PlayerPrefs.GetFloat("PanoramaCube-cl", default);
            cl = prevCl;
            (uiElements[3] as InputField).text = prevCl.ToString();
            
            DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
        }

        public void BindInput(Transform panel)
        {
            // bind the width
            var widthInputField = panel.GetChild(0).GetComponent<InputField>();
            widthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        width = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the depth
            var depthInputField = panel.GetChild(1).GetComponent<InputField>();
            depthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        depth = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the ch
            var chInputField = panel.GetChild(2).GetComponent<InputField>();
            chInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        ch = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the cl
            var clInputField = panel.GetChild(3).GetComponent<InputField>();
            clInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        cl = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            LoadPreviousValues(widthInputField, depthInputField, chInputField, clInputField);
        }
    };
};