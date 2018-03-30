Shader "Unlit/ForcePulse"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Tex2("Texture2", 2D) = "white" {}
        _Color("Color", Color) = (0,0,0,1)
            _LightColor("Light Color", Color) = (0,0,0,1)
        _XScale("X Scale", float) = 1
            _YScale("Y Scale", float) = 1
        _Fill("Fill", float) = 1
            _Dim("Dimming", float) = 0
            _Duration("Duration", float) = 0
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
            fixed4 _Color;
            fixed4 _LightColor;
            float _XScale;
            float _YScale;
            float _Fill;
            float _Dim;
            float _Duration;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : Color
            {
            float zVal1 = 0.5;// _Time.y * 50 % 256;
            int zFloor1 = floor(zVal1);
            int zCeil1 = ceil(zVal1);
            float zRem1 = zVal1 - zFloor1;

            //reciprocal of size
            float inverseSize = 1.0 / 16;
            float inversePixels = 1.0 / 4096.0;
            float2 scaledUV = i.uv;
            scaledUV.x *= _XScale;
            scaledUV.x = fmod(scaledUV.x, 1);
            scaledUV.y *= _YScale;
            scaledUV.y = fmod(scaledUV.y, 1);
            scaledUV *= inverseSize;
            float2 nextPixel = float2(inversePixels, inversePixels);
            float xRem = ((scaledUV.x * 4096) - floor(scaledUV.x * 4096)) + 0.5;
            if (xRem < 0.5)
            {
                nextPixel.x *= -1;
                xRem = 0.5 - xRem;
            }
            else
            {
                xRem -= 0.5;
            }

            float yRem = ((scaledUV.y * 4096) - floor(scaledUV.y * 4096));
            if (yRem < 0.5)
            {
                nextPixel.y *= -1;
                yRem = 0.5 - yRem;
            }
            else
            {
                yRem -= 0.5;
            }

            //integer division in y component
            float2 uvOffsetFloor = float2(fmod(zFloor1, 16) * inverseSize, zFloor1 / 16 * inverseSize);
            float2 uvOffsetCeil = float2(fmod(zCeil1, 16) * inverseSize, zCeil1 / 16 * inverseSize);

            // sample the texture
            float2 offsetBL = fmod(scaledUV, inverseSize);
            float texValueBL = (tex2D(_MainTex, uvOffsetFloor + offsetBL)).r;

            float2 offsetBR = fmod(scaledUV + float2(nextPixel.x, 0), inverseSize);
            float texValueBR = (tex2D(_MainTex, uvOffsetFloor + offsetBR)).r;

            float2 offsetTL = fmod(scaledUV + float2(0, nextPixel.y), inverseSize);
            float texValueTL = (tex2D(_MainTex, uvOffsetFloor + offsetTL)).r;

            float2 offsetTR = fmod(scaledUV + nextPixel, inverseSize);
            float texValueTR = (tex2D(_MainTex, uvOffsetFloor + offsetTR)).r;

            float texValueB = lerp(texValueBL, texValueBR, abs(xRem));
            float texValueT = lerp(texValueTL, texValueTR, abs(xRem));
            float texValue = lerp(texValueB, texValueT, abs(yRem));
            texValue *= 7.5;
            texValue = 1 - texValue;
            texValue = saturate(pow(texValue, _Fill) - _Dim);

            float outsidePos = abs(0.5 - i.uv.x) * 2;
            float fade = saturate(cos(1.5708 * outsidePos - _Duration) + step(_Duration, outsidePos));
            texValue *= fade;

            float4 col = lerp(_LightColor, _Color, saturate(texValue));
            //float minCol = _Fill;
            //float difference = texValue - _Dim + 0.5;
            //fixed4 col = difference * _LightColor + (1 - difference) * _Color;
            col.a = texValue;
            //col.a = step(minCol, texValue) * saturate(texValue / _Fill);
            return col;

            }
            ENDCG
        }
        }
}
