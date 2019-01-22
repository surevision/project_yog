// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Screen" {
	//属性块，shader用到的属性，可以直接在Inspector面板调整
	Properties 
	{
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Brightness("Brightness", Float) = 1
		_Saturation("Saturation", Float) = 1
		_Contrast("Contrast", Float) = 1
		_Hue("Hue", Int) = 0
	}

	//每个shader都有Subshaer，各个subshaer之间是平行关系，只可能运行一个subshader，主要针对不同硬件
	SubShader
	{
		//真正干活的就是Pass了，一个shader中可能有不同的pass，可以执行多个pass
		Pass
			{
				//设置一些渲染状态，此处先不详细解释
				ZTest Always Cull Off ZWrite Off
				
				CGPROGRAM
				//在Properties中的内容只是给Inspector面板使用，真正声明在此处，注意与上面一致性
				sampler2D _MainTex;
				half _Brightness;
				half _Saturation;
				half _Contrast;
				int _Hue;

				//vert和frag函数
				#pragma vertex vert
				#pragma fragment frag
				#include "Lighting.cginc"

				float3 rgb2hsv (float3 color) {
					float r = color.r;
					float g = color.g;
					float b = color.b;

					float _min = min(min(r, g), b);
					float _max = max(max(r, g), b);

					float h,s,v;
					if (_min == _max) h = 0;
					if (_max == r && g>=b) h = 60*(g-b)/(_max-_min);
					if (_max == r && g < b) h = 60*(g-b)/(_max-_min) + 360;
					if (_max == g) h = 60*(b-r)/(_max-_min) + 120;
					if (_max == b) h = 60*(r-g)/(_max-_min) + 240;

					s = 1 - (_min/_max);
					if (_max == 0) s = 0;
					s *= 100;
					v = _max;
					v *= 100;
					return float3(h, s, v);
				}

				float3 hsv2rgb2(float h, float s, float v) {

					float _s = s / 100.0f;
					float _v = v / 100.0f;

					// only value = brightness.
					if(_s <= 0.000001f){
						return float3(_v, _v, _v);
					}

					// first find chroma:
					float _c = _v * _s;

					// the second largest component of this color
					float _h = h / 60;
					int _i = floor(_h);
					int _x = _c * (1 - abs( _h % 2 - 1));

					float r1, g1, b1;
					if (_i == 0) {
						r1 = _c;
						b1 = _x;
						g1 = 0;
					}
					if (_i == 1) {
						 r1=_x;
						 b1=_c;
						g1 = 0;
					}
					if (_i == 2) {
						 r1=0;
						 b1=_c;
						 g1=_x;
					}
					if (_i == 3) {
						 r1=0;
						 b1=_x;
						 g1=_c;
					}
					if (_i == 4) {
						 r1=_x;
						 b1=0;
						 g1=_c;
					}
					if (_i == 5) {
						 r1=_c;
						 b1=0;
						 g1=_x;
					}
					float _m = _v - _c;

					return float3((r1+_m),(b1+_m),(g1+_m));
				}

				float3 hsv2rgb(float h, float s, float v) {
					/*
						if (h < 0 || h > 360 || s < 0 || s > 100 || v < 0 || v > 100)
						{
							throw "hsv2rgb: wrong color input. \n H = [0..360], S = [0..100], V = [0..100] expected";
							return false;
						}
					*/
					int Hi   = floor(h / 60);
					int Vmin = ((100 - s) * v) / 100;
					int a    = (v - Vmin) * (floor(h) % 60) / 60;
					int Vinc = Vmin + a;
					int Vdec = v - a;
					int r = 0;
					int g = 0;
					int b = 0;
					if (Hi == 0) {
						r = v;
						g = Vinc;
						b = Vmin;
					}
					if (Hi == 1) {
						r = Vdec;
						g = v;
						b = Vmin;
					}
					if (Hi == 2) {
						r = Vmin;
						g = v;
						b = Vinc;
					}
					if (Hi == 3) {
						r = Vmin;
						g = Vdec;
						b = v;
					}
					if (Hi == 4) {
						r = Vinc;
						g = Vmin;
						b = v;
					}
					if (Hi == 5) {
						r = v;
						g = Vmin;
						b = Vdec;
					}
					// r = round(r);
					// g = round(g);
					// b = round(b);
					return float3(r / 255.0f, g / 255.0f, b / 255.0f);
				}

				//从vertex shader传入pixel shader的参数
				struct v2f
				{
					float4 pos : SV_POSITION; //顶点位置
					half2  uv : TEXCOORD0;	  //UV坐标
				};

				//vertex shader
				//appdata_img：带有位置和一个纹理坐标的顶点着色器输入
				v2f vert(appdata_img v)
				{
					v2f o;
					//从自身空间转向投影空间
					o.pos = UnityObjectToClipPos(v.vertex);
					//uv坐标赋值给output
					o.uv = v.texcoord;
					return o;
				}

				//fragment shader
				fixed4 frag(v2f i) : SV_Target
				{
					//从_MainTex中根据uv坐标进行采样
					fixed4 renderTex = tex2D(_MainTex, i.uv);
					// rgb转hsv
					float3 hsv = rgb2hsv(renderTex.rgb);
					fixed3 finalColor = hsv2rgb((hsv.x + _Hue) % 360, hsv.y, hsv.z * 2.5);
					// 亮度操作
					//brigtness亮度直接乘以一个系数，也就是RGB整体缩放，调整亮度
					finalColor = finalColor * _Brightness;
					//saturation饱和度：首先根据公式计算同等亮度情况下饱和度最低的值：
					fixed gray = 0.2125 * renderTex.r + 0.7154 * renderTex.g + 0.0721 * renderTex.b;
					fixed3 grayColor = fixed3(gray, gray, gray);
					//根据Saturation在饱和度最低的图像和原图之间差值
					finalColor = lerp(grayColor, finalColor, _Saturation);
					//contrast对比度：首先计算对比度最低的值
					fixed3 avgColor = fixed3(0.5, 0.5, 0.5);
					//根据Contrast在对比度最低的图像和原图之间差值
					finalColor = lerp(avgColor, finalColor, _Contrast);
					//返回结果，alpha通道不变
					return fixed4(finalColor, renderTex.a);
				}
				ENDCG
		}
	}
	//防止shader失效的保障措施
	FallBack Off
}
