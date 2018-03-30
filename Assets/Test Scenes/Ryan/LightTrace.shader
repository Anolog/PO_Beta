Shader "Unlit/LightTrace"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Tex2("Texture 2", 2D) = "white" {}
        _Tex3("Texture 3", 2D) = "white" {}
        _Tex4("Texture 4", 2D) = "white" {}
        _BrightColor("Light Color", Color) = (0,0,0,1)
        _ColorUp("Color Boost", float) = 1
        _GlowSize("Glow Size", float) = 10
        _GlowPos("Glow Position", Range(0.5, 1)) = 0.9
        _GlowWidth("Glow Width", float) = 1
        _TraceSize("Trace Size", float) = 1
        _TraceStrength("Trace Strength", Range(0, 1)) = 0.25
    }
        SubShader
        {
            Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
            LOD 100

            Pass
            {
                Cull Off
                ZWrite Off
                Blend SrcAlpha OneMinusSrcAlpha

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
                sampler2D _Tex2;
                sampler2D _Tex3;
                sampler2D _Tex4;
                fixed4 _BrightColor;
                float _ColorUp;
                float _GlowSize;
                float _GlowPos;
                float _GlowWidth;
                float _TraceSize;
                float _TraceStrength;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float texValue = tex2D(_MainTex, i.uv).r;

                    float rotation = fmod(_Time.y * 1.2, 1);

                    float difference = i.uv.y - rotation;
                    int overlap = step(difference, 0);
                    difference += overlap;
                    difference = saturate(difference);
                    float colorVal = 7.5 * sin(difference * 3.141592654) * pow(difference, 8);

                    float colorAdd = saturate(-_GlowSize * pow(abs((difference - _GlowPos) * _GlowWidth), 2) + _ColorUp);
                    float colorAdd2 = pow(texValue, _TraceSize) * _TraceStrength;
                    colorAdd += colorAdd2;

                    // sample the texture
                    fixed4 col = _BrightColor + fixed4(colorAdd, colorAdd, colorAdd, 0);
                    col.a = texValue * colorVal;

                    float texValue2 = tex2D(_Tex2, i.uv).r;
                    float rotation2 = 1 -fmod(_Time.y * 0.75, 1);

                    float difference2 = i.uv.y - rotation2;
                    int overlap2 = step(difference2, 0);
                    difference2 += overlap2;
                    difference2 = 1 - saturate(difference2);
                    float colorVal2 = 7.5 * sin(difference2 * 3.141592654) * pow(difference2, 8);

                    float colorAdd21 = saturate(-_GlowSize * pow(abs((difference2 - _GlowPos) * _GlowWidth), 2) + _ColorUp);
                    float colorAdd22 = pow(texValue2, _TraceSize) * _TraceStrength;
                    colorAdd22 += colorAdd21;

                    // sample the texture
                    fixed4 col2 = _BrightColor + fixed4(colorAdd21, colorAdd21, colorAdd21, 0);
                    col2.a = texValue2 * colorVal2;

                    float texValue3 = tex2D(_Tex3, i.uv).r;
                    float rotation3 = 1 - fmod(_Time.y * 1.05, 1);

                    float difference3 = i.uv.y - rotation3;
                    int overlap3 = step(difference3, 0);
                    difference3 += overlap3;
                    difference3 = 1 - saturate(difference3);
                    float colorVal3 = 7.5 * sin(difference3 * 3.141592654) * pow(difference3, 8);

                    float colorAdd31 = saturate(-_GlowSize * pow(abs((difference3 - _GlowPos) * _GlowWidth), 2) + _ColorUp);
                    float colorAdd32 = pow(texValue3, _TraceSize) * _TraceStrength;
                    colorAdd32 += colorAdd31;

                    // sample the texture
                    fixed4 col3 = _BrightColor + fixed4(colorAdd31, colorAdd31, colorAdd31, 0);
                    col3.a = texValue3 * colorVal3;

                    float texValue4 = tex2D(_Tex4, i.uv).r;
                    float rotation4 = fmod(_Time.y * 0.65, 1);

                    float difference4 = i.uv.y - rotation4;
                    int overlap4 = step(difference4, 0);
                    difference4 += overlap4;
                    difference4 = saturate(difference4);
                    float colorVal4 = 7.5 * sin(difference4 * 3.141592654) * pow(difference4, 8);

                    float colorAdd41 = saturate(-_GlowSize * pow(abs((difference4 - _GlowPos) * _GlowWidth), 2) + _ColorUp);
                    float colorAdd42 = pow(texValue4, _TraceSize) * _TraceStrength;
                    colorAdd42 += colorAdd41;

                    // sample the texture
                    fixed4 col4 = _BrightColor + fixed4(colorAdd41, colorAdd41, colorAdd41, 0);
                    col4.a = texValue4 * colorVal4;
                    
                    fixed4 finalCol;
                    finalCol.rgb = col.rgb * col.a + col2.rgb * col2.a + col3.rgb * col3.a + col4.rgb * col4.a;
                    finalCol.a = col.a + col2.a + col3.a + col4.a;

                    return finalCol;
                }
                ENDCG
            }
        }
}
