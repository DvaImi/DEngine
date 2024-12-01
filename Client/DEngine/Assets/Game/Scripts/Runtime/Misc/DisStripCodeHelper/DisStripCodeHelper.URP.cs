using UnityEngine.Rendering.Universal;

public partial class DisStripCodeHelper
{
    private void RegisterURPTypes()
    {
#if UNITY_RENDER_PIPELINE_UNIVERSAL
        RegisterType<UniversalRenderPipeline>();
        RegisterType<UniversalAdditionalCameraData>();
        RegisterType<UniversalAdditionalLightData>();
        RegisterType<ScriptableRenderer>();
        RegisterType<ScriptableRendererFeature>();
        RegisterType<ScriptableRenderPass>();
#endif
    }
}