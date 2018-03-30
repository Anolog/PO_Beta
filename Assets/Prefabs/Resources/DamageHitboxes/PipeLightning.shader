Shader "Unlit/PipeLightning"
{
    Properties
    {
        _Resolution("Resolution", Range(1, 50)) = 8
        _Octaves("Octaves", Range(1,8)) = 1
        _Lucanarity("Lucanarity", Range(1, 4)) = 1.67
        _Persistence("Persistence", Range(0, 1)) = 0.5
        _Speed("Speed", Float) = 1
        _PulseSpeed("Pulse Speed", Float) = 1
        _GradientSlope("Gradient Slope", Range(0.35, 2)) = 0.5
        _Grey("Grey Level", Range(0.9, 1.0)) = 0.95
        _Black("Black Level", Range(0.75, 1.0)) = 0.8

        _WhiteColor("Bright Color", Color) = (0,0,0,1)
        _GreyColor("Midtone Color", Color) = (0,0,0,1)
        _BlackColor("Dark Color", Color) = (0,0,0,1)

        _Scale("Scale", Float) = 1
    }

        SubShader
    {
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100

        Pass
    {
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM

#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"



        struct appdata
    {
        float3 normal : NORMAL;
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {

        float2 uv : TEXCOORD0;
        float3 normal : TEXCOORD1;
        half3 cameraDirection : TEXCOORD2;
        float4 vertex : SV_POSITION;
    };

    int _Resolution;
    int _Octaves;
    float _Lucanarity;
    float _Persistence;
    float _Speed;
    float _PulseSpeed;
	float _GradientSlope;
    float _Grey;
    float _Black;

    fixed4 _WhiteColor;
    fixed4 _GreyColor;
    fixed4 _BlackColor;

    float _Scale;

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

    static int hash[16] = {
        12,
        4,
        11,
        6,
        3,
        15,
        9,
        7,
        0,
        5,
        14,
        13,
        8,
        10,
        2,
        1,
    };

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

    for (int j = 0; j < _Octaves; j++)
    {
        float2 scaledUV = i.uv * _Resolution * pow(_Lucanarity, j);
        scaledUV.y *= _Scale;
        scaledUV.y -= _Time.y * _Speed;
        scaledUV.y = fmod(scaledUV.y, -16);
        scaledUV.y += 16;

        float zVal = _Time.y * _PulseSpeed;

        uint xFloor = floor(scaledUV.x);
        uint yFloor = floor(scaledUV.y);
        uint zFloor = floor(zVal);

        float xRemainder = scaledUV.x - xFloor;
        float xRemMinusOne = xRemainder - 1;
        float yRemainder = scaledUV.y - yFloor;
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

        uint leftHash = hash[xFloor & 1];
        uint rightHash = hash[(xFloor + 1) & 1];

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

	//lightning adjustments

	float gradientVal = _GradientSlope * (i.uv.x - 0.5) + 0.5;

    col = abs(gradientVal - col);
    col = 1 - col;

    float yFactor = saturate(-1 * pow(2 * i.uv.y - 1, 6) + 1);
    float xFactor = -2 * pow(i.uv.x - 0.5, 2) + 0.5 * (1 - yFactor);
    col *= yFactor + xFactor;
    //return fixed4(yFactor + xFactor, yFactor + xFactor, yFactor + xFactor, 1);
    //float align = dot(normalize(i.normal), normalize(i.cameraDirection));
    //float alpha = i.uv.x * i.uv.x;

    int white = step(_Grey, col);
    int grey = step(max(_Black, white), col);

    float whiteVal = col - _Grey;
    float whiteGreyDiff = 1 - _Grey;
    float whiteCol = whiteVal / whiteGreyDiff;

    fixed4 lightColor = lerp(_GreyColor, _WhiteColor, whiteCol);

    float greyVal = col - _Black;
    float greyBlackDiff = _Grey - _Black;
    float greyCol = greyVal / greyBlackDiff;

    fixed4 midColor = lerp(_BlackColor, _GreyColor, greyCol);
     
    fixed4 adjustedColor = white * lightColor + grey * midColor;
    float alpha = white * col + grey * greyCol * 0.5;
    adjustedColor.a = alpha;
    //col = i.cameraDirection;
    return adjustedColor;

    }
        ENDCG
    }
    }
}
