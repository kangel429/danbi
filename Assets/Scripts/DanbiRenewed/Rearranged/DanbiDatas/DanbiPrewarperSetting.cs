﻿using UnityEngine;

namespace Danbi {
  public class DanbiPrewarperSetting : MonoBehaviour {
    [SerializeField]
    EDanbiPrewarperSetting_MeshType MeshType;
    public EDanbiPrewarperSetting_MeshType meshType { get => MeshType; set => MeshType = value; }

    [SerializeField]
    EDanbiPrewarperSetting_PanoramaType PanoramaType;
    public EDanbiPrewarperSetting_PanoramaType panoramaType { get => PanoramaType; set => PanoramaType = value; }

    [SerializeField]
    DanbiBaseShape Shape;
    public DanbiBaseShape shape { get => Shape; set => Shape = value; }

    [SerializeField]
    DanbiBaseShape PanoramaShape;
    public DanbiBaseShape panoramaShape { get => PanoramaShape; set => PanoramaShape = value; }

    DanbiCameraInternalParameters CamParams;
    [SerializeField]
    public DanbiCameraInternalParameters camParams { get => CamParams; set => CamParams = value; }

    [SerializeField]
    string KernalName;
    public string kernalName { get => KernalName; set => KernalName = value; }

    void OnEnable() {
      if (!Shape.Null()) {
        return;
      }

      foreach (var it in GetComponentsInChildren<DanbiBaseShape>()) {
        if (it is DanbiBaseShape) {
          Shape = it;
          DanbiComputeShaderControl.RegisterNewPrewarperSetting(this);
        }
      }
    }

  };
};
