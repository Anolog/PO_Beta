// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Energy"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		//Parameters for 1D component
		_Resolution1D("Resolution 1D", Int) = 16
		[IntRange] _Octaves1D("Octaves 1D", Range(1, 10)) = 1
		_Lucanarity1D("Lucanarity 1D", Range(2, 2)) = 2
		_Persistence1D("Persistence 1D", Range(0, 1)) = 0.5
		//The speed at which the first octoave will move
		_Speed1D("Speed 1D", Range(0, 4)) = 0.25
		//The speed at which subsequent octaves will move relative to the previous octave
		_OctaveSpeed1D("Octave Speed Scale 1D", Range(0,3)) = 2
		_LightColor1D("Light Color 1D", Color) = (0,0,0,1)
		_DarkColor1D("Dark Color 1D", Color) = (0,0,0,1)

		//Parameters for 3D component
		//Lock resolution to 8 for 3D perlin effect
		_Resolution3D("Resolution 3D", Range(8,8)) = 8
		[IntRange] _Octaves3D("Octaves 3D", Range(1, 10)) = 1
		//Lock lucanarity to 2 so the texture repeats properly and doesn't cause seams
		_Lucanarity3D("Lucanarity 3D", Range(2, 2)) = 2
		_Persistence3D("Persistence 3D", Range(0, 1)) = 0.5
		//The speed at which the first octoave will move
		_Speed3D("Speed 3D", Range(0, 4)) = 0.25
		//The speed at which subsequent octaves will move relative to the previous octave
		_OctaveSpeed3D("Octave Speed Scale 3D", Range(0,3)) = 2
		_LightColor3D("Light Color 3D", Color) = (0,0,0,1)
		_DarkColor3D("Dark Color 3D", Color) = (0,0,0,1)

		//How much of each component to use
		_Component3D("3D Strength", Range(0, 2)) = 0.5
		_Component1D("1D Strendth", Range(0,2)) = 0.5
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

            #pragma target 3.5

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

			sampler2D _MainTex;
			float4 _MainTex_ST;

			int _Resolution1D;
			int _Octaves1D;
			float _Lucanarity1D;
			float _Persistence1D;
            float _Speed1D;
            float _OctaveSpeed1D;
            fixed4 _LightColor1D;
            fixed4 _DarkColor1D;

            //Gradients for use with the 1D effect
			static int gradients1D[2] = { 1, -1 };
			//Hash mask for 1D gradients
			static int gradientSize1D = 1;
			//Hask mask for 1D hash array
			static int hash1D[8] = { 6, 7, 3, 1, 0, 4, 2, 5 };
			static int hashSize1D = 7;
			
			int _Resolution3D;
			int _Octaves3D;
			float _Lucanarity3D;
			float _Persistence3D;
			float _Speed3D;
			float _OctaveSpeed3D;
			fixed4 _LightColor3D;
			fixed4 _DarkColor3D;

			float _Component3D;
			float _Component1D;

			//Gradients for 3D effect
			static float3 gradients3D[16] = {
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
			//3D gradients hash mask
			static uint gradientSize3D = 15;
			//3D hash array
			static int hash3D[16] = {	12,	4, 11, 6, 3, 15, 9, 7, 0, 5, 14, 13, 8,	10,	2, 1 };
			//Hash mask for 3D hash array
			static uint hashSize3D = 15;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.cameraDirection = normalize(UnityWorldSpaceViewDir(worldPos));
				o.normal = UnityObjectToWorldNormal(v.normal);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

				
				fixed4 col1D = fixed4(0, 0, 0, 1);
				
				//1D component
				{
					float totalStrength1D = 0;

					for (int j = 0; j < _Octaves1D; j++)
					{
						float2 scaledUV = i.uv * _Resolution1D * pow(_Lucanarity1D, j);

						//effect is moving vertically, so we use y, modify speed based on octave
						scaledUV.y += _Time.y * _Speed1D * pow(_OctaveSpeed1D, j);

						uint yFloor = abs(floor(scaledUV.y));

						float yRemainder = scaledUV.y - yFloor;
						float yRemMinusOne = yRemainder - 1;
						float yInterp = smoothstep(0, 1, yRemainder);

						uint leftHash = hash1D[yFloor & hashSize1D];
						uint rightHash = hash1D[(yFloor + 1) & hashSize1D];

						int leftGrad = gradients1D[leftHash & gradientSize1D];
						int rightGrad = gradients1D[rightHash & gradientSize1D];

						//interpolate individual gradients
						float leftVal = leftGrad * yRemainder;
						float rightVal = rightGrad * yRemMinusOne;

						//linear interpolation
						float colour = lerp(leftVal, rightVal, yInterp);

						//accumlate strtength across octaves
						float strength = pow(_Persistence1D, j + 1);
						totalStrength1D += strength;
						col1D += fixed4(colour, colour, colour, 0) * strength;
					}

					//divide by total strength to get normalized value
					col1D /= totalStrength1D;
					//map values to 0-1 and translate to chosen colours
					float darkColStr1D = col1D * 0.5 + 0.5;
					float lightColStr1D = 1 - darkColStr1D;	

					//final ID component
					col1D = _LightColor1D * lightColStr1D + _DarkColor1D * darkColStr1D;
				}

				fixed4 col3D = fixed4(0, 0, 0, 1);

				//3D component
				{
					float totalStrength3D = 0;

					for (int j = 0; j < _Octaves3D; j++)
					{
						float2 scaledUV = i.uv * _Resolution3D * pow(_Lucanarity3D, j);
						scaledUV.y *= 0.5;

						float color = 0;

                        //modify speed based on octave
						float zVal = _Time.y * _Speed3D * pow(_OctaveSpeed3D, j);

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

                        //1D hash
						uint leftHash = hash3D[xFloor & hashSize3D];
						uint rightHash = hash3D[(xFloor + 1) & hashSize3D];

                        //2D hash
						uint bottomLeftHash = hash3D[(leftHash + yFloor) & hashSize3D];
						uint bottomRightHash = hash3D[(rightHash + yFloor) & hashSize3D];
						uint topLeftHash = hash3D[(leftHash + yFloor + 1) & hashSize3D];
						uint topRightHash = hash3D[(rightHash + yFloor + 1) & hashSize3D];

                        //3D hash
						fixed3 frontBottomLeftGrad = gradients3D[hash3D[(bottomLeftHash + zFloor) & hashSize3D]];
						fixed3 frontBottomRightGrad = gradients3D[hash3D[(bottomRightHash + zFloor) & hashSize3D]];
						fixed3 frontTopLeftGrad = gradients3D[hash3D[(topLeftHash + zFloor) & hashSize3D]];
						fixed3 frontTopRightGrad = gradients3D[hash3D[(topRightHash + zFloor) & hashSize3D]];
						fixed3 backBottomLeftGrad = gradients3D[hash3D[(bottomLeftHash + zFloor + 1) & hashSize3D]];
						fixed3 backBottomRightGrad = gradients3D[hash3D[(bottomRightHash + zFloor + 1) & hashSize3D]];
						fixed3 backTopLeftGrad = gradients3D[hash3D[(topLeftHash + zFloor + 1) & hashSize3D]];
						fixed3 backTopRightGrad = gradients3D[hash3D[(topRightHash + zFloor + 1) & hashSize3D]];

						float frontBottomLeftVal = dot(frontBottomLeftGrad, fixed3(xRemainder, yRemainder, zRemainder));
						float frontBottomRightVal = dot(frontBottomRightGrad, fixed3(xRemMinusOne, yRemainder, zRemainder));
						float frontTopLeftVal = dot(frontTopLeftGrad, fixed3(xRemainder, yRemMinusOne, zRemainder));
						float frontTopRightVal = dot(frontTopRightGrad, fixed3(xRemMinusOne, yRemMinusOne, zRemainder));
						float backBottomLeftVal = dot(backBottomLeftGrad, fixed3(xRemainder, yRemainder, zRemMinusOne));
						float backBottomRightVal = dot(backBottomRightGrad, fixed3(xRemMinusOne, yRemainder, zRemMinusOne));
						float backTopLeftVal = dot(backTopLeftGrad, fixed3(xRemainder, yRemMinusOne, zRemMinusOne));
						float backTopRightVal = dot(backTopRightGrad, fixed3(xRemMinusOne, yRemMinusOne, zRemMinusOne));

						//linear interpolation
						color = lerp(
							lerp(lerp(frontBottomLeftVal, frontBottomRightVal, xInterp), lerp(frontTopLeftVal, frontTopRightVal, xInterp), yInterp),
							lerp(lerp(backBottomLeftVal, backBottomRightVal, xInterp), lerp(backTopLeftVal, backTopRightVal, xInterp), yInterp),
							zInterp);

						float strength = pow(_Persistence3D, j + 1);
						totalStrength3D += strength;
						col3D += fixed4(color, color, color, 0) * strength;
					}

					//col3D = smoothstep(0, 1, smoothstep(0, 1, smoothstep(0, 1, col / sqrt(3) + 0.5f)));

					col3D /= totalStrength3D;
					float darkColStr3D = col3D / sqrt(3) + 0.5f;
					float lightColStr3D = 1 - darkColStr3D;

					col3D = _LightColor3D * lightColStr3D + _DarkColor3D * darkColStr3D;
				}

                //add colors
                fixed4 finalCol = col3D * _Component3D + col1D * _Component1D;

                //fade effect bades on alignment of normals with view vector
				float a = dot(i.normal, i.cameraDirection);
				a *= a;

                //subtract opacity at top and bottom using sine curve
				a += min(0, (-sin((i.uv.y + 0.333333) * 3.151592654 * 1.7) + 0.2));
                //ensure alpha is in 0-1 range
				a = saturate(a);

                //opacity may be introduced through the selected colors, multiply to achieve additional transparency
				finalCol.a *= a;

				return finalCol;

			}
			ENDCG
		}
	}
}
