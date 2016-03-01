Shader "SDnBshades/TriParticleShader" 
{

	Properties
	{
		_Color("Color", Color) = (0.38,0.26,0.98,1.0)
		_Size("Size", float) = 0.1
	}
	SubShader 
	{
		//uses DrawProcedural so tag doesnt really matter
		Tags { "Queue" = "Transparent" }
	
		Pass 
		{
		
		ZWrite Off 
		ZTest LEqual 
		Cull Off 
		Fog { Mode Off }
    	Blend SrcAlpha OneMinusSrcAlpha
       	//Blend One One
     	
		CGPROGRAM
		#pragma target 5.0

		#pragma vertex vert
		#pragma geometry geom
		#pragma fragment frag

		#include "UnityCG.cginc"

		//StructuredBuffer<float4> _Colors;		
		StructuredBuffer<float3> _VertPos;

		
		fixed4 _Color;
		float _Size;

		struct v2g 
		{
			float4 pos : SV_POSITION;
			int id : TEXCOORD0;
			float4 col : COLOR;
		};

		v2g vert(uint id : SV_VertexID)
		{
			v2g OUT;
			float3 worldPos = _VertPos[id];
			OUT.pos = mul (UNITY_MATRIX_VP, float4(worldPos,1.0f));
			OUT.id = id;
			OUT.col = _Color;

			return OUT;
		}
		
		struct g2f {
		float4 posi : SV_POSITION;
		float2 uv : TEXCOORD0;
		float4 colo : COLOR;
		};

		[maxvertexcount(3)]
		void geom(point v2g IN[1], inout TriangleStream<g2f> outStream)
		{
			float dx = _Size;
			float dy = _Size * _ScreenParams.x / _ScreenParams.y;
			g2f OUT;
			OUT.colo = IN[0].col;
			OUT.posi = IN[0].pos + float4(-dx, dy,0,0); OUT.uv=float2(0,0); outStream.Append(OUT);
			OUT.posi = IN[0].pos + float4( dx, dy,0,0); OUT.uv=float2(1,0); outStream.Append(OUT);
			OUT.posi = IN[0].pos + float4(-dx,-dy,0,0); OUT.uv=float2(0,1); outStream.Append(OUT);
			//OUT.posi = IN[0].pos + float4( dx,-dy,0,0); OUT.uv=float2(1,1); outStream.Append(OUT);

			outStream.RestartStrip();
		}

		float4 frag (g2f IN) : COLOR
		{
			//return IN.colo; 
			return float4(IN.colo.rgb ,IN.colo.a ); 
		}

		ENDCG

		}
	}

Fallback Off
}















