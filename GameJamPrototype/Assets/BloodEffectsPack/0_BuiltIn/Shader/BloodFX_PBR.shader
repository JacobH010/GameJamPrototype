Shader "BloodEffectsPack/BuiltIn/BloodFX_PBR"
{
    Properties
    {
        [Header(ViewDirMask)]
        _ViewDirMaskThreshold("ViewDirMaskThreshold", range(0,1)) = 0.1
        
        [Header(Color)]
        _Color("Color", color) = (1,1,1,1)
        _ColorIntensity("ColorIntensity", float) = 1.0
        _AlbedoPower("AlbedoPower", float) = 1.0
       

        _HueShift("HueShift", Range(-180,180)) = 0
        _AmbientColorIntensity("AmbientColorIntensity", float) = 1.0

        [Header(Main)]
        [NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
        _Smoothness("Smoothness", Range(0,1)) = 0


        [NoScaleOffset] _NormalMap("NormalMap", 2D) = "bump"{}
        _BumpScale("BumpScale", float) = 1.0

        [Header(Specularity)]
        [Toggle] _UseSpecularity("UseSpecularity", float) = 1.0
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}

    
          

            Pass
            {
            
                Tags {"LightMode" = "ForwardBase"}

                ZWrite Off
                Blend SrcAlpha OneMinusSrcAlpha

                CGPROGRAM
                #pragma target 3.0
                #pragma multi_compile _ VERTEXLIGHT_ON

                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog

                #define FORWARD_BASE_PASS
                #include "BloodVFX_Lighting.cginc"
                ENDCG
            }
            Pass
            {
                Tags{"LightMode" = "ForwardAdd"}
          
                Blend One One
                ZWrite Off
                CGPROGRAM
                #pragma target 3.0
            
                #pragma multi_compile_fwdadd
                #pragma vertex vert
                #pragma fragment frag


   
                #pragma multi_compile_fog
                #include "BloodVFX_Lighting.cginc"
                ENDCG
            }
            Pass
            {	
                Tags {"LightMode" = "ShadowCaster"}
                ZWrite On
                CGPROGRAM
                #pragma target 3.0
                #pragma vertex vert
                #pragma fragment frag
                #include "BloodVFX_Shadow.cginc"
                ENDCG
               


            }
        }
}
