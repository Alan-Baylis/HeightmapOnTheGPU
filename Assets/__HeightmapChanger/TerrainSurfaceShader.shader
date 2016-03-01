Shader "TerrainTess" 
{
        Properties 
        {
            _Color ("Color", color) = (1,1,1,0)
            _SecColor ("Secondary color", color) = (0.4,0.1,0,0.5)
            _TriColor ("Trinary color", color) = (0.5,0.5,0.5,0.5)

            _Tess ("Tessellation", Range(1,32)) = 4
            _MainTex ("Base (RGB)", 2D) = "white" {}
            _HeightTex ("Heightmap Texture", 2D) = "gray" {}
            
            _HeightTexDims("Heightmap XY dimensions, Z = height, W = stepLength", Vector) = (256,256, 6, 0.1)
            _NormalMap ("Normalmap", 2D) = "bump" {}
            _Mod("Mod xy =(minmax tesselation range)  z = (displayHeight) ", Vector) = (1.0,1.0,1.0,1.0)
        }
        SubShader 
        {
            Tags { "RenderType"="Opaque" }
            LOD 300
            
            CGPROGRAM
            #pragma surface surf Standard  vertex:disp tessellate:tessDistance addshadow
            //addshadow fullforwardshadows nolightmap
            #pragma target 5.0
            #include "Tessellation.cginc"

			#define BIFILTER 1
			#define PI 3.1415926


            struct appdata 
            {
                float4 vertex : POSITION;
                float4 tangent : TANGENT;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };
            sampler2D _MainTex;
            sampler2D _HeightTex;
			float4 _HeightTex_ST;
			
            float _Tess;
            float4 _HeightTexDims;
            float4 _Mod;
           	float4 _SecColor;
           	float4 _TriColor;
            sampler2D _NormalMap;
            fixed4 _Color;

            float4 tessDistance (appdata v0, appdata v1, appdata v2) 
            {

                float minDist = _Mod.x;
                float maxDist = _Mod.y;
                return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
            }


            void disp (inout appdata v)
            {
                float d = tex2Dlod(_HeightTex, float4(v.texcoord.xy,0,0)).r * _Mod.z;
                v.vertex.xyz += v.normal * d;
            }

            struct Input {
                float2 uv_MainTex;
                
                float4 color : COLOR;
            };

           // sampler2D _MainTex;
            
            
            
            
			float txH(float2 puv)
			{
				float height =  tex2D(_HeightTex, puv.xy *_HeightTex_ST.xy + _HeightTex_ST.zw).r;
				return height;
			}
			
			float4 trueTXH(float2 puv)
			{
				float4 height =  tex2D(_HeightTex, puv.xy *_HeightTex_ST.xy + _HeightTex_ST.zw);
				return height;
			}
			
			float2 stepUV(float2 parUV, int2 pixShift)
			{
				#if BIFILTER
					parUV *= _HeightTexDims.xy;
					return (parUV + float2(pixShift.x, pixShift.y)*_HeightTexDims.w)/_HeightTexDims.xy;
				#else
					parUV *= _HeightTexDims.xy;
					return (floor(parUV) + pixShift)/_HeightTexDims.xy;
				#endif
			}
			
			struct NandAO
			{
				float2 normalz;
				float AO;
				float2 disp;
			};
			
			
			float2 computeNorms(float me, float e, float w, float s, float n)
			{
				//return float2(((n)-(s)), ((e)-(w)));
				return float2(-w, -n);
			}
			
			NandAO normalsAndAO(float2 parUV)
			{
				float run = 1;
				#if BIFILTER
					run = _HeightTexDims.w/_HeightTexDims.z;
				#endif
				
				float4 dataAt1 = trueTXH(parUV);
				float heightAt1 = dataAt1.x;
				
				float riseX0 = heightAt1 - txH(stepUV(parUV, int2(-1, 0)));
				float riseX2 = txH(stepUV(parUV, int2(1, 0))) - heightAt1;
				
				float riseY0 = heightAt1 - txH(stepUV(parUV, int2(0, -1)));
				float riseY2 = txH(stepUV(parUV, int2(0, 1))) - heightAt1;

				riseY0 *= _HeightTexDims.z;
				riseY2 *= _HeightTexDims.z;
				riseX2 *= _HeightTexDims.z;
				riseX0 *= _HeightTexDims.z;

				float thetaX0 = atan(riseX0/run);
				float thetaX2 = atan(riseX2/run);
				
				float thetaY0 = atan(riseY0/run);
				float thetaY2 = atan(riseY2/run);


				float thetaNX = thetaX0;//(thetaX0 + thetaX2)*0.5+PI*0.5;
				float thetaNZ = thetaY0;//(thetaY0 + thetaY2)*0.5+PI*0.5;

				float AOX = thetaX2 - thetaX0;
				float AOY = thetaY2 - thetaY0;
				//float AoX0Y0 = length(float2(thetaX0, thetaY0);
				//float AoX2Y2 = length(float2(thetaX2, thetaY2);
				//
				//float AoX0Y2 = length(float2(thetaX0, thetaY2);
				//float AoX2Y0 = length(float2(thetaX2, thetaY0);

				

				float AOall = AOX+AOY;//dot(float2(thetaX0, thetaY0), float2(thetaX2, thetaY2));

				//float AOall = AOX+AOY;//dot(float2(thetaX0, thetaY0), float2(thetaX2, thetaY2));
				float AOsign = (AOall)/abs(AOall);
				
				AOall = AOsign*length(float2(AOX, AOY));
				
				NandAO sol;
				//sol.AO = float3(AOX, AOY, AOsign*length(float2(AOX, AOY)));
				sol.AO = AOall;

				
				float nX = tan(0.5*PI - thetaNX); // = opposite over adjacent, adjacent is 1 for this case (y/x)
				float nZ = tan(0.5*PI - thetaNZ); // = opposite over adjacent, adjacent is the Z length for the given Y length (y/z) so its y/(y/z) which is to say; y*z/y or just Z;
				
				sol.normalz = float2(riseX2, -riseY2);//float3(computeNorms(0,riseX0,riseX2,riseY0,riseY2), 0);
				sol.disp = float2(dataAt1.x, dataAt1.y);
				return sol;
			}



            void surf (Input IN, inout SurfaceOutputStandard o) 
            {
                half4 texCol = tex2D(_MainTex, IN.uv_MainTex.xy);
                
                
                NandAO nando = normalsAndAO(IN.uv_MainTex);
                
                float3 mainColor = lerp(_SecColor.rgb,_Color.rgb, nando.disp.x);
                mainColor = lerp(_TriColor.rgb,mainColor, nando.disp.y);
                o.Albedo = IN.color.rgb*texCol.rgb*mainColor*_Color.a;

                //o.Occlusion = nando.AO.z;
                float AmbentOccl = 1- saturate(nando.AO);
                o.Occlusion = AmbentOccl;
                o.Metallic = 0;
               	o.Smoothness = 1;
                //o.Gloss = 1.0;
                o.Normal = float3(-nando.normalz.x, nando.normalz.y, 1);
               // o.Emission =lerp(_SecColor.rgb,_Color.rgb, nando.disp.y);


              // o.Emission = float3(-nando.normalz.x, nando.normalz.y, 1);

            }
            ENDCG
        }
        FallBack "Diffuse"
    }