﻿Shader "Custom/LitTeleport" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
        _NoiseColor ("Noise Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
            _FadeTime ("Fade Time", float) = 0
	}
	SubShader {
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		LOD 200
		
        //Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:blend
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
            float3 viewDir;
            float3 worldPos;
            float4 screenPos;
            float3 worldNormal;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
        fixed4 _NoiseColor;
        float _FadeTime;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
            
            float2 adjustedUV = IN.screenPos.xy * 0.0625;
            adjustedUV.x += int(_FadeTime * 100) % 16 * 0.0625;
            adjustedUV.y += int(_FadeTime * 100) / 16 * 0.0625;
            float noiseVal = 7.5 * tex2D(_MainTex, adjustedUV).r;

            float alignment = saturate(dot(normalize(IN.viewDir), IN.worldNormal));
            float alpha = saturate(pow(alignment * (1 - noiseVal), 2) - 2 * _FadeTime + 1);
            o.Alpha = alpha;
            fixed3 noise = fixed3(noiseVal, noiseVal, noiseVal);

            o.Albedo = lerp(c.rgb, noise, _FadeTime);
		}
		ENDCG
	}
	//FallBack "Diffuse"
}