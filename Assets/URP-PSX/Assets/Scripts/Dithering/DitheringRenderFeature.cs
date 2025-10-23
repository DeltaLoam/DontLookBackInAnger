using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PSX
{
    public class DitheringRenderFeature : ScriptableRendererFeature
    {
        DitheringPass ditheringPass;

        public override void Create()
        {
            ditheringPass = new DitheringPass(RenderPassEvent.BeforeRenderingPostProcessing);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // ✅ Use cameraColorTargetHandle instead of cameraColorTarget
            var cameraColorTarget = renderer.cameraColorTargetHandle;
            ditheringPass.Setup(cameraColorTarget);
            renderer.EnqueuePass(ditheringPass);
        }
    }

    public class DitheringPass : ScriptableRenderPass
    {
        private static readonly string shaderPath = "PostEffect/Dithering";
        static readonly string k_RenderTag = "Render Dithering Effects";

        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int TempTargetId = Shader.PropertyToID("_TempTargetDithering");

        // PROPERTIES
        static readonly int PatternIndex = Shader.PropertyToID("_PatternIndex");
        static readonly int DitherThreshold = Shader.PropertyToID("_DitherThreshold");
        static readonly int DitherStrength = Shader.PropertyToID("_DitherStrength");
        static readonly int DitherScale = Shader.PropertyToID("_DitherScale");

        Dithering dithering;
        Material ditheringMaterial;

        // ✅ Use RTHandle instead of RenderTargetIdentifier
        RTHandle currentTarget;

        public DitheringPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
            var shader = Shader.Find(shaderPath);
            if (shader == null)
            {
                Debug.LogError("Shader not found: " + shaderPath);
                return;
            }
            this.ditheringMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        public void Setup(RTHandle currentTarget)
        {
            this.currentTarget = currentTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (this.ditheringMaterial == null)
            {
                Debug.LogError("Material not created.");
                return;
            }

            if (!renderingData.cameraData.postProcessEnabled)
                return;

            var stack = VolumeManager.instance.stack;
            this.dithering = stack.GetComponent<Dithering>();
            if (this.dithering == null || !this.dithering.IsActive())
                return;

            var cmd = CommandBufferPool.Get(k_RenderTag);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var source = currentTarget;
            var w = cameraData.camera.scaledPixelWidth;
            var h = cameraData.camera.scaledPixelHeight;

            cameraData.camera.depthTextureMode |= DepthTextureMode.Depth;

            ditheringMaterial.SetInt(PatternIndex, dithering.patternIndex.value);
            ditheringMaterial.SetFloat(DitherThreshold, dithering.ditherThreshold.value);
            ditheringMaterial.SetFloat(DitherStrength, dithering.ditherStrength.value);
            ditheringMaterial.SetFloat(DitherScale, dithering.ditherScale.value);

            int shaderPass = 0;

            // ✅ Use Blitter instead of cmd.Blit
            Blitter.BlitCameraTexture(cmd, source, source, ditheringMaterial, shaderPass);
        }
    }
}
