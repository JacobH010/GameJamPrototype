Shader "Custom/AlwaysOnTopLit"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "Queue" = "Overlay" }  // Renders on top of everything
        LOD 200

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            ZTest Always             // Renders regardless of depth
            Cull Off                 // Render both sides
            Blend SrcAlpha OneMinusSrcAlpha // Support for transparency

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"  // Includes lighting functions

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the main texture
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color;

            // Calculate basic diffuse lighting
            float3 normal = normalize(i.worldNormal);
            fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
            float NdotL = max(dot(normal, worldLightDir), 0.0);
            fixed3 diffuse = _LightColor0.rgb * NdotL;

            // Combine color with diffuse lighting
            fixed4 finalColor = texColor * fixed4(diffuse, 1.0);

            return finalColor;
        }
        ENDCG
    }
    }
}
