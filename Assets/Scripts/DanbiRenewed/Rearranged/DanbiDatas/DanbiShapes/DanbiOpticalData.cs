﻿namespace Danbi {
  [System.Serializable]
  public struct DanbiOpticalData {
    // public UnityEngine.Vector3 albedo; // 12
    public UnityEngine.Vector3 specular; // 12
    // public float smoothness; // 4
    // public UnityEngine.Vector3 emission; // 12  

    public int stride => 12;
  }; //40
};