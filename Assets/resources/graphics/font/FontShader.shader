// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/FontShader"
{
	Properties
	{
	    _MainTex ("Sprite Texture", 2D) = "white" {}
	    _Color ("Tint", Color) = (1,1,1,1)
	    [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

	    _StencilComp ("Stencil Comparison", Float) = 8
	    _Stencil ("Stencil ID", Float) = 0
	    _StencilOp ("Stencil Operation", Float) = 0
	    _StencilWriteMask ("Stencil Write Mask", Float) = 255
	    _StencilReadMask ("Stencil Read Mask", Float) = 255
	}

	SubShader {
		Tags
	    { 
	        "Queue"="Transparent" 
	        "IgnoreProjector"="True" 
	        "RenderType"="Transparent" 
	        "PreviewType"="Plane"
	        "CanUseSpriteAtlas"="True"
	    }
	    Stencil
	    {
	        Ref [_Stencil]
	        Comp [_StencilComp]
	        Pass [_StencilOp] 
	        ReadMask [_StencilReadMask]
	        WriteMask [_StencilWriteMask]
	    }

		Lighting Off Cull Off ZTest Always ZWrite Off Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform fixed4 _Color;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color * _Color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 col = i.color;
				col.a *= UNITY_SAMPLE_1CHANNEL(_MainTex, i.texcoord);
				return col;
			}
			ENDCG 
		}
	} 	
}