Shader "Custom/ToonShaderAlpha"
{
	Properties
	{
		_myColor("Example Color", Color) = (1, 1, 1, 1)
		_Texture("Texture", 2D) = "white" {}
	}

		SubShader
	{
		// Not necessary, done by default
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

		ZWrite Off

		CGPROGRAM
			#pragma surface surf BasicToon alpha

			half4 LightingBasicToon(SurfaceOutput s, half3 lightDir, half atten)
			{
				half NdotL = dot(s.Normal, lightDir) * 0.5 + 0.5;
				half diff = NdotL > 0.75 ? 1 : 0.5;
				half4 c;
				c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten);
				c.a = s.Alpha;
				return c;
			}

			fixed4 _myColor;
			sampler2D _Texture;

			struct Input
			{
				float2 uv_Texture;
			};

			void surf(Input IN, inout SurfaceOutput o)
			{
				o.Albedo = (tex2D(_Texture, IN.uv_Texture) * _myColor).rgb;
				o.Alpha = tex2D(_Texture, IN.uv_Texture).a;
			}
		ENDCG
	}

		FallBack "Diffuse"
}