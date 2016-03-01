Shader "Custom/BufferShaderColorFromTexture"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_PercentTex("PercentageInfluenceTex", Range(0.0,1.0)) = 1
		_PercentLerp("PercentInfluenceLerp(not Multiply)", Range(0.0,1.0)) = 1
	}
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }
 
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
 			
 			
 			sampler2D _MainTex;
 			float _PercentTex;
 			float _PercentLerp;
 
            struct Vert
            {
                float3 position;
                float3 uv3s;
                float3 color;
            };
 
            uniform StructuredBuffer<Vert> buffer;
 
            struct v2f
            {
                float4  pos : SV_POSITION;
                float3 col : COLOR;
                float3 uv3 : TEXCOORD0;
            };
 
            v2f vert(uint id : SV_VertexID)
            {
                Vert vert = buffer[id];
 
                v2f OUT;
                OUT.pos = mul(UNITY_MATRIX_MVP, float4(vert.position, 1));
                OUT.col = vert.color;
                OUT.uv3 = vert.uv3s;
                return OUT;
            }
 
            float4 frag(v2f IN) : COLOR
            {
            	float4 mainTexCol = tex2D(_MainTex, IN.uv3.xy)*IN.uv3.z;
            	float4 lerpColToTex = lerp(float4(IN.col,1),mainTexCol,_PercentTex);
            	float4 multColTex = mainTexCol*float4(IN.col,1);
            	
                return lerp(multColTex, lerpColToTex, _PercentLerp);// float4(IN.col,1);
            }
 
            ENDCG
 
        }
    }
}