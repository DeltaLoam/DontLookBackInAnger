using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PSX
{
    public class PixelationRenderFeature : ScriptableRendererFeature
    {
        PixelationPass pixelationPass;

        public override void Create()
        {
            pixelationPass = new PixelationPass(RenderPassEvent.BeforeRenderingPostProcessing);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // ✅ use cameraColorTargetHandle instead of cameraColorTarget
            var cameraColorTarget = renderer.cameraColorTargetHandle;
            pixelationPass.Setup(cameraColorTarget);
            renderer.EnqueuePass(pixelationPass);
        }
    }

    public class PixelationPass : ScriptableRenderPass
    {
        private static readonly string shaderPath = "PostEffect/Pixelation";
        static readonly string k_RenderTag = "Render Pixelation Effects";
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int TempTargetId = Shader.PropertyToID("_TempTargetPixelation");

        static readonly int WidthPixelation = Shader.PropertyToID("_WidthPixelation");
        static readonly int HeightPixelation = Shader.PropertyToID("_HeightPixelation");
        static readonly int ColorPrecision = Shader.PropertyToID("_ColorPrecision");

        Pixelation pixelation;
        Material pixelationMaterial;

        // ✅ use RTHandle instead of RenderTargetIdentifier
        RTHandle currentTarget;

        public PixelationPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find(shaderPath);
            if (shader == null)
            {
                Debug.LogError("Shader not found.");
                return;
            }
            this.pixelationMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        public void Setup(RTHandle currentTarget)
        {
            this.currentTarget = currentTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (this.pixelationMaterial == null)
            {
                Debug.LogError("Material not created.");
                return;
            }

            if (!renderingData.cameraData.postProcessEnabled) return;

            var stack = VolumeManager.instance.stack;
            this.pixelation = stack.GetComponent<Pixelation>();
            if (this.pixelation == null || !this.pixelation.IsActive()) return;

            var cmd = CommandBufferPool.Get(k_RenderTag);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var source = currentTarget;
            int destination = TempTargetId;

            var w = cameraData.camera.scaledPixelWidth;
            var h = cameraData.camera.scaledPixelHeight;

            cameraData.camera.depthTextureMode |= DepthTextureMode.Depth;

            pixelationMaterial.SetFloat(WidthPixelation, pixelation.widthPixelation.value);
            pixelationMaterial.SetFloat(HeightPixelation, pixelation.heightPixelation.value);
            pixelationMaterial.SetFloat(ColorPrecision, pixelation.colorPrecision.value);

            int shaderPass = 0;

            // ✅ Updated blit syntax for RTHandles
            Blitter.BlitCameraTexture(cmd, source, source, pixelationMaterial, shaderPass);
        }
    }
}
