Shader "Custom/BilinearFiltering" 
{
	Properties 
	{	
		_DiffColor ("Diffuse Color", Color) = (1.0,1.0,1.0,1.0)
		_SpecColor ("Specular Color", Color) = (1.0,1.0,1.0,1.0)
		_Shininess ("Shininess", float) = 1
		_MainTexDims("Dimensions", Vector) = (256, 256,0,1)
		_MainTex ("Heightmap Texture", 2D) = "white" {}
		
		_UtiliSlider("Utility Slider", Range(0,1)) = 0

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

			#pragma vertex vert		
			#pragma fragment frag
			#pragma target 3.0
			
			//following is for shadows
     		#pragma multi_compile_fwdbase
 			#include "UnityCG.cginc"
 			#include "AutoLight.cginc"


						
			
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTexDims;

			uniform float4 _DiffColor;
			uniform float4 _SpecColor;
			uniform float _Shininess;

			//unity defined vars
			uniform float4 _LightColor0;
			uniform float _UtiliSlider;
						
			struct v2f {
				float4 pos : SV_POSITION;
				float4 posWorld : TEXCOORD0;
				float4 tex : TEXCOORD1;

				float3 normalWorld : TEXCOORD2;
				//shadow bro
				LIGHTING_COORDS(3,4)
			};
			
			v2f vert(appdata_base v)
			{
				v2f o;
				o.normalWorld = normalize(mul(float4(v.normal, 0.0), _World2Object).xyz);
				o.tex = v.texcoord;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.posWorld = mul(_Object2World, v.vertex);
				
				//shadow bro
				TRANSFER_VERTEX_TO_FRAGMENT(o);

				return o;
			}
			
			
			float splineM(float2 pa, float2 pb)
			{
				return (pb.y - pa.y)/(pb.x-pa.x);
			}
			
			
			float splineInterp(float i, float2 p1, float2 p2, float2 p3, float2 p4)
			{
				float m1 = splineM(p1, p2);
				float m2 = splineM(p2, p3);
				float m3 = splineM(p3, p4);
				
				float L2 = m2*(i - p2.x) + p2.y;
				
				float zB = 4*(p1.x*p2.x + p3.x*p4.x - p1.x*p4.x)-pow(p2.x + p3.x,2);
				float z2 = 6*(m3*p2.x + m2*p3.x - m3*p3.x + 2*m2*p4.x + 2*m1*p2.x - 2*m1*p4.x - 3* m2*p2.x)/zB;
				float z3 = 6*(m2*p2.x + m1*p3.x - m1*p2.x + 2*m2*p1.x + 2*m3*p3.x - 2*m3*p1.x - 3* m2*p3.x)/zB;
				
				float a2 = (2*z2 + z3)/(6*(p2.x - p3.x));
				float b2 = (2*z3 + z2)/(6*(p3.x - p2.x));
				
				float C2 = a2*pow((i - p3.x),2)*(i - p2.x) + b2*(i-p3.x)*pow((i-p2.x),2);
				return C2 + L2;
			}
			
			float txH(float2 puv)
			{
				float height =  tex2D(_MainTex, puv.xy *_MainTex_ST.xy + _MainTex_ST.zw).r;
				return height;
			}
			
			float2 stepUV(float2 parUV, int2 pixShift)
			{
				parUV *= _MainTexDims.xy;
				return (floor(parUV) + float2(pixShift.x,pixShift.y)*_MainTexDims.w)/_MainTexDims.xy;
			}
			
			float getHeight(float2 parUV)
			{
				float3 uv00 = float3(stepUV(parUV, int2(-1,-1)),1);
				float3 uv01 = float3(stepUV(parUV, int2(-1,00)),1);
				float3 uv02 = float3(stepUV(parUV, int2(-1, 1)),1);
				float3 uv03 = float3(stepUV(parUV, int2(-1, 2)),1);
												  
				float3 uv10 = float3(stepUV(parUV, int2(00,-1)),1);
				float3 uv11 = float3(stepUV(parUV, int2(00,00)),1);
				float3 uv12 = float3(stepUV(parUV, int2(00, 1)),1);
				float3 uv13 = float3(stepUV(parUV, int2(00, 2)),1);
												  
				float3 uv20 = float3(stepUV(parUV, int2( 1,-1)),1);
				float3 uv21 = float3(stepUV(parUV, int2( 1,-0)),1);
				float3 uv22 = float3(stepUV(parUV, int2( 1, 1)),1);
				float3 uv23 = float3(stepUV(parUV, int2( 1, 2)),1);
												    
				float3 uv30 = float3(stepUV(parUV, int2( 2,-1)),1);
				float3 uv31 = float3(stepUV(parUV, int2( 2,-0)),1);
				float3 uv32 = float3(stepUV(parUV, int2( 2, 1)),1);
				float3 uv33 = float3(stepUV(parUV, int2( 2, 2)),1);
				
				uv00.z = txH(uv00)*_MainTexDims.z;
				uv01.z = txH(uv01)*_MainTexDims.z;
				uv02.z = txH(uv02)*_MainTexDims.z;
				uv03.z = txH(uv03)*_MainTexDims.z;
					
				uv10.z = txH(uv10)*_MainTexDims.z;
				uv11.z = txH(uv11)*_MainTexDims.z;
				uv12.z = txH(uv12)*_MainTexDims.z;
				uv13.z = txH(uv13)*_MainTexDims.z;
					
				uv20.z = txH(uv20)*_MainTexDims.z;
				uv21.z = txH(uv21)*_MainTexDims.z;
				uv22.z = txH(uv22)*_MainTexDims.z;
				uv23.z = txH(uv23)*_MainTexDims.z;
					
				uv30.z = txH(uv30)*_MainTexDims.z;
				uv31.z = txH(uv31)*_MainTexDims.z;
				uv32.z = txH(uv32)*_MainTexDims.z;
				uv33.z = txH(uv33)*_MainTexDims.z;
				
				
				float crossHatch0 = splineInterp(parUV.x, uv00.xz, uv10.xz, uv20.xz, uv30.xz);
				float crossHatch1 = splineInterp(parUV.x, uv01.xz, uv11.xz, uv21.xz, uv31.xz);
				float crossHatch2 = splineInterp(parUV.x, uv02.xz, uv12.xz, uv22.xz, uv32.xz);
				float crossHatch3 = splineInterp(parUV.x, uv03.xz, uv13.xz, uv23.xz, uv33.xz);

				float pinPoint = splineInterp(parUV.y, float2(uv00.y, crossHatch0), float2(uv01.y, crossHatch1),float2(uv02.y, crossHatch2), float2(uv03.y, crossHatch3));
				
				return pinPoint/_MainTexDims.z;
			}
			
			

			float4 frag(v2f i) : COLOR
			{					
				
				//more efficient way to get the light stuff:
				float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz, _WorldSpaceLightPos0.w));
				
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz); //convert and use object coords not world coords like o.pos would

				//float atten = lerp( 1.0, 1.0/dist, _WorldSpaceLightPos0.w);
				//following needed for shadow
				float atten = LIGHT_ATTENUATION(i);
				
				float3 normalDirection = i.normalWorld;
				//texture unwrapper
				
				
				
				float textureMapCol =  tex2D(_MainTex, i.tex.xy *_MainTex_ST.xy + _MainTex_ST.zw).r;
				
				

				float3 diffuseReflection = atten *  saturate( dot(normalDirection, lightDirection ) );
				float3 specularReflection = _SpecColor.rgb * diffuseReflection * pow(max(0.0, dot( reflect(-lightDirection, normalDirection), viewDirection)), _Shininess); 
				diffuseReflection *=  _DiffColor.rgb*_LightColor0.rgb;
				
				float3 lightFinal = specularReflection * _SpecColor.a + diffuseReflection*_DiffColor.a + UNITY_LIGHTMODEL_AMBIENT.rgb*2;
				//lightFinal *= textureMapCol.rgb * _DiffColor.rgb;
				
				float2 tuv = i.tex.xy;
				
				//This is part of something else: (an attempt to smooth out the wierd bits that crop up
				//tuv = 1 - (tuv*_MainTexDims.xy -  floor(tuv*_MainTexDims.xy));
				//int power = 25;
				//tuv = saturate(tuv - 0.9);
				
				
				tuv = (i.tex.xy*_MainTexDims.xy+ tuv.xy)/_MainTexDims.xy;
				float4 SecCol = float4(stepUV(tuv, int2(4,0)),0,1);//textureMapCol.r;
				float4 alpha = lerp(getHeight(tuv), SecCol ,_UtiliSlider);
				return alpha;//float4(textureMapCol.rgb , 1);
			}
			ENDCG
		}
	}
		
		
		//second pass, remove (maybe... any textures) but def remove ambient lighting		

	FallBack "Diffuse"
}
