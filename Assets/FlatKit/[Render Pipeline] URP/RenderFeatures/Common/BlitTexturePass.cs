using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FlatKit
{
    internal class BlitTexturePass : ScriptableRenderPass
    {
        public static readonly string CopyEffectShaderName = "Hidden/FlatKit/CopyTexture";

        private ProfilingSampler _profilingSampler;
        private Material _effectMaterial;
        private Material _copyMaterial;

        private RTHandle _temporaryColorTexture;

        public void Setup(Material effectMaterial, bool useDepth, bool useNormals, bool useColor)
        {
            _effectMaterial = effectMaterial;
            string name = effectMaterial.name.Substring(effectMaterial.name.LastIndexOf('/') + 1);
            _profilingSampler = new ProfilingSampler($"Blit {name}");

            _copyMaterial = CoreUtils.CreateEngineMaterial(CopyEffectShaderName);

            ConfigureInput(
                (useColor ? ScriptableRenderPassInput.Color : ScriptableRenderPassInput.None) |
                (useDepth ? ScriptableRenderPassInput.Depth : ScriptableRenderPassInput.None) |
                (useNormals ? ScriptableRenderPassInput.Normal : ScriptableRenderPassInput.None)
            );
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_effectMaterial == null)
            {
                return;
            }
            if (renderingData.cameraData.camera.cameraType != CameraType.Game)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, _profilingSampler))
            {
                RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;

                RenderingUtils.ReAllocateIfNeeded(
                    ref _temporaryColorTexture,
                    descriptor,
                    name: "_TemporaryColorTexture"
                );

                SetSourceSize(cmd, descriptor);

                RTHandle cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

                // First blit with effect
                Blitter.BlitCameraTexture(
                    cmd,
                    cameraTargetHandle,
                    _temporaryColorTexture,
                    _effectMaterial,
                    0
                );

                // Copy back to camera target
                Blitter.BlitCameraTexture(
                    cmd,
                    _temporaryColorTexture,
                    cameraTargetHandle,
                    _copyMaterial,
                    0
                );
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private static void SetSourceSize(CommandBuffer cmd, RenderTextureDescriptor desc)
        {
            float width = desc.width;
            float height = desc.height;

            if (desc.useDynamicScale)
            {
                width *= ScalableBufferManager.widthScaleFactor;
                height *= ScalableBufferManager.heightScaleFactor;
            }

            cmd.SetGlobalVector("_SourceSize",
                new Vector4(width, height, 1.0f / width, 1.0f / height));
        }
    }
}