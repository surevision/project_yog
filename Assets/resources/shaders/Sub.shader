// 减法遮罩
Shader "Custom/Sub" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Mask ("Culling Mask", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range (0,1)) = 0.1	//透明度生效调整
	}
	SubShader {
		Tags { "Queue"="Transparent" } 
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest GEqual [_Cutoff]
		Pass
		{
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
			//LOOK! The texture name + ST is needed to get the tiling/offset
			uniform float4 _MainTex_ST;
			sampler2D _Mask;
			float4 _Mask_TexelSize;
			//LOOK! The texture name + ST is needed to get the tiling/offset
			uniform float4 _Mask_ST;


			struct appdata_t                           //vert输入
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f                                 //vert输出数据结构
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				half2 mask_uv	: TEXCOORD1;
			};

			// vert shader
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif
				OUT.mask_uv = IN.texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
				return OUT;
			}

			// frag shader
			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord);
				fixed4 c_mask = tex2D(_Mask, IN.mask_uv);
				c.a -= c_mask.a;
				return c * _Color;
			}

			ENDCG
		}
		/*Pass
		{
			SetTexture[_Mask] {combine texture}
			SetTexture[_MainTex] {combine texture,texture - previous}
		}*/
	}
}
