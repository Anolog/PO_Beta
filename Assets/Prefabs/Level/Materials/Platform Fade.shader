Shader "Transparent/Platform Fade"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _FadeDepth("Fade Depth", float) = 5.0
        _MinOpacity("Minimum Opacity", Range(0, 1)) = 0.5
        _FadeWidth("Fade Width", Range(1, 2)) = 1
    }
    SubShader
    {
        //Tags {"RenderType" = "Opaque" }
         Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}
         LOD 200

         Pass{

             Blend SrcAlpha OneMinusSrcAlpha

             CGPROGRAM

             #pragma vertex vert
             #pragma fragment frag

             #include "UnityCG.cginc"
             //additional lighting functionality
             #include "UnityLightingCommon.cginc"

             struct v2f
             {
                 float4 pos : SV_POSITION;
                 float2 depth : TEXCOORD1;
                 float4 screenPos : TEXCOORD2;
                 float2 uv : TEXCOORD0;
                 fixed4 lightColor : COLOR0;
             };
             
             struct appdata
             {
                 float4 vertex : POSITION; // vertex position
                 float2 uv : TEXCOORD0; // texture coordinate
                 float3 normal : NORMAL; //surface normal
             };
                
             float _FadeDepth;
             float _MinOpacity;
             float _FadeWidth;

             sampler2D _MainTex;

             v2f vert(appdata v)
             {
                 v2f o;
                 o.pos = UnityObjectToClipPos(v.vertex);
                 COMPUTE_EYEDEPTH(o.depth);
                 o.screenPos = ComputeScreenPos(o.pos);

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
                 
                 o.uv = v.uv;

                 return o;
             }

             half4 frag(v2f i) : SV_Target
             {
             	 //depth of geometry from camera
                 float depth = i.depth.x;

                 //determine opacity based on screen position, centre is most transparent
                 fixed2 screenPos = fixed2(i.screenPos.x, i.screenPos.y) / i.screenPos.w;
                 fixed2 distFromScreenCentre = abs(screenPos - fixed2(0.5f, 0.5f));
                 float screenSpaceOpacityFactor = saturate(length(distFromScreenCentre));

                 //determine opacity based on depth, smallest depth is most transparent
                 float k = 1 / pow(_FadeDepth * _FadeWidth, 6);
                 //all geometry less than fade depth will receive an alpha value in 0-1 range
                 float depthOpacityFactor = saturate(k * pow(depth, 6));

                 //combine depth and screen position opacities
                 float combinedOpacityFactor = (1 - (1 - depthOpacityFactor) * (1 - screenSpaceOpacityFactor)) * (1 - _MinOpacity);

                 //add the minimum opacity value for the effect
                 float opacity = _MinOpacity + combinedOpacityFactor;

                 //multiple texture color by light

                 fixed4 finalColor = tex2D(_MainTex, i.uv) * i.lightColor;
                 finalColor.a = opacity;

                 return finalColor;
             }

             ENDCG
         }
    }
}