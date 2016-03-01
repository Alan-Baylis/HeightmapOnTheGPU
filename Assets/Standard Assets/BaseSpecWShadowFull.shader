Shader "Custom/BaseSpecWShadowFull" 
{
	Properties 
	{	
		_DiffColor ("Diffuse Color", Color) = (1.0,1.0,1.0,1.0)
		_SpecColor ("Specular Color", Color) = (1.0,1.0,1.0,1.0)
		_Shininess ("Shininess", float) = 1
	
		_MainTex ("Main Texture", 2D) = "white" {}
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


			uniform float4 _DiffColor;
			uniform float4 _SpecColor;
			uniform float _Shininess;

			//unity defined vars
			uniform float4 _LightColor0;

						
			struct v2f {
				float4 pos : SV_POSITION;
				float4 posWorld : TEXCOORD0;
				float4 tex : TEXCOORD1;

				float3 normalWorld : TEXCOORD2;
				float3 tangentWorld : TEXCOORD3;
				float3 binormalWorld : TEXCOORD4; 

				//shadow bro
				LIGHTING_COORDS(3,4)
			};
			
			v2f vert(appdata_full v)
			{
				v2f o;
				o.normalWorld = normalize(mul(float4(v.normal, 0.0), _World2Object).xyz);
				o.tex = v.texcoord;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.posWorld = mul(_Object2World, v.vertex);
				
				
				o.tangentWorld = normalize( mul( _Object2World, v.tangent ).xyz );
				o.binormalWorld = normalize( cross( o.normalWorld, o.tangentWorld) * v.tangent.w );

				//shadow bro
				TRANSFER_VERTEX_TO_FRAGMENT(o);

				return o;
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
				float4 textureMapCol =  tex2D(_MainTex, i.tex.xy *_MainTex_ST.xy + _MainTex_ST.zw);


				float3 diffuseReflection = atten *  saturate( dot(normalDirection, lightDirection ) );
				float3 specularReflection = _SpecColor.rgb * diffuseReflection * pow(max(0.0, dot( reflect(-lightDirection, normalDirection), viewDirection)), _Shininess); 
				diffuseReflection *=  _DiffColor.rgb*_LightColor0.rgb;
				
				float3 lightFinal = specularReflection * _SpecColor.a + diffuseReflection*_DiffColor.a + UNITY_LIGHTMODEL_AMBIENT.rgb*2;
				//lightFinal *= textureMapCol.rgb * _DiffColor.rgb;
				


				return float4(lightFinal , 1);
			}
			ENDCG
		}
	}
		
		
		//second pass, remove (maybe... any textures) but def remove ambient lighting		

	FallBack "Diffuse"
}
