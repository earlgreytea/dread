using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class StreakFlareRenderPass : ScriptableRenderPass
{
    const string k_PassName = "StreakFlarePass";
    private Material m_StreakFlareMaterial = null;

    static readonly int m_WaveDistance = Shader.PropertyToID("_WaveDistance");
    static readonly int m_TrailLenght = Shader.PropertyToID("_TrailLenght");
    static readonly int m_WaveColor = Shader.PropertyToID("_WaveColor");

    public StreakFlareRenderPass(StreakFlareRendererFeature.Settings settings)
    {
        renderPassEvent = settings.renderPassEvent;
        m_StreakFlareMaterial = settings.blitMaterial;

        SetMaterialData(settings);

        requiresIntermediateTexture = true;
    }

    private void SetMaterialData(StreakFlareRendererFeature.Settings settings)
    {
        // Set any material properties based on our pass settings.
        m_StreakFlareMaterial.SetFloat(m_WaveDistance, settings.WaveDistance);
        m_StreakFlareMaterial.SetFloat(m_TrailLenght, settings.TrailLenght);
        m_StreakFlareMaterial.SetColor(m_WaveColor, settings.WaveColor);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        //VolumeStack stack = VolumeManager.instance.stack;
        //FullScreenVolumeComponent customVolume = stack.GetComponent<FullScreenVolumeComponent>();

        //if (!customVolume.IsActive()) return;

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        if (resourceData.isActiveTargetBackBuffer)
        {
            Debug.LogError(
                $"Skipping render pass. FullScreenRendererFeature requires an intermediate ColorTexture, we can't use the BackBuffer as a texture input."
            );
            return;
        }

        // Get the source texture
        var source = resourceData.activeColorTexture;

        // Create the destination texture
        var destinationDesc = renderGraph.GetTextureDesc(source);
        destinationDesc.name = $"CameraColor-{k_PassName}";
        destinationDesc.clearBuffer = false;

        TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

        // Create blit parameters and Blit
        RenderGraphUtils.BlitMaterialParameters para = new RenderGraphUtils.BlitMaterialParameters(
            source,
            destination,
            m_StreakFlareMaterial,
            0
        );
        renderGraph.AddBlitPass(para, passName: k_PassName);

        // Asign to the color texture the destination value after the blit
        resourceData.cameraColor = destination;
    }
}
