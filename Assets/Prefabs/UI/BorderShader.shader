Shader "Transparent/BorderShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HealthHitColor("Health Hit Color", Color) = (1, 0, 0, 1)
		_ShieldHitAlpha("Shield Hit Alpha Amount", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
		LOD 100

		Pass
		{
			ZWrite Off
			ZTest Always

			Blend SrcALpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _HealthHitColor;
			float _ShieldHitAlpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
	
				col.a = _ShieldHitAlpha;


				return col;
			}
			ENDCG
		}
	}
}
