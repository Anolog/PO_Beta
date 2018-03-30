Shader "Unlit/3DTextureSampling"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _PerlinStr("Perlin Strength", Range(0, 1)) = 0.5
        _PerlinSpeed("WorleySpeed", float) = 1
        _PerlinDarkCol("Perlin Dark Color", Color) = (0,0,0,1)
        _PerlinLightCol("Perlin Light Color", Color) = (0,0,0,1)

            _Resolution("Resolution", Range(1, 50)) = 8
            _Octaves("Octaves", Range(1,8)) = 1
            //needs to be 1,2,4 or 8 to avoid seams, 2 looks the best
            _Lucanarity("Lucanarity", Range(2, 2)) = 2
            _Persistence("Persistence", Range(0, 1)) = 0.5

        _WorleyTex("Worley Tex", 2D) = "white" {}
        _WorleyStr("Worley Strength", float) = 0.5
        _WorleySpeed ("WorleySpeed", float) = 1
        _WorleyCol("Worley Color", Color) = (0,0,0,1)
        _WorleyExp("Worley Exponent", float) = 2
        _WorleyMul("Worley Multiplier", float) = 1
        
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
            #pragma target 3.5

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
            float _PerlinStr;
            float _PerlinSpeed;
            fixed4 _PerlinDarkCol;
            fixed4 _PerlinLightCol;

            int _Resolution;
            int _Octaves;
            float _Lucanarity;
            float _Persistence;

            sampler2D _WorleyTex;
            float4 _WorleyTex_ST;
            float _WorleyStr;
            float _WorleySpeed;
            fixed4 _WorleyCol;
            float _WorleyExp;
            float _WorleyMul;

            //array of gradients for use by perlin effect
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

            //hash array for perlin effect
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

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{

				//Worley component

				//mod 256 to keep in range of 256x256x256 procedural 3D texture
                float zVal1 = _Time.y * _WorleySpeed % 256;
                uint zFloor1 = abs(floor(zVal1));
                uint zCeil1 = abs(ceil(zVal1));
                float zRem1 = zVal1 - zFloor1;

                //reciprocal of size
                float inverseSize = 1.0 / 16;
                float2 scaledUV = i.uv * inverseSize;

                //integer division in y component
                float2 uvOffsetFloor = float2(zFloor1 % 16 * inverseSize, zFloor1 / 16 * inverseSize);
                float2 uvOffsetCeil = float2(zCeil1 % 16 * inverseSize, zCeil1 / 16 * inverseSize);

                // sample the texture
                float strW = pow(saturate(tex2D(_WorleyTex, scaledUV + uvOffsetFloor) * _WorleyMul), _WorleyExp).r;
                //final Worley component value
                fixed4 colW = strW * _WorleyCol;

                //Polar perlin component

                //accumulated vales across octoaves of perlin noise
                float col = 0;
                float totalStrength = 0;

                //bitmask to ensure we never go out of bounds in the hash array
                uint hashSize = 15;

                for (int j = 0; j < _Octaves; j++)
                {
                    float2 scaledUV = (i.uv - float2(0.5f, 0.5f)) * _Resolution * pow(_Lucanarity, j);

                    //convert rectangular x and y coordinates to r and theta
                    //they will still be referred to as x and y throughout the rest of the code
                    float rVal = length(i.uv - float2(0.5f, 0.5f)) * _Resolution;
                    float thetaVal = atan2(scaledUV.y, scaledUV.x) + 3.141592654;
                    thetaVal *= pow(_Lucanarity, j) * 8 / 3.141592654f;

                    float zVal = _Time.y * _PerlinSpeed;

                    uint xFloor = floor(rVal);
                    uint yFloor = floor(thetaVal);
                    uint zFloor = floor(zVal);

                    float xRemainder = rVal - xFloor;
                    float xRemMinusOne = xRemainder - 1;
                    float yRemainder = thetaVal - yFloor;
                    float yRemMinusOne = yRemainder - 1;
                    float zRemainder = zVal - zFloor;
                    float zRemMinusOne = zRemainder - 1;

                    //interpolation values for each perlin direction
                    float xInterp = smoothstep(0, 1, xRemainder);
                    float yInterp = smoothstep(0, 1, yRemainder);
                    float zInterp = smoothstep(0, 1, zRemainder);

           			//r direction hash
                    uint leftHash = hash[xFloor & hashSize];
                    uint rightHash = hash[(xFloor + 1) & hashSize];

                    //theta direction hash
                    uint bottomLeftHash = hash[(leftHash + yFloor) & hashSize];
                    uint bottomRightHash = hash[(rightHash + yFloor) & hashSize];
                    uint topLeftHash = hash[(leftHash + yFloor + 1) & hashSize];
                    uint topRightHash = hash[(rightHash + yFloor + 1) & hashSize];

                    //z direction hash
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

                    //linear interpolation of 8 hash values
                    float color = lerp(
                        lerp(lerp(frontBottomLeftVal, frontBottomRightVal, xInterp), lerp(frontTopLeftVal, frontTopRightVal, xInterp), yInterp),
                        lerp(lerp(backBottomLeftVal, backBottomRightVal, xInterp), lerp(backTopLeftVal, backTopRightVal, xInterp), yInterp),
                        zInterp);

					//accumulate strengths
                    float strength = pow(_Persistence, j + 1);
                    totalStrength += strength;
                    col += color * strength;
                }

                //divide by total accumulated strength
                col /= totalStrength;
                //transform to 0-1 range
                col = col / sqrt(3) + 0.5f;
                //final perlin value
                float strP = col;
                //transform to selected colors
                fixed4 colP = (strP * _PerlinStr + 0.5 * (1 - _PerlinStr)) * _PerlinLightCol + ((1 - strP) * _PerlinStr + 0.5 * _PerlinStr) * _PerlinDarkCol;

                //combine to make final color
                fixed4 colF = colP + colW * _WorleyStr;
				return colF;
			}
			ENDCG
		}
	}
}
