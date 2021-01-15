using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;
using InfinityTech.Rendering.RDG;
using UnityEngine.Experimental.Rendering;
using InfinityTech.Rendering.MeshDrawPipeline;

namespace InfinityTech.Rendering.Pipeline
{
    public partial class InfinityRenderPipeline
    {
        struct FOpaqueDepthData
        {
            public RendererList RendererList;
            public RDGTextureRef DepthBuffer;
        }

        void RenderOpaqueDepth(Camera RenderCamera, NativeArray<FMeshBatch> MeshBatchs, FCullingData CullingData, CullingResults CullingResult)
        {
            //Request Resource
            RendererList RenderList = RendererList.Create(CreateRendererListDesc(CullingResult, RenderCamera, InfinityPassIDs.OpaqueDepth, new RenderQueueRange(2450, 3000)));

            RDGTextureDesc DepthDesc = new RDGTextureDesc(RenderCamera.pixelWidth, RenderCamera.pixelHeight) { clearBuffer = true, dimension = TextureDimension.Tex2D, enableMSAA = false, bindTextureMS = false, name = "DepthTexture", depthBufferBits = EDepthBits.Depth32 };
            RDGTextureRef DepthTexture = GraphBuilder.ScopeTexture(InfinityShaderIDs.RT_DepthBuffer, DepthDesc);

            //Add OpaqueDepthPass
            GraphBuilder.AddPass<FOpaqueDepthData>("OpaqueDepth", ProfilingSampler.Get(CustomSamplerId.OpaqueDepth),
            (ref FOpaqueDepthData PassData, ref RDGPassBuilder PassBuilder) =>
            {
                PassData.RendererList = RenderList;
                PassData.DepthBuffer = PassBuilder.UseDepthBuffer(DepthTexture, EDepthAccess.ReadWrite);
            },
            (ref FOpaqueDepthData PassData, RDGContext GraphContext) =>
            {
                RendererList DepthRenderList = PassData.RendererList;
                DepthRenderList.drawSettings.sortingSettings = new SortingSettings(RenderCamera) { criteria = SortingCriteria.QuantizedFrontToBack };
                DepthRenderList.drawSettings.enableInstancing = RenderPipelineAsset.EnableInstanceBatch;
                DepthRenderList.drawSettings.enableDynamicBatching = RenderPipelineAsset.EnableDynamicBatch;
                DepthRenderList.filteringSettings.renderQueueRange = new RenderQueueRange(2450, 2999);
                GraphContext.RenderContext.DrawRenderers(DepthRenderList.cullingResult, ref DepthRenderList.drawSettings, ref DepthRenderList.filteringSettings);
            });
        }
    }
}