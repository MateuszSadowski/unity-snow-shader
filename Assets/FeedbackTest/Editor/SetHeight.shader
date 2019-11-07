Shader "SetHeight"
{
    Properties { _MainTex ("Texture", any) = "" {} }
    SubShader
    {
        ZTest Always Cull OFF ZWrite Off
        HLSLINCLUDE
        #include "UnityCG.cginc"
        #include "TerrainTool.cginc"
        sampler2D _MainTex;
        float4 _MainTex_TexelSize;      // 1/width, 1/height, width, height
        sampler2D _BrushTex;
        float4 _BrushParams;            // x = strength, y = target height
        #define BRUSH_STRENGTH      ( _BrushParams[0] )
        #define TARGET_HEIGHT       ( _BrushParams[1] )
        struct appdata_t
        {
            float4 vertex : POSITION;
            float2 pcUV : TEXCOORD0;
        };
        struct v2f
        {
            float4 vertex : SV_POSITION;
            float2 pcUV : TEXCOORD0;
        };
        v2f vert( appdata_t v )
        {
            v2f o;
            
            o.vertex = UnityObjectToClipPos( v.vertex );
            o.pcUV = v.pcUV;
            return o;
        }
        ENDHLSL
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float4 frag( v2f i ) : SV_Target
            {
                float height = UnpackHeightmap( tex2D( _MainTex, i.pcUV ) );
                float2 brushUV = PaintContextUVToBrushUV( i.pcUV );
                float oob = all( saturate( brushUV ) == brushUV ) ? 1 : 0;
                height = lerp(height, TARGET_HEIGHT, BRUSH_STRENGTH * oob);
                return PackHeightmap( height );
            }
            ENDHLSL
        }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float4 frag( v2f i ) : SV_Target
            {
                float height = UnpackHeightmap( tex2D( _MainTex, i.pcUV ) );
                float2 brushUV = PaintContextUVToBrushUV( i.pcUV );
                float oob = all( saturate( brushUV ) == brushUV ) ? 1 : 0;
                float brush = UnpackHeightmap( tex2D( _BrushTex, brushUV ) );
                height = lerp(height, height > TARGET_HEIGHT ? 0.0f : 1.0f, BRUSH_STRENGTH * oob * brush);
                return PackHeightmap( height );
            }
            ENDHLSL
        }
    }
}