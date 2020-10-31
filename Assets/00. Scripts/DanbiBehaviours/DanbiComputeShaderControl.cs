﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ComputeBuffersDic = System.Collections.Generic.Dictionary<string, UnityEngine.ComputeBuffer>;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiComputeShaderControl : MonoBehaviour
    {
        [SerializeField, Header("2 by default for the best performance"), Readonly]
        int MaxNumOfBounce = 2;

        [SerializeField, Readonly]
        int m_SamplingThreshold = 30;

        public ComputeShader m_rayTracingShader;

        Material m_addMaterial_ScreenSampling;

        int m_SamplingCounter;

        DanbiCameraControl CameraControl;

        public RenderTexture m_resultRT_LowRes;

        public RenderTexture m_convergedResultRT_HiRes;

        public ComputeBuffersDic buffersDic { get; } = new ComputeBuffersDic();

        public delegate void OnValueChanged();
        public static OnValueChanged Call_OnSettingChanged;
        readonly System.DateTime seedDateTime = new System.DateTime();

        void Awake()
        {
            // 1. query the hardward it supports the compute shader.
            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogError("This machine doesn't support Compute Shader!", this);
            }

            // 2. Find Compute Shader in case that it's not assigned.
            if (m_rayTracingShader.Null())
            {
                m_rayTracingShader = DanbiComputeShaderHelper.FindComputeShader("DanbiMain");
            }

            // 3. Initialize the Screen Sampling shader.
            m_addMaterial_ScreenSampling = new Material(Shader.Find("Hidden/AddShader"));

            // 4. Bind the delegates.
            Call_OnSettingChanged += PrepareMeshesAsComputeBuffer;
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;

            // 5. Populate kernels index.
            PopulateKernels();

            DanbiDbg.PrepareDbgBuffers();
        }

        void Start()
        {
            // 1. start with building meshes as compute buffers.
            PrepareMeshesAsComputeBuffer();
        }

        void OnDisable()
        {
            // 1. unbind the delegates.
            Call_OnSettingChanged -= PrepareMeshesAsComputeBuffer;
            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdate;

            // 2. release all the compute buffers.
            foreach (var it in buffersDic)
            {
                it.Value.Release();
            }
        }

        void Update()
        {
            SetShaderParams();

            // Debug.Log($"1 : {DanbiDbg.DbgBuf_direct}");
        }

        void PopulateKernels()
        {
            DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Dome_Reflector_Cube_Panorama,
                m_rayTracingShader.FindKernel("Dome_Reflector_Cube_Panorama"));
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Dome_Reflector_Cylinder_Panorama,
            //     rayTracingShader.FindKernel("Dome_Reflector_Cylinder_Panorama"));

            // foreach (var k in DanbiKernelHelper.KernalDic)
            // {
            //     Debug.Log($"Kernel key {k.Key} -> {k.Value}", this);
            // }
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Halfsphere_Reflector_Cylinder_Panorama, rayTracingShader.FindKernel("Halfsphere_Reflector_Cylinder_Panorama"));
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Cone_Reflector_Cube_Panorama, rayTracingShader.FindKernel("Cone_Reflector_Cube_Panorama"));
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Cone_Reflector_Cylinder_Panorama, rayTracingShader.FindKernel("Cone_Reflector_Cylinder_Panorama"));
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            PrepareMeshesAsComputeBuffer();

            if (control is DanbiUIImageGeneratorParametersPanelControl)
            {
                var imageGeneratorParamPanel = control as DanbiUIImageGeneratorParametersPanelControl;

                MaxNumOfBounce = imageGeneratorParamPanel.maxBoundCount;
                m_SamplingThreshold = imageGeneratorParamPanel.samplingThreshold;
                return;
            }

            if (control is DanbiUIVideoGeneratorParametersPanelControl)
            {
                var videoGeneratorParamPanel = control as DanbiUIVideoGeneratorParametersPanelControl;

                MaxNumOfBounce = videoGeneratorParamPanel.maxBoundCount;
                m_SamplingThreshold = videoGeneratorParamPanel.samplingThreshold;
                return;
            }
        }
        void PrepareMeshesAsComputeBuffer() => DanbiPrewarperSetting.Call_OnPreparePrerequisites?.Invoke(this);

        void SetShaderParams()
        {
            Random.InitState(seedDateTime.Millisecond);
            m_rayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        }

        public void SetBuffersAndRenderTextures(Texture2D panoramaImage, (int x, int y) screenResolutions)
        {
            // 01. Prepare RenderTextures.
            DanbiComputeShaderHelper.PrepareRenderTextures(screenResolutions,
                                                           out m_SamplingCounter,
                                                           ref m_resultRT_LowRes,
                                                           ref m_convergedResultRT_HiRes);

            // 02. Prepare the current kernel for connecting Compute Shader.                    
            int currentKernel = DanbiKernelHelper.CurrentKernelIndex;

            // Set the other parameters as buffer into the ray tracing compute shader.
            m_rayTracingShader.SetBuffer(currentKernel, "_DomeData", buffersDic["_DomeData"]);
            m_rayTracingShader.SetBuffer(currentKernel, "_PanoramaData", buffersDic["_PanoramaData"]);
            m_rayTracingShader.SetInt("_MaxBounce", MaxNumOfBounce);
            m_rayTracingShader.SetBuffer(currentKernel, "_Vertices", buffersDic["_Vertices"]);
            m_rayTracingShader.SetBuffer(currentKernel, "_Indices", buffersDic["_Indices"]);
            m_rayTracingShader.SetBuffer(currentKernel, "_Texcoords", buffersDic["_Texcoords"]);

            // 03. Prepare the translation matrices.
            DanbiCameraControl.Call_OnSetCameraBuffers?.Invoke(screenResolutions, this);

            // 04. Textures.
            // DanbiComputeShaderHelper.ClearRenderTexture(resultRT_LowRes);
            m_rayTracingShader.SetTexture(currentKernel, "_DistortedImage", m_resultRT_LowRes);
            m_rayTracingShader.SetTexture(currentKernel, "_PanoramaImage", panoramaImage);

            // rayTracingShader.SetBuffer(currentKernel, "_Dbg_direct", DanbiDbg.Dbg   Buf_direct);

            m_SamplingCounter = 0;
        }

        public void Dispatch((int x, int y) threadGroups, RenderTexture dest)
        {
            // 01. Check the ray tracing shader is valid.
            if (m_rayTracingShader.Null())
            {
                Debug.LogError("Ray-tracing shader is invalid!", this);
            }

            // 02. Dispatch with the current kernel.
            m_rayTracingShader.Dispatch(DanbiKernelHelper.CurrentKernelIndex, threadGroups.x, threadGroups.y, 1);

            // 03. Check Screen Sampler and apply it.      
            m_addMaterial_ScreenSampling.SetFloat("_SampleCount", m_SamplingCounter);

            // 04. Sample the result into the ConvergedResultRT to improve the aliasing quality.
            Graphics.Blit(m_resultRT_LowRes, m_convergedResultRT_HiRes, m_addMaterial_ScreenSampling);

            // 05. Upscale float precisions to improve the resolution of the result RenderTextue and blit to dest rendertexture.
            Graphics.Blit(m_convergedResultRT_HiRes, dest);

            // 06. Update the sample counts.
            ++m_SamplingCounter;
            if (m_SamplingCounter > m_SamplingThreshold)
            {
                m_SamplingCounter = 0;
                // TODO: Only called for video.
                // One distorted image is finished at last!!
                /// => m_distortedRT = converged_resultRT;
                DanbiControl.Call_OnImageRenderedForVideoFrame?.Invoke(m_convergedResultRT_HiRes);
                // DanbiControl.Call_OnImageRendered?.Invoke(true);
                // StartCoroutine(Coroutine_RenderFinished());
            }
        }

        // IEnumerator Coroutine_RenderFinished()
        // {
        //     // yield return new WaitUntil(() => m_convergedResultRT_HiRes != null);

        // }

    }; // class ending.
}; // namespace Danbi