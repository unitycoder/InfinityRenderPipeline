﻿using UnityEngine;
using UnityEngine.Rendering;
using InfinityTech.Rendering.RenderGraph;
using UnityEngine.Experimental.Rendering;
using InfinityTech.Rendering.GPUResource;
using InfinityTech.Rendering.MeshPipeline;
using UnityEngine.Rendering.RendererUtils;

namespace InfinityTech.Rendering.Pipeline
{
    internal static class GBufferPassUtilityData
    {
        internal static string TextureAName = "GBufferTextureA";
        internal static string TextureBName = "GBufferTextureB";
    }

    public partial class InfinityRenderPipeline
    {
        struct GBufferPassData
        {
            public RendererList rendererList;
            public RGTextureRef depthTexture;
            public RGTextureRef gbufferTextureA;
            public RGTextureRef gbufferTextureB;
            public MeshPassProcessor meshPassProcessor;
        }

        void RenderGBuffer(RenderContext renderContext, Camera camera, in CullingDatas cullingDatas, in CullingResults cullingResults)
        {
            TextureDescriptor gbufferADsc = new TextureDescriptor(camera.pixelWidth, camera.pixelHeight) { dimension = TextureDimension.Tex2D, name = GBufferPassUtilityData.TextureAName, colorFormat = GraphicsFormat.R8G8B8A8_UNorm, depthBufferBits = EDepthBits.None };
            TextureDescriptor gbufferBDsc = new TextureDescriptor(camera.pixelWidth, camera.pixelHeight) { dimension = TextureDimension.Tex2D, name = GBufferPassUtilityData.TextureBName, colorFormat = GraphicsFormat.R8G8B8A8_UNorm, depthBufferBits = EDepthBits.None };
                    
            RGTextureRef depthTexture = m_RGScoper.QueryTexture(InfinityShaderIDs.DepthBuffer);
            RGTextureRef gbufferTextureA = m_RGScoper.CreateAndRegisterTexture(InfinityShaderIDs.GBufferA, gbufferADsc);
            RGTextureRef gbufferTextureB = m_RGScoper.CreateAndRegisterTexture(InfinityShaderIDs.GBufferB, gbufferBDsc);
            
            //Add GBufferPass
            using (RGRasterPassRef passRef = m_RGBuilder.AddRasterPass<GBufferPassData>(ProfilingSampler.Get(CustomSamplerId.RenderGBuffer)))
            {
                //Setup Phase
                passRef.SetOption(ClearFlag.Color, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);

                RendererListDesc rendererListDesc = new RendererListDesc(InfinityPassIDs.GBufferPass, cullingResults, camera);
                {
                    rendererListDesc.layerMask = camera.cullingMask;
                    rendererListDesc.renderQueueRange = new RenderQueueRange(0, 2999);
                    rendererListDesc.sortingCriteria = SortingCriteria.QuantizedFrontToBack;
                    rendererListDesc.renderingLayerMask = 1;
                    rendererListDesc.rendererConfiguration = PerObjectData.None;
                    rendererListDesc.excludeObjectMotionVectors = false;
                }

                ref GBufferPassData passData = ref passRef.GetPassData<GBufferPassData>();
                passData.rendererList = renderContext.scriptableRenderContext.CreateRendererList(rendererListDesc);
                passData.meshPassProcessor = m_GBufferMeshProcessor;
                passData.gbufferTextureA = passRef.UseColorBuffer(gbufferTextureA, 0);
                passData.gbufferTextureB = passRef.UseColorBuffer(gbufferTextureB, 1);
                passData.depthTexture = passRef.UseDepthBuffer(depthTexture, EDepthAccess.ReadWrite);
                
                m_GBufferMeshProcessor.DispatchSetup(cullingDatas, new MeshPassDescriptor(0, 2999));

                //Execute Phase
                passRef.SetExecuteFunc((in GBufferPassData passData, in RGContext graphContext) =>
                {
                    //MeshDrawPipeline
                    passData.meshPassProcessor.DispatchDraw(graphContext, 1);

                    //UnityDrawPipeline
                    graphContext.cmdBuffer.DrawRendererList(passData.rendererList);
                });
            }
        }
    }
}