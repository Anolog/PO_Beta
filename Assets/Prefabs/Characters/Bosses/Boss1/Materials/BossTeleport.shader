// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/BossTeleport"
{
    Properties
    {
        _FadeTime("Fade Time", float) = 0

        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

    _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

    [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

    _Parallax("Height Scale", Range(0.005, 0.08)) = 0.02
        _ParallaxMap("Height Map", 2D) = "black" {}

    _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

    _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

    _DetailMask("Detail Mask", 2D) = "white" {}

    _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
    _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailNormalMap("Normal Map", 2D) = "bump" {}

    [Enum(UV0,0,UV1,1)] _UVSec("UV Set for secondary textures", Float) = 0


        // Blending state
        [HideInInspector] _Mode("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
    }

        CGINCLUDE
#define UNITY_SETUP_BRDF_INPUT MetallicSetup
        ENDCG

        SubShader
    {
        Tags{ "RenderType" = "Opaque" "PerformanceChecks" = "False" }
        LOD 300


        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
    {
        Name "FORWARD"
        Tags{ "LightMode" = "ForwardBase" }

        Blend[_SrcBlend][_DstBlend]
        ZWrite[_ZWrite]

        CGPROGRAM
#pragma target 3.0

        // -------------------------------------

#pragma shader_feature _NORMALMAP
#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
#pragma shader_feature _EMISSION
#pragma shader_feature _METALLICGLOSSMAP
#pragma shader_feature ___ _DETAIL_MULX2
#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
#pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
#pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
#pragma shader_feature _PARALLAXMAP

#pragma multi_compile_fwdbase
#pragma multi_compile_fog
#pragma multi_compile_instancing
        // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
        //#pragma multi_compile _ LOD_FADE_CROSSFADE

#pragma vertex vertBase
#pragma fragment fragForwardBaseBossTeleport
#include "UnityStandardCoreForward.cginc"
#include "UnityStandardCore.cginc"
#include "UnityStandardConfig.cginc"

        struct VertexOutputBaseSimple
    {
        UNITY_POSITION(pos);
        float4 tex                          : TEXCOORD0;
        half4 eyeVec                        : TEXCOORD1; // w: grazingTerm

        half4 ambientOrLightmapUV           : TEXCOORD2; // SH or Lightmap UV
        SHADOW_COORDS(3)
            UNITY_FOG_COORDS_PACKED(4, half4) // x: fogCoord, yzw: reflectVec

            half4 normalWorld                   : TEXCOORD5; // w: fresnelTerm

#ifdef _NORMALMAP
        half3 tangentSpaceLightDir          : TEXCOORD6;
#if SPECULAR_HIGHLIGHTS
        half3 tangentSpaceEyeVec        : TEXCOORD7;
#endif
#endif
#if UNITY_REQUIRE_FRAG_WORLDPOS
        float3 posWorld                     : TEXCOORD8;
#endif

        UNITY_VERTEX_OUTPUT_STEREO
    };

        float _FadeTime;
        //sampler2D _MainTex;
        //float4 _MainTex_ST;

#if !SPECULAR_HIGHLIGHTS
#   define REFLECTVEC_FOR_SPECULAR(i, s) half3(0, 0, 0)
#elif defined(_NORMALMAP)
#   define REFLECTVEC_FOR_SPECULAR(i, s) reflect(i.tangentSpaceEyeVec, s.tangentSpaceNormal)
#else
#   define REFLECTVEC_FOR_SPECULAR(i, s) s.reflUVW
#endif

        UnityLight MainLightSimple(VertexOutputBaseSimple i, FragmentCommonData s)
        {
            UnityLight mainLight = MainLight();
            return mainLight;
        }

        half3 LightDirForSpecular(VertexOutputBaseSimple i, UnityLight mainLight)
        {
#if SPECULAR_HIGHLIGHTS && defined(_NORMALMAP)
            return i.tangentSpaceLightDir;
#else
            return mainLight.dir;
#endif
        }

        half PerVertexGrazingTerm(VertexOutputBaseSimple i, FragmentCommonData s)
        {
#if GLOSSMAP
            return saturate(s.smoothness + (1 - s.oneMinusReflectivity));
#else
            return i.eyeVec.w;
#endif
        }

        half PerVertexFresnelTerm(VertexOutputBaseSimple i)
        {
            return i.normalWorld.w;
        }

        half3 BRDF3DirectSimple(half3 diffColor, half3 specColor, half smoothness, half rl)
        {
#if SPECULAR_HIGHLIGHTS
            return BRDF3_Direct(diffColor, specColor, Pow4(rl), smoothness);
#else
            return diffColor;
#endif
        }

        FragmentCommonData FragmentSetupSimple(VertexOutputBaseSimple i)
        {
            half alpha = Alpha(i.tex.xy);
#if defined(_ALPHATEST_ON)
            clip(alpha - _Cutoff);
#endif

            FragmentCommonData s = UNITY_SETUP_BRDF_INPUT(i.tex);

            // NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
            s.diffColor = PreMultiplyAlpha(s.diffColor, alpha, s.oneMinusReflectivity, /*out*/ s.alpha);

            s.normalWorld = i.normalWorld.xyz;
            s.eyeVec = i.eyeVec.xyz;
            s.posWorld = i.posWorld;
            //s.reflUVW = i.fogCoord.yzw;

//#ifdef _NORMALMAP
//            s.tangentSpaceNormal = NormalInTangentSpace(i.tex);
//#else
//            s.tangentSpaceNormal = 0;
//#endif

            return s;
        }

        half4 fragForwardBaseBossTeleport(VertexOutputBaseSimple i) : COLOR
    {
        UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

        FragmentCommonData s = FragmentSetupSimple(i);

        UnityLight mainLight = MainLightSimple(i, s);

#if !defined(LIGHTMAP_ON) && defined(_NORMALMAP)
        half ndotl = saturate(dot(s.tangentSpaceNormal, i.tangentSpaceLightDir));
#else
        half ndotl = saturate(dot(s.normalWorld, mainLight.dir));
#endif

        //we can't have worldpos here (not enough interpolator on SM 2.0) so no shadow fade in that case.
        half shadowMaskAttenuation = UnitySampleBakedOcclusion(i.ambientOrLightmapUV, 0);
        half realtimeShadowAttenuation = SHADOW_ATTENUATION(i);
        half atten = UnityMixRealtimeAndBakedShadows(realtimeShadowAttenuation, shadowMaskAttenuation, 0);

        half occlusion = Occlusion(i.tex.xy);
        half rl = dot(REFLECTVEC_FOR_SPECULAR(i, s), LightDirForSpecular(i, mainLight));

        UnityGI gi = FragmentGI(s, occlusion, i.ambientOrLightmapUV, atten, mainLight);
        half3 attenuatedLightColor = gi.light.color * ndotl;

        half3 c = BRDF3_Indirect(s.diffColor, s.specColor, gi.indirect, PerVertexGrazingTerm(i, s), PerVertexFresnelTerm(i));
        c += BRDF3DirectSimple(s.diffColor, s.specColor, s.smoothness, rl) * attenuatedLightColor;
        c += Emission(i.tex.xy);

        c = tex2D(_MainTex, i.pos.xy).xyz;
        s.alpha = _FadeTime;

        UNITY_APPLY_FOG(i.fogCoord, c);

        return OutputForward(half4(c, 1), s.alpha);
    }



        ENDCG
    }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
    {
        Name "FORWARD_DELTA"
        Tags{ "LightMode" = "ForwardAdd" }
        Blend[_SrcBlend] One
        Fog{ Color(0,0,0,0) } // in additive pass fog should be black
        ZWrite Off
        ZTest LEqual

        CGPROGRAM
#pragma target 3.0

            // -------------------------------------


    #pragma shader_feature _NORMALMAP
    #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    #pragma shader_feature _METALLICGLOSSMAP
    #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
    #pragma shader_feature ___ _DETAIL_MULX2
    #pragma shader_feature _PARALLAXMAP

    #pragma multi_compile_fwdadd_fullshadows
    #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

    #pragma vertex vertAdd
    #pragma fragment fragAdd
    #include "UnityStandardCoreForward.cginc"

            ENDCG
        }
            // ------------------------------------------------------------------
            //  Shadow rendering pass
            Pass{
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
    #pragma target 3.0

            // -------------------------------------


    #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    #pragma shader_feature _METALLICGLOSSMAP
    #pragma shader_feature _PARALLAXMAP
    #pragma multi_compile_shadowcaster
    #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

    #pragma vertex vertShadowCaster
    #pragma fragment fragShadowCaster

    #include "UnityStandardShadow.cginc"

            ENDCG
        }
            // ------------------------------------------------------------------
            //  Deferred pass
            Pass
        {
            Name "DEFERRED"
            Tags{ "LightMode" = "Deferred" }

            CGPROGRAM
    #pragma target 3.0
    #pragma exclude_renderers nomrt


            // -------------------------------------

    #pragma shader_feature _NORMALMAP
    #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    #pragma shader_feature _EMISSION
    #pragma shader_feature _METALLICGLOSSMAP
    #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
    #pragma shader_feature ___ _DETAIL_MULX2
    #pragma shader_feature _PARALLAXMAP

    #pragma multi_compile_prepassfinal
    #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

    #pragma vertex vertDeferred
    #pragma fragment fragDeferred

    #include "UnityStandardCore.cginc"

            ENDCG
        }

            // ------------------------------------------------------------------
            // Extracts information for lightmapping, GI (emission, albedo, ...)
            // This pass it not used during regular rendering.
            Pass
        {
            Name "META"
            Tags{ "LightMode" = "Meta" }

            Cull Off

            CGPROGRAM
    #pragma vertex vert_meta
    #pragma fragment frag_meta

    #pragma shader_feature _EMISSION
    #pragma shader_feature _METALLICGLOSSMAP
    #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    #pragma shader_feature ___ _DETAIL_MULX2
    #pragma shader_feature EDITOR_VISUALIZATION

    #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

        SubShader
        {
            Tags{ "RenderType" = "Opaque" "PerformanceChecks" = "False" }
            LOD 150

            // ------------------------------------------------------------------
            //  Base forward pass (directional light, emission, lightmaps, ...)
            Pass
        {
            Name "FORWARD"
            Tags{ "LightMode" = "ForwardBase" }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]

            CGPROGRAM
    #pragma target 2.0

    #pragma shader_feature _NORMALMAP
    #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    #pragma shader_feature _EMISSION
    #pragma shader_feature _METALLICGLOSSMAP
    #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
    #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            // SM2.0: NOT SUPPORTED shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP

    #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

    #pragma multi_compile_fwdbase
    #pragma multi_compile_fog

    #pragma vertex vertBase
    #pragma fragment fragBase
    #include "UnityStandardCoreForward.cginc"

            ENDCG
        }
            // ------------------------------------------------------------------
            //  Additive forward pass (one light per pass)
            Pass
        {
            Name "FORWARD_DELTA"
            Tags{ "LightMode" = "ForwardAdd" }
            Blend[_SrcBlend] One
            Fog{ Color(0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
    #pragma target 2.0

    #pragma shader_feature _NORMALMAP
    #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    #pragma shader_feature _METALLICGLOSSMAP
    #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
    #pragma shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP
    #pragma skip_variants SHADOWS_SOFT

    #pragma multi_compile_fwdadd_fullshadows
    #pragma multi_compile_fog

    #pragma vertex vertAdd
    #pragma fragment fragAdd
    #include "UnityStandardCoreForward.cginc"

            ENDCG
        }
            // ------------------------------------------------------------------
            //  Shadow rendering pass
            Pass{
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
    #pragma target 2.0

    #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
    #pragma shader_feature _METALLICGLOSSMAP
    #pragma skip_variants SHADOWS_SOFT
    #pragma multi_compile_shadowcaster

    #pragma vertex vertShadowCaster
    #pragma fragment fragShadowCaster

    #include "UnityStandardShadow.cginc"

            ENDCG
        }

            // ------------------------------------------------------------------
            // Extracts information for lightmapping, GI (emission, albedo, ...)
            // This pass it not used during regular rendering.
            Pass
        {
            Name "META"
            Tags{ "LightMode" = "Meta" }

            Cull Off

            CGPROGRAM
    #pragma vertex vert_meta
    #pragma fragment frag_meta

    #pragma shader_feature _EMISSION
    #pragma shader_feature _METALLICGLOSSMAP
    #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
    #pragma shader_feature ___ _DETAIL_MULX2
    #pragma shader_feature EDITOR_VISUALIZATION

    #include "UnityStandardMeta.cginc"
            ENDCG
        }
        }


            FallBack "VertexLit"
            CustomEditor "StandardShaderGUI"
}
