﻿using UnityEditor;
using InfinityTech.Component;

namespace InfinityTech.Editor.Component
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LightComponent))]
    public class LightComponentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {

        }
    }
}
