using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class OutlineRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public struct Settings
    {
        public RenderPassEvent passEvent;
    }

    public Settings settings = new Settings()
    {
        passEvent = RenderPassEvent.AfterRenderingPostProcessing
    };

    private static Material outlineMaterial;
    private static Material copyNormalMaterial;
    private static Material copyDepthMaterial;

    private static RTHandle cameraColorTarget;

    private static RTHandle colorTarget;
    private static RTHandle normalTarget;
    private static RTHandle depthTarget;

    private static RTHandle dummyTarget;

    private OutlinePass outlinePass = null;

    public override void Create()
    {
        copyNormalMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Shader Graphs/GetSceneNormal"));
        copyDepthMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Shader Graphs/GetSceneDepth"));
        outlineMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Shader Graphs/Outline"));

        outlinePass = new OutlinePass(this);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game) return;

        renderer.EnqueuePass(outlinePass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType != CameraType.Game) return;

        cameraColorTarget = renderer.cameraColorTargetHandle;

        outlinePass.ConfigureInput(ScriptableRenderPassInput.Color);
        outlinePass.ConfigureInput(ScriptableRenderPassInput.Normal);
        outlinePass.ConfigureInput(ScriptableRenderPassInput.Depth);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(outlineMaterial);
        CoreUtils.Destroy(copyNormalMaterial);
        CoreUtils.Destroy(copyDepthMaterial);

        colorTarget?.Release();
        normalTarget?.Release();
        depthTarget?.Release();

        dummyTarget?.Release();
    }

    class OutlinePass : ScriptableRenderPass
    {
        public OutlineRendererFeature feature;

        public OutlinePass(OutlineRendererFeature feature)
        {
            this.feature = feature;

            renderPassEvent = feature.settings.passEvent;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);

            RenderTextureDescriptor cameraDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            RenderTextureDescriptor descriptor;

            descriptor = new RenderTextureDescriptor(cameraDescriptor.width, cameraDescriptor.height, RenderTextureFormat.DefaultHDR);
            RenderingUtils.ReAllocateIfNeeded(ref colorTarget, descriptor, name: "_Outline_Color");
            RenderingUtils.ReAllocateIfNeeded(ref normalTarget, descriptor, name: "_Outline_Normal");
            RenderingUtils.ReAllocateIfNeeded(ref depthTarget, descriptor, name: "_Outline_Depth");

            descriptor = new RenderTextureDescriptor(1, 1, RenderTextureFormat.Default);
            RenderingUtils.ReAllocateIfNeeded(ref dummyTarget, descriptor, name: "_Outline_Dummy");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);

            ConfigureTarget(cameraColorTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CameraData cameraData = renderingData.cameraData;
            if (cameraData.camera.cameraType != CameraType.Game) return;

            CommandBuffer cmd = CommandBufferPool.Get("Outline");

            Blit(cmd, dummyTarget, normalTarget, copyNormalMaterial);
            Blit(cmd, dummyTarget, depthTarget, copyDepthMaterial);
            Blit(cmd, cameraColorTarget, colorTarget);

            outlineMaterial.SetTexture("_Color_Buffer", colorTarget.rt);
            outlineMaterial.SetTexture("_Normal_Buffer", normalTarget.rt);
            outlineMaterial.SetTexture("_Depth_Buffer", depthTarget.rt);

            Blit(cmd, dummyTarget, cameraColorTarget, outlineMaterial);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }
    }
}
