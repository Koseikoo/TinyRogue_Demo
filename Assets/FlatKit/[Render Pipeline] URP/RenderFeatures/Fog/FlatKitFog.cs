using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FlatKit
{
    public class FlatKitFog : ScriptableRendererFeature
    {
        [Tooltip("To create new settings use 'Create > FlatKit > Fog Settings'.")]
        public FogSettings settings;

        [SerializeField, HideInInspector]
        private Material _effectMaterial;

        private BlitTexturePass _blitTexturePass;

        private Texture2D _lutDepth;
        private Texture2D _lutHeight;

        private static readonly string FogShaderName = "Hidden/FlatKit/FogFilter";
        private static readonly int DistanceLut = Shader.PropertyToID("_DistanceLUT");
        private static readonly int Near = Shader.PropertyToID("_Near");
        private static readonly int Far = Shader.PropertyToID("_Far");
        private static readonly int UseDistanceFog = Shader.PropertyToID("_UseDistanceFog");
        private static readonly int UseDistanceFogOnSky = Shader.PropertyToID("_UseDistanceFogOnSky");
        private static readonly int DistanceFogIntensity = Shader.PropertyToID("_DistanceFogIntensity");
        private static readonly int HeightLut = Shader.PropertyToID("_HeightLUT");
        private static readonly int LowWorldY = Shader.PropertyToID("_LowWorldY");
        private static readonly int HighWorldY = Shader.PropertyToID("_HighWorldY");
        private static readonly int UseHeightFog = Shader.PropertyToID("_UseHeightFog");
        private static readonly int UseHeightFogOnSky = Shader.PropertyToID("_UseHeightFogOnSky");
        private static readonly int HeightFogIntensity = Shader.PropertyToID("_HeightFogIntensity");
        private static readonly int DistanceHeightBlend = Shader.PropertyToID("_DistanceHeightBlend");

        public override void Create()
        {
            if (settings == null)
            {
                Debug.LogWarning("[FlatKit] Missing Fog Settings");
                return;
            }

            _blitTexturePass = new BlitTexturePass
            {
                renderPassEvent = settings.renderEvent
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (renderingData.cameraData.isPreviewCamera)
            {
                return;
            }
#endif

            if (settings == null)
            {
                Debug.LogWarning("[FlatKit] Missing Fog Settings");
                return;
            }

            if (!CreateMaterials())
            {
                return;
            }

            SetMaterialProperties();

            _blitTexturePass.Setup(_effectMaterial, useDepth: true, useNormals: false, useColor: false);
            renderer.EnqueuePass(_blitTexturePass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_effectMaterial);

            if (_lutDepth != null)
            {
                DestroyImmediate(_lutDepth);
            }

            if (_lutHeight != null)
            {
                DestroyImmediate(_lutHeight);
            }
        }

        private bool CreateMaterials()
        {
            if (_effectMaterial == null)
            {
                Shader effectShader = Shader.Find(FogShaderName);
                Shader blitShader = Shader.Find(BlitTexturePass.CopyEffectShaderName);

                if (effectShader == null || blitShader == null)
                {
                    return false;
                }

                _effectMaterial = CoreUtils.CreateEngineMaterial(effectShader);
            }

            return _effectMaterial != null;
        }

        private void SetMaterialProperties()
        {
            if (_effectMaterial == null)
            {
                return;
            }

            UpdateDistanceLut();
            _effectMaterial.SetTexture(DistanceLut, _lutDepth);
            _effectMaterial.SetFloat(Near, settings.near);
            _effectMaterial.SetFloat(Far, settings.far);
            _effectMaterial.SetFloat(UseDistanceFog, settings.useDistance ? 1f : 0f);
            _effectMaterial.SetFloat(UseDistanceFogOnSky, settings.useDistanceFogOnSky ? 1f : 0f);
            _effectMaterial.SetFloat(DistanceFogIntensity, settings.distanceFogIntensity);

            UpdateHeightLut();
            _effectMaterial.SetTexture(HeightLut, _lutHeight);
            _effectMaterial.SetFloat(LowWorldY, settings.low);
            _effectMaterial.SetFloat(HighWorldY, settings.high);
            _effectMaterial.SetFloat(UseHeightFog, settings.useHeight ? 1f : 0f);
            _effectMaterial.SetFloat(UseHeightFogOnSky, settings.useHeightFogOnSky ? 1f : 0f);
            _effectMaterial.SetFloat(HeightFogIntensity, settings.heightFogIntensity);

            _effectMaterial.SetFloat(DistanceHeightBlend, settings.distanceHeightBlend);
        }

        private void UpdateDistanceLut()
        {
            if (settings.distanceGradient == null)
            {
                return;
            }

            if (_lutDepth != null)
            {
                DestroyImmediate(_lutDepth);
            }

            const int width = 256;
            _lutDepth = new Texture2D(width, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear
            };

            for (int x = 0; x < width; x++)
            {
                Color color = settings.distanceGradient.Evaluate(x / (float)(width - 1));
                _lutDepth.SetPixel(x, 0, color);
            }

            _lutDepth.Apply();
        }

        private void UpdateHeightLut()
        {
            if (settings.heightGradient == null)
            {
                return;
            }

            if (_lutHeight != null)
            {
                DestroyImmediate(_lutHeight);
            }

            const int width = 256;
            _lutHeight = new Texture2D(width, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear
            };

            for (int x = 0; x < width; x++)
            {
                Color color = settings.heightGradient.Evaluate(x / (float)(width - 1));
                _lutHeight.SetPixel(x, 0, color);
            }

            _lutHeight.Apply();
        }
    }
}