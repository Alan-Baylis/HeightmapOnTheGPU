Shader "Custom/NormalsAndAOfromHeightmap" 
{
	Properties 
	{	
		_DiffColor ("Diffuse Color", Color) = (1.0,1.0,1.0,1.0)
		_SpecColor ("Specular Color", Color) = (1.0,1.0,1.0,1.0)
		_Shininess ("Shininess", float) = 1
		_HeightTexDims("Dimensions XY then Z=texHeight, then W = step size for bilinear filterer", Vector) = (256, 256,5,0.1)
		_MaxParallax("MaxParralax", float) = 0.2

		_HeightTex ("Heightmap Texture", 2D) = "white" {}
		_UtiliSlider("Utility Slider", Range(0,1)) = 0
		        
		_TessControl ("TesselationControl; minDist, maxDist", Vector) = (1.0,1.0,1.0,1.0)   
		_Tess ("Tessellation", Range(1,32)) = 4

		
 	}
 	
 	
	SubShader 
	{
	

		//be sure to have for shadows
		Tags { "RenderType"="Opaque" }
		Pass
		{

			//be sure to have the following for shadows
			Tags {"LightMode" = "ForwardBase"}
			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest 

			//#pragma tessellate:tess
			#pragma vertex vert		
			#pragma fragment frag 
			#pragma target 5.0
			#include "Tessellation.cginc"

			//following is for shadows
     		#pragma multi_compile_fwdbase 
 			#include "UnityCG.cginc"
 			#include "AutoLight.cginc"
			
			#define BIFILTER 0
			#define PI 3.1415926
		
			
			uniform sampler2D _HeightTex;
			uniform float4 _HeightTex_ST;
			uniform float4 _HeightTexDims;

			uniform float4 _DiffColor;
			uniform float4 _SpecColor;
			uniform float _Shininess;

			uniform float _UtiliSlider;
			uniform float _MaxParallax;
			
			
			
			uniform float _Tess;
			uniform float4 _TessControl;
			//unity defined vars
			uniform float4 _LightColor0;
						
			struct v2f {
				float4 pos : SV_POSITION;
				float4 posWorld : TEXCOORD0;
				float4 tex : TEXCOORD1;

				float3 normalWorld : TEXCOORD2;
				float3 tangentWorld : TEXCOORD3;
				float3 binormalWorld : TEXCOORD4; 
				float3 viewDirInScaledSurfaceCoords :TEXCOORD5;
				float3 worldSpaceOffsetCoords :TEXCOORD6;

				//shadow bro
				LIGHTING_COORDS(7,8)
			};


			float4 tess(appdata_tan v0, appdata_tan v1, appdata_tan v2) 
            {
                //float2 texInfo = v0.color;//tex2Dlod(_MainTex, float4(v0.texcoord,0,0));

                float minDist = _TessControl.x;
                float maxDist = _TessControl.y;
                return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
            }

			
			
			v2f vert(appdata_tan v)
			{
				v2f o;
				o.tex = v.texcoord;
				
				float heightMapVal = tex2Dlod(_HeightTex,float4(o.tex.xy *_HeightTex_ST.xy + _HeightTex_ST.zw,0,0)).y;
				v.vertex.y += heightMapVal*(_HeightTexDims.z/4);

				o.normalWorld = normalize(mul(float4(v.normal, 0.0), _World2Object).xyz);

				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.posWorld = mul(_Object2World, v.vertex);
				
				o.tangentWorld = normalize( mul( _Object2World, v.tangent ).xyz );
				o.binormalWorld = normalize( cross( o.normalWorld, o.tangentWorld) * v.tangent.w );
           
           		float3 binormal = cross(v.normal, v.tangent.xyz)*v.tangent.w;
				float3x3 localSurfaceToScaledObjectT = float3x3(v.tangent.xyz, binormal, v.normal.xyz);
				 
				float3 viewDirInObjectCoords = mul(_World2Object, float4(_WorldSpaceCameraPos, 1.0)).xyz - v.vertex.xyz;
                o.viewDirInScaledSurfaceCoords = mul(localSurfaceToScaledObjectT, viewDirInObjectCoords); 
				o.worldSpaceOffsetCoords = mul(_Object2World,o.viewDirInScaledSurfaceCoords);
				
				TRANSFER_VERTEX_TO_FRAGMENT(o);

				//shadow bro

				return o;
			}			
			float txH(float2 puv)
			{
				float height =  tex2D(_HeightTex, puv.xy *_HeightTex_ST.xy + _HeightTex_ST.zw).r;
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
				float3 AO;
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
				
				float heightAt1 = txH(parUV);
				
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
				
				float AOall = AOX+AOY;//dot(float2(thetaX0, thetaY0), float2(thetaX2, thetaY2));
				float AOsign = (AOall)/abs(AOall);
				
				
				NandAO sol;
				sol.AO = float3(AOX, AOY, AOsign*length(float2(AOX, AOY)));
				
				
				float nX = tan(0.5*PI - thetaNX); // = opposite over adjacent, adjacent is 1 for this case (y/x)
				float nZ = tan(0.5*PI - thetaNZ); // = opposite over adjacent, adjacent is the Z length for the given Y length (y/z) so its y/(y/z) which is to say; y*z/y or just Z;
				
				sol.normalz = float2(riseX2, -riseY2);//float3(computeNorms(0,riseX0,riseX2,riseY0,riseY2), 0);
				
				return sol;
			}
			
			
			float3 StoC(float parVal)
			{
				return float3(abs(parVal), max(parVal,0), frac(max(parVal,0)));
			}
			
			float checker(float parVal)
			{
				return round((acos(sin(parVal*PI*2)))/PI);
			}
			
			float4 frag(v2f i) : COLOR
			{	

				float3 worldPos = i.posWorld.xyz;
				
				//more efficient way to get the light stuff: 
				float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - worldPos, _WorldSpaceLightPos0.w));
				
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - worldPos); //convert and use object coords not world coords like o.pos would
				//float atten = lerp( 1.0, 1.0/dist, _WorldSpaceLightPos0.w);
				//following needed for shadow
				float atten = LIGHT_ATTENUATION(i);
				
				
				NandAO normsAndAO = normalsAndAO(i.tex.xy);
				float2 hNORMs = normsAndAO.normalz;
				float hAO = normsAndAO.AO.z;
				
				float3 normalDirection = i.normalWorld + _HeightTexDims.z*(i.tangentWorld*-hNORMs.x + i.binormalWorld*hNORMs.y);
				
				normalDirection = normalize(normalDirection);
				//texture unwrapper
				
				float heightMapVal = tex2D(_HeightTex, i.tex.xy *_HeightTex_ST.xy + _HeightTex_ST.zw).y;
				float2 UV = i.tex.xy;
				float height = (-1+ heightMapVal)*_HeightTexDims.z;
				float2 deltaUV = clamp(height * i.viewDirInScaledSurfaceCoords.xy / i.viewDirInScaledSurfaceCoords.z , -_MaxParallax, +_MaxParallax); 
				UV += deltaUV;				


				float mult = 20;
				//float3 mainTexCol = float3(checker(mult*UV.x), checker(mult*UV.y), 1-abs(checker(mult*0.25*(UV.x))+checker(mult*0.25*(UV.y))-1) );
				float3 mainTexCol = float3(1,1,1);				
				
				float3 diffuseReflection = atten *  saturate( dot(normalDirection, lightDirection ) );
				float3 specularReflection =  atten*diffuseReflection * pow(max(0.0, dot( reflect(-lightDirection, normalDirection), viewDirection)), _Shininess); 
				diffuseReflection *=  _LightColor0.rgb*mainTexCol;
					
				float fresnel = saturate(dot(normalDirection, viewDirection));
														
				specularReflection*= _LightColor0.rgb;
				float AOmultiplier =  1-saturate(hAO);
				float3 lightFinal = _SpecColor.rgb * specularReflection * _SpecColor.a +(_DiffColor.rgb) * diffuseReflection*_DiffColor.a + UNITY_LIGHTMODEL_AMBIENT.rgb*_DiffColor.rgb;
				lightFinal*=AOmultiplier;
				
				float3 testa = float3(hNORMs.x, hNORMs.y, 0);
				testa = lerp(lightFinal,heightMapVal,_UtiliSlider);
				return float4(testa, 1);
				//return float4(hNORMs.x, 0, 0, 1);
				//return float4(hNORMs, 1);
				//return float4(lightFinal.rgb, 1);
				//return float4(diffuseReflection*AOmultiplier,1);
			}
			ENDCG
		}
	}
		
		
		//second pass, remove (maybe... any textures) but def remove ambient lighting		

	FallBack "Diffuse"
}
