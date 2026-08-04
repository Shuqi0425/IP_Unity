using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    internal class LitTessellationShader : BaseShaderGUI
    {
        [Flags]
        protected enum AddonsExpandable
        {
            SurfaceOptions = 1,
            SurfaceInputs = 2,
            Advanced = 4,
            Details = 8,
            RESERVED1 = 16,
            RESERVED2 = 32,
            Geometry = 64,
            GeometryAdvanced = 128,
        }

        // Constants
        static readonly string[] workflowModeNames = Enum.GetNames(typeof(LitGUI.WorkflowMode));

        // Properties
        private LitGUI.LitProperties litProperties;
        private LitDetailGUI.LitProperties litDetailProperties;
        private LitTessellationGUI.LitTessellationProperties litTessellationProperties;

        public override void FillAdditionalFoldouts(MaterialHeaderScopeList materialScopesList)
        {
            materialScopesList.RegisterHeaderScope(LitDetailGUI.Styles.detailInputs, (uint)Expandable.Details, _ => LitDetailGUI.DoDetailArea(litDetailProperties, materialEditor));
            materialScopesList.RegisterHeaderScope(LitTessellationGUI.Styles.geometryLabel, (uint)AddonsExpandable.Geometry, _ => LitTessellationGUI.GeometryInputs(litTessellationProperties, materialEditor));
            materialScopesList.RegisterHeaderScope(LitTessellationGUI.Styles.geometryAdvancedLabel, (uint)AddonsExpandable.GeometryAdvanced, _ => LitTessellationGUI.GeometryAdvanced(litTessellationProperties, materialEditor));
        }

        // Collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);

            // Properties structs
            litProperties = new LitGUI.LitProperties(properties);
            litDetailProperties = new LitDetailGUI.LitProperties(properties);
            litTessellationProperties = new LitTessellationGUI.LitTessellationProperties(properties);
        }

        // Material changed check
        public override void ValidateMaterial(Material material)
        {
            // Set keywords
            SetMaterialKeywords(material, (material) =>
            {
                LitGUI.SetMaterialKeywords(material);
                LitDetailGUI.SetMaterialKeywords(material);
                LitTessellationGUI.SetMaterialKeywords(material);
            });
        }

        // Material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            if (litProperties.workflowMode != null)
                DoPopup(LitGUI.Styles.workflowModeText, litProperties.workflowMode, workflowModeNames);

            base.DrawSurfaceOptions(material);
        }

        // Material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            LitGUI.Inputs(litProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        // Material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            if (litProperties.reflections != null && litProperties.highlights != null)
            {
                materialEditor.ShaderProperty(litProperties.highlights, LitGUI.Styles.highlightsText);
                materialEditor.ShaderProperty(litProperties.reflections, LitGUI.Styles.reflectionsText);
            }

            base.DrawAdvancedOptions(material);
        }

        // New material assignment
        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Blend", (float)blendMode);

            material.SetFloat("_Surface", (float)surfaceType);
            if (surfaceType == SurfaceType.Opaque)
            {
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }

            if (oldShader.name.Equals("Standard (Specular setup)"))
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Specular);
                Texture texture = material.GetTexture("_SpecGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
            else
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Metallic);
                Texture texture = material.GetTexture("_MetallicGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }

            ValidateMaterial(material);
        }

        // Expose tile and offset drawing method
        public static void ExposedDrawTileOffset(MaterialEditor materialEditor, MaterialProperty textureProp) =>
            DrawTileOffset(materialEditor, textureProp);
    }
}
