Shader "Unlit/TeleportFade"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (0,0,0,0)
    }
        SubShader
    {
            Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }


        Pass{

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM

#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

        struct v2f
    {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    struct appdata
    {
        float4 vertex : POSITION; // vertex position
        float2 uv : TEXCOORD0; // texture coordinate
        float3 normal : NORMAL; //surface normal
    };

    sampler2D _MainTex;
    fixed4 _Color;

    static int hash1[20] = {
        11,
        19,
        13,
        9,
        2,
        0,
        12,
        1,
        3,
        14,
        10,
        6,
        15,
        17,
        18,
        16,
        4,
        7,
        8,
        5
    };

    static int hash2[20] = {
        13,
        10,
        9,
        11,
        1,
        5,
        4,
        2,
        15,
        3,
        0,
        19,
        7,
        14,
        18,
        6,
        17,
        16,
        12,
        8
    };

    v2f vert(appdata v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;

        return o;
    }

    half4 frag(v2f i) : SV_Target
    {
    
    uint hashSize = 19;
    float inverseHashSize = 1.0 / 19.0;
    fixed4 finalColor = _Color;

    float hash = hash2[(uint)i.pos.x & hashSize] / 19.0;

    finalColor.a = saturate(hash);

    return finalColor;
    }

        ENDCG
    }
    }
}