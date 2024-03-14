Shader "Unlit/BondBorderShader"
{
    
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Angle ("Angle", Range(0, 360)) = 45
        _StepStart ("StepStart", Range(0, 1)) = .45
        _StepLength ("StepLength", Range(0, 1)) = .5
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Angle;
            float _StepStart;
            float _StepLength;
            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Calculate angle in radians
                float angleRad = radians(_Angle);

                // Calculate coordinates of the point on the unit circle
                float2 circlePoint = float2(cos(angleRad), sin(angleRad)) * 0.5 + 0.5;

                // Masking using distance from center
                float dist = distance(circlePoint, i.uv);

                half4 f = tex2D(_MainTex, i.uv);
                float alpha = smoothstep(_StepStart, _StepStart + _StepLength, dist);

                return f * half4(1, 1, 1, 1-alpha);

                return half4(1, 1, 1, alpha);
            }
            ENDCG
        }
    }
}
