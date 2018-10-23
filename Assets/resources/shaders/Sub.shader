// 减法遮罩
Shader "Custom/Sub" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
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
			SetTexture [_Mask] {combine texture}
			SetTexture [_MainTex] {combine texture,texture-previous}
		}
	}
}
