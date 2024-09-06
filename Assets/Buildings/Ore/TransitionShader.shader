Shader "Custom/TransitionShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TransitionTex ("Transition Texture", 2D) = "white" {}
        _Progress ("Transition Progress", Range(0, 1)) = 0
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0.5
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionStrength ("Emission Strength", Float) = 1
    }
    SubShader
    {
        Tags {"RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        sampler2D _MainTex;
        sampler2D _TransitionTex;
        float _Progress;
        float _DissolveAmount;
        fixed4 _EmissionColor;
        float _EmissionStrength;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_TransitionTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 transitionValue = tex2D (_TransitionTex, IN.uv_TransitionTex);
            
            float dissolveValue = transitionValue.r - _Progress;
            float emissionValue = 1 - saturate(dissolveValue / _DissolveAmount);
            
            o.Albedo = c.rgb;
            o.Emission = _EmissionColor.rgb * emissionValue * _EmissionStrength;
            o.Alpha = step(0, dissolveValue);
        }
        ENDCG
    }
    FallBack "Diffuse"
}