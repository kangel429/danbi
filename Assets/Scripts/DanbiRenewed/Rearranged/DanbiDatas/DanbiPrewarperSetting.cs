﻿using System.Collections.Generic;
using System.Linq;

using UnityEngine;

// using AdditionalData = System.ValueTuple<Danbi.DanbiOpticalData, Danbi.DanbiShapeTransform>;

namespace Danbi
{
    public sealed class DanbiPrewarperSetting : MonoBehaviour
    {
        [SerializeField]
        EDanbiPrewarperSetting_MeshType MeshType;

        [SerializeField]
        EDanbiPrewarperSetting_PanoramaType PanoramaType;

        [SerializeField]
        string KernalName;

        /// <summary>
        /// Stride of this prewarper set.
        /// </summary>
        public int stride => CalcStride();

        DanbiBaseShape Reflector;
        DanbiBaseShape Panorama;

        public DanbiCameraExternalData camAdditionalData { get; set; }

        public delegate void OnMeshRebuild(DanbiComputeShaderControl control);
        public OnMeshRebuild Call_OnMeshRebuild;

        void Awake()
        {
            Call_OnMeshRebuild += Caller_OnMeshRebuild;
            DanbiComputeShaderControl.Call_OnShaderParamsUpdated += Caller_OnShaderParamsUpdated;

            #region Assign resources      
            // 1. Assign automatically the reflector and the Panorama screen.
            foreach (var it in GetComponentsInChildren<DanbiBaseShape>())
            {
                if (!(it is DanbiBaseShape))
                    continue;

                if (it.name.Contains("Reflector"))
                {
                    Reflector = it;
                }

                if (it.name.Contains("Panorama"))
                {
                    Panorama = it;
                }
            }
            #endregion Assign resources      
        }

        void OnDisable()
        {
            Call_OnMeshRebuild -= Caller_OnMeshRebuild;
            DanbiComputeShaderControl.Call_OnShaderParamsUpdated -= Caller_OnShaderParamsUpdated;
        }

        void Caller_OnMeshRebuild(DanbiComputeShaderControl control)
        {
            // 1. Clear every data before rebuilt every meshes into the POD_meshdata.      
            var opticalDataList = new List<DanbiOpticalData>();
            var shapeTransformList = new List<DanbiHalfsphereData>();
            var meshData = new DanbiMeshData();

            DanbiOpticalData opticalData = default;
            DanbiHalfsphereData shapeTransform = default;

            // 2. fill out the meshData for mesh geometries and the additionalData for Shader.
            Reflector.Call_OnMeshRebuild?.Invoke(ref meshData, out opticalData, out shapeTransform);
            opticalDataList.Add(opticalData);
            shapeTransformList.Add(shapeTransform);

            Panorama.Call_OnMeshRebuild?.Invoke(ref meshData, out opticalData, out shapeTransform);
            opticalDataList.Add(opticalData);
            shapeTransformList.Add(shapeTransform);

            // 3. Find Kernel and set it as a current kernel.
            //DanbiKernelHelper.AddKernalIndexWithKey(KernalName, control.rtShader.FindKernel("/*TODO*/"));
            //DanbiKernelHelper.CurrentKernelIndex = DanbiKernelHelper.GetKernalIndex(KernalName);

            // 4. Create new ComputeBuffer.      
            control.buffersDic.Add("_Vertices", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<Vector3>(meshData.Vertices, 12));
            control.buffersDic.Add("_Indices", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<int>(meshData.Indices, 4));
            control.buffersDic.Add("_Texcoords", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<Vector2>(meshData.Texcoords, 8));
            //control.buffersDic.Add("_MeshAdditionalData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiOpticalData>(opticalDataList, stride));
            control.buffersDic.Add("_MeshAdditionalData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret<DanbiHalfsphereData>(shapeTransformList, stride));
        }

        void Caller_OnShaderParamsUpdated()
        {

        }

        int CalcStride()
        {
            int res = 0;
            // 1. Create Shape MeshAdditionalData by MeshType.
            switch (MeshType)
            {
                case EDanbiPrewarperSetting_MeshType.Custom_Cone:
                    break;

                case EDanbiPrewarperSetting_MeshType.Custom_Cube:
                    break;

                case EDanbiPrewarperSetting_MeshType.Custom_Cylinder:
                    break;

                case EDanbiPrewarperSetting_MeshType.Custom_Hemisphere:
                    break;

                case EDanbiPrewarperSetting_MeshType.Custom_Pyramid:
                    break;

                case EDanbiPrewarperSetting_MeshType.Procedural_Cylinder:
                    break;

                case EDanbiPrewarperSetting_MeshType.Procedural_Hemisphere:
                    break;
            }

            // 2. Create Panorama MeshAdditionalData by PanoramaType.
            switch (PanoramaType)
            {
                case EDanbiPrewarperSetting_PanoramaType.Cube_panorama:
                    break;

                case EDanbiPrewarperSetting_PanoramaType.Cylinder_panorama:
                    break;
            }

            // 4. Add DanbiOpticalData.
            // res += Reflector.opticalData.stride;
            // res += Panorama.opticalData.stride;

            // 5. Add DanbiShapeTransform.
            if (Reflector is DanbiCylinder)
            {
                res += (Reflector as DanbiCylinder).shapeTransform.stride;
            }

            if (Reflector is DanbiCone)
            {
                res += (Reflector as DanbiCone).shapeTransform.stride;
            }

            if (Reflector is DanbiHalfSphere)
            {
                res += (Reflector as DanbiHalfSphere).shapeTransform.stride;
            }

            res += (Panorama as DanbiCubePanorama).shapeTransform.stride;

            // 7. Add DanbiCameraInternalParameters.
            // res += camAdditionalData.stride;

            return res;
        }

    };
};
