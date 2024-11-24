#if !defined(BLOODVFX_LIGHTING_INCLUDED)
#define BLOODVFX_LIGHTING_INCLUDED



#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
    float4 tangent: TANGENT;
    float4 color : COLOR;
};
struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float4 color : COLOR;
    float3 normal : TEXCOORD1;
    float4 tangent : TEXCOORD2;
    float3 worldPos : TEXCOORD3;

    #if defined(VERTEXLIGHT_ON)
        float3 vertexLightColor : TEXCOORD4;
    #endif
    UNITY_FOG_COORDS(5)
};



sampler2D _MainTex;
float _AlbedoPower;
float4 _Color;
float _ColorIntensity;
float _AmbientColorIntensity;
float _HueShift;

float _Smoothness;

sampler2D _NormalMap;
float _BumpScale;

float _ViewDirMaskThreshold;
float _UseSpecularity;

float3 ApplyHueShift(float3 aColor, float hue)
{
    float angle = radians(hue);
    float3 k = float3(0.57735, 0.57735, 0.57735);
    float cosAngle = cos(angle);
    return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
}



v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex);


    o.color = v.color;


    #if defined(VERTEXLIGHT_ON)
        o.vertexLightColor = Shade4PointLights(
            unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
            unity_LightColor[0].rgb, unity_LightColor[1].rgb,
            unity_LightColor[2].rgb, unity_LightColor[3].rgb,
            unity_4LightAtten0, o.worldPos, o.normal
        );
    #endif


    UNITY_TRANSFER_FOG(o, o.vertex);
    return o;
}

float4 frag(v2f i) : SV_Target
{



    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
    //float viewDirMask = i.viewDirMask;
    float viewDirMask = step(_ViewDirMaskThreshold, abs(dot(viewDir, i.normal)));

    float3 tangentSpaceNormal = UnpackScaleNormal(tex2D(_NormalMap, i.uv), _BumpScale);
    float3  binormal = cross(i.normal, i.tangent.xyz) * (i.tangent.w * unity_WorldTransformParams.w);




    i.normal = normalize(
        tangentSpaceNormal.x * i.tangent +
        tangentSpaceNormal.y * binormal +
        tangentSpaceNormal.z * i.normal);
    






    //Light
    UnityLight light;
    #if defined(POINT) || defined(POINT_COOKIE) || defined(POINT_SPOT)
        light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos.xyz);
    #else
        light.dir = _WorldSpaceLightPos0.xyz;
    #endif
    
    
    UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);






    light.color = _LightColor0.rgb * attenuation;

    light.ndotl = DotClamped(i.normal.xyz, light.dir);





    //Indirect Light
    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0;
    #if defined(VERTEXLIGHT_ON)
        indirectLight.diffuse = i.vertexLightColor;
    #endif
    #if defined(FORWARD_BASE_PASS)
        indirectLight.diffuse += max(0, ShadeSH9(float4(i.normal, 1)));
    #endif






    float4 albedo = tex2D(_MainTex, i.uv);
    albedo.rgb = pow(albedo.rgb, _AlbedoPower) * 2.0;
    albedo *= _Color;
    albedo *= i.color;
    albedo.rgb = ApplyHueShift(albedo.rgb, _HueShift);
    float4 col = albedo;

    col.rgb *= _ColorIntensity;
    
  




    //PBS
    col.a *= viewDirMask;
    float3 specularTint;
    float oneMinusReflectivity;
    col.rgb = DiffuseAndSpecularFromMetallic(col.rgb, 0, specularTint, oneMinusReflectivity);


    col.rgb = UNITY_BRDF_PBS(col.rgb, lerp(0.0, specularTint * pow(col.a,20) , _UseSpecularity),
        oneMinusReflectivity, _Smoothness,
        i.normal, viewDir,
        light, indirectLight);


    #if defined(FORWARD_BASE_PASS)
        col.rgb += albedo * _AmbientColorIntensity;
    #endif



    #if !defined(FORWARD_BASE_PASS)
        col.rgb *= col.a;
    #endif


    UNITY_APPLY_FOG(i.fogCoord, col);
    return col;
}

#endif
