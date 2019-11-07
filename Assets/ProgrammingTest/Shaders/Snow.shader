Shader "Custom/Snow"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _OverlayColor ("Overlay Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Height ("Height", Range(0, 100)) = 10
        _HeightFalloff ("Height Falloff", Range(0, 2)) = 1
        _Slope ("Slope", Range(0, 1)) = 0.7
        _SlopeFalloff ("Slope Falloff", Range(0, 2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #include "Helper.cginc"
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _OverlayTex;
        float _Height;
        float _HeightFalloff;
        float _Slope;
        float _SlopeFalloff;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_OverlayTex;
            float3 worldPos;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _OverlayColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightMin = _Height - _HeightFalloff;
            float heightMax = _Height + _HeightFalloff;
            float slopeMin = _Slope - _SlopeFalloff;
            float slopeMax = _Slope + _SlopeFalloff;
            fixed3 verticalDirection = fixed3(0.0, 1.0, 0.0);
            float height = IN.worldPos.y;

            float heightStep = getClampedInterpolationStep(_Height, _HeightFalloff, height);

            float slope = dot(normalize(IN.worldNormal.xyz), verticalDirection);
            float slopeStep = getClampedInterpolationStep(_Slope, _SlopeFalloff, slope);

            float step = clamp(heightStep * slopeStep, 0.0, 1.0);
            fixed4 c = lerp(tex2D(_MainTex, IN.uv_MainTex) * _Color, tex2D(_OverlayTex, IN.uv_OverlayTex) * _OverlayColor, step);

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
