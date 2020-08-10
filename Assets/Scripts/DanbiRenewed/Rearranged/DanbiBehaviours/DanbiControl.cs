﻿using System.Collections.Generic;

using UnityEngine;

namespace Danbi {
  [System.Serializable,
    RequireComponent(typeof(DanbiScreen),
                     typeof(DanbiComputeShaderControl),
                     typeof(DanbiUIControl))]
  public class DanbiControl : MonoBehaviour {
    #region Exposed
    /// <summary>
    /// When this is true, then current renderTexture is transferred into the frame buffer.  
    /// </summary>
    [Readonly, SerializeField, Header("It toggled off to false after the image is saved.")]
    bool bStopRender = false;

    /// <summary>
    /// 
    /// </summary>
    [Readonly, SerializeField, Header("When this is true, then the current RenderTexture is used for render.")]
    bool bDistortionReady = false;

    [Readonly, SerializeField, Header("")]
    bool bCaptureFinished = false;

    [SerializeField, Header("It affects to the Scene at editor-time and at run-time")]
    Texture2D TargetPanoramaTex;

    [Readonly, SerializeField, Space(20)]
    EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.CAPTURE;
    #endregion Exposed

    #region Internal
    /// <summary>
    /// 
    /// </summary>
    DanbiComputeShaderControl ShaderControl;
    /// <summary>
    /// 
    /// </summary>
    DanbiScreen Screen;
    /// <summary>
    /// 
    /// </summary>
    public Texture2D targetPanoramaTex { get => TargetPanoramaTex; set => TargetPanoramaTex = value; }
    /// <summary>
    /// 
    /// </summary>
    List<PanoramaScreenObject> CurrentPanoramaList = new List<PanoramaScreenObject>();

    /// <summary>
    /// used to raytracing to obtain  distorted image and to project the distorted image onto the scene
    /// </summary>
    Camera MainCameraCache;

    public delegate void OnRenderStarted();
    public static OnRenderStarted Call_OnRenderStarted;

    public delegate void OnRenderFinished();
    public static OnRenderFinished Call_OnRenderFinished;

    public delegate void OnSaveImage();
    public static OnSaveImage Call_OnSaveImage;
    #endregion Internal

    #region Delegates
    public static void UnityEvent_CreatePredistortedImage() => Call_OnRenderStarted?.Invoke();

    public static void UnityEvent_OnRenderFinished() => Call_OnRenderFinished?.Invoke();

    public static void UnityEvent_SaveImageAt(string path/* = Not used*/) => Call_OnSaveImage?.Invoke();

    #endregion Delegates

    /// <summary>
    /// Reset() is called when the script is attached and not in playmode.
    /// </summary>
    void Reset() {
      Screen = GetComponent<DanbiScreen>();
      MainCameraCache = Camera.main;
      ShaderControl = GetComponent<DanbiComputeShaderControl>();

      DanbiImage.ScreenResolutions = Screen.screenResolution;
      DanbiDisableMeshFilterProps.DisableAllUnnecessaryMeshRendererProps();
    }

    void Start() {
      // 1. bind the call backs.      
      DanbiControl.Call_OnRenderStarted += Caller_RenderStarted;
      DanbiControl.Call_OnRenderFinished += Caller_RenderFinished;
      DanbiControl.Call_OnSaveImage += Caller_SaveImage;

    }

    void OnValidate() {
      /**/
    }

    void OnDisable() {
      // Dispose buffer resources.
      DanbiControl.Call_OnRenderStarted -= Caller_RenderStarted;
      DanbiControl.Call_OnRenderFinished -= Caller_RenderFinished;
      DanbiControl.Call_OnSaveImage -= Caller_SaveImage;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
      switch (SimulatorMode) {
        case EDanbiSimulatorMode.PREPARE: {
            // Nothing to do on the prepare stage.
          }
          return;

        case EDanbiSimulatorMode.CAPTURE: {
            // bStopRender is already true, but the result isn't saved yet (by button).
            // 
            // so we stop updating rendering but keep the screen with the result for preventing performance issue.          
            if (bDistortionReady) {
              Graphics.Blit(ShaderControl.ResultRT_LowRes, destination);
            }
            else {
              // 1. Calculate the resolution-wise thread size from the current screen resolution.
              //    and Dispatch.
              ShaderControl.Dispatch((Mathf.CeilToInt(Screen.screenResolution.x * 0.125f),
                                      Mathf.CeilToInt(Screen.screenResolution.y * 0.125f)),
                                        destination);
            }
          }
          break;

        default: {
            Debug.LogError($"Other Value {SimulatorMode} isn't used in this context.", this);

          }
          break;

      }
    }

    #region Binded Caller    
    void Caller_RenderStarted() {
      ShaderControl.MakePredistortedImage(TargetPanoramaTex, (Screen.screenResolution.x, Screen.screenResolution.y));
      SimulatorMode = EDanbiSimulatorMode.CAPTURE;
    }

    void Caller_RenderFinished() {
      bDistortionReady = true;
      SimulatorMode = EDanbiSimulatorMode.PREPARE;
    }

    void Caller_SaveImage() {
      bStopRender = true;
      SimulatorMode = EDanbiSimulatorMode.PREPARE;
    }
    #endregion Binded Caller
  };
};