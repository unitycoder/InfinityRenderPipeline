﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace InfinityTech.Rendering.Feature
{
    public class RayTraceGlobalIllumination
    {
        public CommandBuffer CmdBuffer;
        public RayTracingShader TraceGlobalIlluminationShader;
        public RayTracingAccelerationStructure TracingAccelerationStructure;
        public RayTracingAccelerationStructure.RASSettings TracingAccelerationStructureSetting;


        public void OnEnable() {

        }

        public void OnPreRender() {

        }

        public void OnDisable() {

        }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void InitRatTraceGlobalIllumination() {
        
        }
    }
}
