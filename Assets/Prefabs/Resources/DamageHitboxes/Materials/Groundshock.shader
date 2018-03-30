﻿Shader "Unlit/Groundshock"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    _Resolution("Resolution", Range(1, 100)) = 8
        _Octaves("Octaves", Range(1,8)) = 1
        _Lucanarity("Lucanarity", Range(1, 4)) = 1.67
        _Persistence("Persistence", Range(0, 1)) = 0.5
        _Speed("Speed", Float) = 1
        _OctaveSpeed("Octave Speed", Float) = 1

        _XScale("X Scale", float) = 1
        _YScale("Y Scale", float) = 1
        _Fill("Fill", float) = 1
        _Dim("Dimming", float) = 0
        _Duration("Duration", float) = 0

        _WorldDirY("World Direction Y", float) = 0

        _Color("Color", Color) = (0,0,0,1)
        _LightColor("Light Color", Color) = (0,0,0,1)

    }
        SubShader
    {
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100

        Pass
    {
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        //Cull Off

        CGPROGRAM

#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

        struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
        float3 normal : NORMAL;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float3 normal : TEXCOORD1;
        half3 cameraDirection : TEXCOORD2;
        float4 vertex : SV_POSITION;
    };

    sampler2D _MainTex;
    float4 _MainTex_ST;
    int _Resolution;
    int _Octaves;
    float _Lucanarity;
    float _Persistence;
    float _Speed;
    float _OctaveSpeed;

    float _XScale;
    float _YScale;
    float _Fill;
    float _Dim;
    float _Duration;

    float _WorldDirY;

    fixed4 _Color;
    fixed4 _LightColor;

    static float3 gradients[16] = {
        float3(1, 1, 0),
        float3(-1, 1, 0),
        float3(1,-1, 0),
        float3(-1,-1, 0),
        float3(1, 0, 1),
        float3(-1, 0, 1),
        float3(1, 0,-1),
        float3(-1, 0,-1),
        float3(0, 1, 1),
        float3(0,-1, 1),
        float3(0, 1,-1),
        float3(0,-1,-1),
        float3(1, 1, 0),
        float3(-1, 1, 0),
        float3(0,-1, 1),
        float3(0,-1,-1)
    };

    static int hash[16] = { 12,	4, 11, 6, 3, 15, 9, 7, 0, 5, 14, 13, 8,	10,	2, 1 };

    /*static int hash[48] = {
    5,
    12,
    7,
    40,
    18,
    45,
    39,
    33,
    46,
    41,
    20,
    17,
    38,
    27,
    14,
    37,
    23,
    24,
    0,
    15,
    22,
    29,
    35,
    2,
    21,
    42,
    11,
    25,
    36,
    44,
    9,
    34,
    4,
    43,
    10,
    8,
    26,
    47,
    1,
    16,
    6,
    3,
    28,
    13,
    31,
    30,
    32,
    19,
    };*/

    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        o.cameraDirection = normalize(UnityWorldSpaceViewDir(worldPos));
        o.normal = UnityObjectToWorldNormal(v.normal);
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        float col = 0;
    float totalStrength = 0;

    uint hashSize = 15;

    //i.uv.x = abs(0.5 - i.uv.x);
    float outsidePos = min(abs(1 - i.uv.x), abs(i.uv.x)) * 2;
    float fade = saturate(cos(0.5 * (1.5708 * outsidePos - 0.5 * _Duration)) + step(0.5 * _Duration, outsidePos));
    _XScale = 0.5 + 4 * (_Duration);
    //_XScale -= 1;

    i.uv.x = 0.7 * (1 - i.uv.x) + 0.15;

    for (int j = 0; j < _Octaves; j++)
    {
        //return fixed4(i.uv.x, i.uv.x, i.uv.x, 1);
        //i.uv.x = fmod(i.uv.x, 1);
        float2 scaledUV = i.uv * _Resolution * pow(_Lucanarity, j);
        //scaledUV.x *= _XScale;
        float zVal = _Time.y * _Speed;

        uint xFloor = floor(scaledUV.x / _XScale);
        uint yFloor = floor(scaledUV.y / _YScale);
        uint zFloor = floor(zVal);

        float xRemainder = scaledUV.x / _XScale - xFloor;
        float xRemMinusOne = xRemainder - 1;
        float yRemainder = scaledUV.y / _YScale - yFloor;
        float yRemMinusOne = yRemainder - 1;
        float zRemainder = zVal - zFloor;
        float zRemMinusOne = zRemainder - 1;

        float xInterp = smoothstep(0, 1, xRemainder);
        float yInterp = smoothstep(0, 1, yRemainder);
        float zInterp = smoothstep(0, 1, zRemainder);
        //smootherstep
        //interpolationPoint = 6 * pow(heightAboveFloor, 5) - 15 * pow(heightAboveFloor, 4) + 10 * pow(heightAboveFloor, 3);
        //smootheststep
        //interpolationPoint = -20 * pow(heightAboveFloor, 7) + 70 * pow(heightAboveFloor, 6) - 84 * pow(heightAboveFloor, 5) + 35 * pow(heightAboveFloor, 4);

        uint leftHash = hash[xFloor & hashSize];
        uint rightHash = hash[(xFloor + 1) & hashSize];

        uint bottomLeftHash = hash[(leftHash + yFloor) & hashSize];
        uint bottomRightHash = hash[(rightHash + yFloor) & hashSize];
        uint topLeftHash = hash[(leftHash + yFloor + 1) & hashSize];
        uint topRightHash = hash[(rightHash + yFloor + 1) & hashSize];

        fixed3 frontBottomLeftGrad = gradients[hash[(bottomLeftHash + zFloor) & hashSize]];
        fixed3 frontBottomRightGrad = gradients[hash[(bottomRightHash + zFloor) & hashSize]];
        fixed3 frontTopLeftGrad = gradients[hash[(topLeftHash + zFloor) & hashSize]];
        fixed3 frontTopRightGrad = gradients[hash[(topRightHash + zFloor) & hashSize]];
        fixed3 backBottomLeftGrad = gradients[hash[(bottomLeftHash + zFloor + 1) & hashSize]];
        fixed3 backBottomRightGrad = gradients[hash[(bottomRightHash + zFloor + 1) & hashSize]];
        fixed3 backTopLeftGrad = gradients[hash[(topLeftHash + zFloor + 1) & hashSize]];
        fixed3 backTopRightGrad = gradients[hash[(topRightHash + zFloor + 1) & hashSize]];

        float frontBottomLeftVal = dot(frontBottomLeftGrad, fixed3(xRemainder, yRemainder, zRemainder));
        float frontBottomRightVal = dot(frontBottomRightGrad, fixed3(xRemMinusOne, yRemainder, zRemainder));
        float frontTopLeftVal = dot(frontTopLeftGrad, fixed3(xRemainder, yRemMinusOne, zRemainder));
        float frontTopRightVal = dot(frontTopRightGrad, fixed3(xRemMinusOne, yRemMinusOne, zRemainder));
        float backBottomLeftVal = dot(backBottomLeftGrad, fixed3(xRemainder, yRemainder, zRemMinusOne));
        float backBottomRightVal = dot(backBottomRightGrad, fixed3(xRemMinusOne, yRemainder, zRemMinusOne));
        float backTopLeftVal = dot(backTopLeftGrad, fixed3(xRemainder, yRemMinusOne, zRemMinusOne));
        float backTopRightVal = dot(backTopRightGrad, fixed3(xRemMinusOne, yRemMinusOne, zRemMinusOne));

        //linear interpolation
        float color = lerp(
            lerp(lerp(frontBottomLeftVal, frontBottomRightVal, xInterp), lerp(frontTopLeftVal, frontTopRightVal, xInterp), yInterp),
            lerp(lerp(backBottomLeftVal, backBottomRightVal, xInterp), lerp(backTopLeftVal, backTopRightVal, xInterp), yInterp),
            zInterp);


        float strength = pow(_Persistence, j + 1);
        totalStrength += strength;
        col += color * strength;
    }



    col /= totalStrength;
    col = col * 0.5 + 0.5;

    col *= saturate(pow(sin(3.1415 * i.uv.x), 0.5));
    col *= saturate(3 * sin(3.1415 * i.uv.y));
    //col *= pow(1 - saturate(abs(i.normal.y - i.cameraDirection.y)), 1);

    col = abs(0.5 - col);
    col = 1 - col;

    col = saturate(pow(col, _Fill) - (1 - pow(1 - _Duration, 0.3)));
    col = 0.8 * col + 0.1;

    col *= fade;

    float4 color = lerp(_LightColor, _Color, 2 * col);// -((pow(fade, 0.2) - 1))));
                                                      //float minCol = _Fill;
                                                      //float difference = texValue - _Dim + 0.5;
                                                      //fixed4 col = difference * _LightColor + (1 - difference) * _Color;
    color.a = col;
    //col.a = step(minCol, texValue) * saturate(texValue / _Fill);
    return color;

    }
        ENDCG
    }
    }
}