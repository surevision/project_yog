Shader "Custom/Loop" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_LoopTex ("Loop", 2D) = "white" {}
		_LoopTex_TexelSize ("LoopSize", Vector) = (1,1,1,1)
		_offsetX ("ox", Range(-1, 1)) = 0
		_offsetY ("oy", Range(-1, 1)) = 0
	}
	SubShader {
		Tags { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Cull Off         //关闭背面剔除
		Lighting Off     //关闭灯光
		ZWrite Off       //关闭Z缓冲
		Blend SrcAlpha OneMinusSrcColor     //混合源系数

		Pass {

			CGPROGRAM
		
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON       //告诉Unity编译不同版本的Shader,这里和后面vert中的PIXELSNAP_ON对应
			#include "UnityCG.cginc"

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			fixed4 _Color;
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			sampler2D _LoopTex;
			float4 _LoopTex_TexelSize;

			float _offsetX;
			float _offsetY;

			struct appdata_t                           //vert输入
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f                                 //vert输出数据结构
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};

			// vert shader
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif
				return OUT;
			}

			// frag shader
			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_LoopTex, float2(
					(floor(IN.texcoord.x * _MainTex_TexelSize.z + _MainTex_TexelSize.z * _offsetX + _MainTex_TexelSize.z) % _LoopTex_TexelSize.z) / _LoopTex_TexelSize.z, 
					(floor(IN.texcoord.y * _MainTex_TexelSize.w + _MainTex_TexelSize.w * _offsetY + _MainTex_TexelSize.w) % _LoopTex_TexelSize.w) / _LoopTex_TexelSize.w));
				c.a = 1 - (0.299 * c.r + 0.587 * c.g + 0.114 * c.b);	// 亮度转不透明度
				return c * _Color;
			}

			ENDCG
		}

	}
}