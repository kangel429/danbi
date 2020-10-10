﻿using System.Collections.Generic;

using UnityEngine;

namespace Danbi
{
    [RequireComponent(typeof(DanbiScreen),
                      typeof(DanbiComputeShaderControl),
                      typeof(DanbiCameraControl))]
    public sealed class DanbiControl : MonoBehaviour
    {
        #region Exposed
        // /// <summary>
        // /// When this is true, then current renderTexture is transferred into the frame buffer.  
        // /// </summary>
        // [Readonly, SerializeField]
        // bool bStopRender = false;

        /// <summary>
        /// When this is true, then the current RenderTexture is used for render.
        /// </summary>    
        [Readonly, SerializeField]
        bool isImageRendered = false;

        [Readonly, SerializeField]
        bool isRenderStarted = false;

        [SerializeField, Readonly]
        Texture2D TargetPanoramaTex;

        [Readonly, SerializeField, Header("Current State of Simulator."), Space(20)]
        EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.PREPARE;

        #endregion Exposed

        #region Internal

        /// <summary>
        /// Everything about Shader goes here.
        /// </summary>
        DanbiComputeShaderControl ShaderControl;

        /// <summary>
        /// Result Screen Info.
        /// </summary>
        DanbiScreen Screen;

        /// <summary>
        /// used to raytracing to create an pre-distorted image and to project the distorted image onto the scene
        /// </summary>
        Camera MainCameraCache;

        [SerializeField] DanbiProjector Projector;

        // [SerializeField] KinectSensorManager 
        string fileSaveLocation;

        #endregion Internal

        #region Delegates
        public delegate void OnGenerateImage();
        public static OnGenerateImage Call_OnGenerateImage;
        public delegate void OnSaveImage();
        public static OnSaveImage Call_OnSaveImage;

        public delegate void OnChangeSimulatorMode(EDanbiSimulatorMode mode);
        public static OnChangeSimulatorMode Call_OnChangeSimulatorMode;

        public delegate void OnChangeImageRendered(bool isRendered);
        public static OnChangeImageRendered Call_OnChangeImageRendered;

        // public delegate void OnGenerateImageFinished();
        // public static OnGenerateImageFinished Call_OnGenerateImageFinished;


        // public delegate void OnGenerateVideo();
        // public static OnGenerateVideo Call_OnGenerateVideo;

        // public delegate void OnGenerateVideoFinished();
        // public static OnGenerateVideoFinished Call_OnGenerateVideoFinished;

        // public delegate void OnSaveVideo();
        // public static OnSaveVideo Call_OnSaveVideo;

        public static void UnityEvent_CreatePredistortedImage()
            => DanbiControl.Call_OnGenerateImage?.Invoke();

        // public static void UnityEvent_OnRenderFinished()
        //     => DanbiControl.Call_OnGenerateImageFinished?.Invoke();

        // TODO:
        public static void UnityEvent_SaveImageAt(string path/* = Not used*/)
            => DanbiControl.Call_OnSaveImage?.Invoke();

        #endregion Delegates

        void OnReset()
        {
            isImageRendered = false;
            SimulatorMode = EDanbiSimulatorMode.PREPARE;
        }

        void Start()
        {
            // 1. Acquire resources.
            Screen = GetComponent<DanbiScreen>();
            MainCameraCache = Camera.main;
            ShaderControl = GetComponent<DanbiComputeShaderControl>();

            DanbiImage.ScreenResolutions = Screen.screenResolution;
#if UNITY_EDITOR
            // Turn off unnecessary MeshRenderer settings.
            DanbiDisableMeshFilterProps.DisableAllUnnecessaryMeshRendererProps();
#endif

            // 2. bind the call backs.      
            DanbiControl.Call_OnGenerateImage += Caller_OnGenerateImage;
            // DanbiControl.Call_OnGenerateImageFinished += Caller_OnGenerateImageFinished;
            DanbiControl.Call_OnSaveImage += Caller_OnSaveImage;
            Call_OnChangeSimulatorMode += (EDanbiSimulatorMode mode) => SimulatorMode = mode;
            Call_OnChangeImageRendered += (bool isRendered) => isImageRendered = isRendered;

            // DanbiControl.Call_OnGenerateVideo += Caller_OnGenerateVideo;
            // DanbiControl.Call_OnGenerateVideoFinished += Caller_OnGenerateVideoFinished;
            // DanbiControl.Call_OnSaveVideo += Caller_OnSaveVideo;

            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;
        }

