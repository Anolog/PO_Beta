Shader "Unlit/Platform Opaque"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
        {
            Tags{ "LightMode" = "ForwardBase" }


            Pass{

            CGPROGRAM

    #pragma vertex vert
    #pragma fragment frag
    #pragma multi_compile_fwdbase// nolightmap nodirlightmap nodynlightmap novertexlight

    #include "UnityCG.cginc"
            //additional lighting functionality
    #include "UnityLightingCommon.cginc"
    #include "AutoLight.cginc"


            struct v2f
        {
            float4 pos : SV_POSITION;
            fixed4 lightColor : COLOR0;
            float2 uv : TEXCOORD0;
            SHADOW_COORDS(1)
        };

        struct appdata
        {
            float4 vertex : POSITION; // vertex position
            float2 uv : TEXCOORD0; // texture coordinate
            float3 normal : NORMAL; //surface normal
        };

        sampler2D _MainTex;

        v2f vert(appdata v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);

            //diffuse lighting
            half3 surfaceNormal = UnityObjectToWorldNormal(v.normal);
            //calcualte light intesity on the surface
            half lightIntensity = max(0, dot(surfaceNormal, _WorldSpaceLightPos0.xyz));
            //calculate light color
            fixed4 diffLightColor = lightIntensity * _LightColor0;

            //built in function for ambient light
            fixed4 ambientLight = unity_AmbientSky;// ShadeSH9(half4(surfaceNormal, 1));

                                                   //total light
            o.lightColor = ambientLight + diffLightColor;

            TRANSFER_SHADOW(o)

            o.uv = v.uv;

            return o;
        }

        half4 frag(v2f i) : SV_Target
        {
            fixed shadow = SHADOW_ATTENUATION(i);
            
            //multiply texture color by light and shadow
            fixed4 finalColor = tex2D(_MainTex, i.uv) * i.lightColor * shadow;
            
            return finalColor;
        }

            ENDCG
        }
            UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
        }
}