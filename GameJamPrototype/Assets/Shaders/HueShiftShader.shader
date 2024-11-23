Shader "Custom/HueShift"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _HueShift("Hue Shift", Range(0, 1)) = 0
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
            LOD 100

            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float _HueShift;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                float3 HueShift(float3 color, float shift)
                {
                    float angle = shift * 6.28; // Convert range [0,1] to radians [0, 2π]
                    float3 k = float3(0.57735, 0.57735, 0.57735); // Normalize (1,1,1)
                    float c = cos(angle);
                    float s = sin(angle);
                    return color * c + cross(k, color) * s + k * dot(k, color) * (1.0 - c);
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv);
                    col.rgb = HueShift(col.rgb, _HueShift);
                    // Preserve alpha
                    return fixed4(col.rgb, col.a);
                }
                ENDCG
            }
        }
}