        void OnDisable()
        {
            // Dispose buffer resources.
            DanbiControl.Call_OnGenerateImage -= Caller_OnGenerateImage;
            // DanbiControl.Call_OnGenerateImageFinished -= Caller_OnGenerateImageFinished;
            DanbiControl.Call_OnSaveImage -= Caller_OnSaveImage;

            // DanbiControl.Call_OnGenerateVideo -= Caller_OnGenerateVideo;
            // DanbiControl.Call_OnGenerateVideoFinished -= Caller_OnGenerateVideoFinished;
            // DanbiControl.Call_OnSaveVideo -= Caller_OnSaveVideo;

            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdate;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {

            if (control is DanbiUIImageGeneratorTexturePanelControl)
            {
                var texturePanel = control as DanbiUIImageGeneratorTexturePanelControl;

                TargetPanoramaTex = texturePanel.loadedTex;
            }

            // if (control is DanbiUIVideoGeneratorParametersPanelControl)
            // {
            //     //var videoPanel = control as DanbiUIVideoGeneratorParametersPanelControl;
            //     // TargetPanoramaTex = videoPanel;                
            // }

            if (control is DanbiUIImageGeneratorFilePathPanelControl)
            {
                var fileSavePanel = control as DanbiUIImageGeneratorFilePathPanelControl;

                fileSaveLocation = fileSavePanel.fileSaveLocation;
            }

            DanbiComputeShaderControl.Call_OnValueChanged?.Invoke();
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            switch (SimulatorMode)
            {
                case EDanbiSimulatorMode.PREPARE:
                    // Blit the dest with the current activeTexture (Framebuffer[0]).
                    Graphics.Blit(Camera.main.activeTexture, destination);
                    break;

                case EDanbiSimulatorMode.CAPTURE:
                    if (!isRenderStarted)
                        return;
                    // bStopRender is already true, but the result isn't saved yet (by button).
                    // 
                    // so we stop updating rendering but keep the screen with the result for preventing performance issue.          
                    if (isImageRendered)
                    {
                        Graphics.Blit(ShaderControl.convergedResultRT_HiRes, destination);
                    }
                    else
                    {
                        // 1. Calculate the resolution-wise thread size from the current screen resolution.
                        //    and Dispatch.
                        ShaderControl.Dispatch((Mathf.CeilToInt(Screen.screenResolution.x * 0.125f), Mathf.CeilToInt(Screen.screenResolution.y * 0.125f)),
                                               destination);
                    }
                    break;

                default:
                    Debug.LogError($"Other Value {SimulatorMode} isn't used in this context.", this);
                    break;
            }
        }

        #region Binded Caller    
        void Caller_OnGenerateImage()
        {
            Call_OnChangeImageRendered?.Invoke(false);
            isRenderStarted = true;

            if (Screen.screenResolution.x != 0.0f && Screen.screenResolution.y != 0.0f)
            {
                ShaderControl.MakePredistortedImage(TargetPanoramaTex,
                                                    (Screen.screenResolution.x, Screen.screenResolution.y),
                                                    MainCameraCache);
            }
            else
            {
                (int x, int y) backupScreenResolution = (2560, 1440);
                ShaderControl.MakePredistortedImage(TargetPanoramaTex,
                                                    backupScreenResolution,
                                                    MainCameraCache);
            }

            Call_OnChangeSimulatorMode?.Invoke(EDanbiSimulatorMode.CAPTURE);
            Projector.PrepareResources(ShaderControl, Screen);
        }

        // void Caller_OnGenerateImageFinished()
        // {
        //     isImageRendered = true;
        //     // SimulatorMode = EDanbiSimulatorMode.PREPARE;
        //     // TODO:
        // }

        void Caller_OnSaveImage()
        {
            Call_OnChangeImageRendered?.Invoke(true);
            DanbiFileSys.SaveImage(ref SimulatorMode,
                                   ShaderControl.convergedResultRT_HiRes,
                                   fileSaveLocation,
                                   (Screen.screenResolution.x, Screen.screenResolution.y));
            Call_OnChangeSimulatorMode?.Invoke(EDanbiSimulatorMode.PREPARE);
            // TODO:
        }

        // void Caller_OnGenerateVideo()
        // {
        //     bStopRender = false;
        //     bDistortionReady = false;
        //     SimulatorMode = EDanbiSimulatorMode.CAPTURE;
        // }

        // void Caller_OnGenerateVideoFinished()
        // {
        //     bDistortionReady = true;
        //     SimulatorMode = EDanbiSimulatorMode.PREPARE;
        // }

        // void Caller_OnSaveVideo()
        // {
        //     bStopRender = true;
        //     bDistortionReady = true;
        //     SimulatorMode = EDanbiSimulatorMode.PREPARE;
        // }
        #endregion Binded Caller        
    };
};
