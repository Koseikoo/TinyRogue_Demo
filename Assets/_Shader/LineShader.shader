Shader "Unlit/LineShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _PointA ("Point A", Vector) = (0, 0, 0, 0)
        _PointB ("Point B", Vector) = (0, 0, 0, 0)
        _Thickness ("Thickness", Float) = 0.01
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" }
        Pass
        {
            Blend One OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _PointA;
            float4 _PointB;
            float _Thickness;
            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenPos = i.pos.xy / i.pos.w;

                float2 dir = _PointB.xy - _PointA.xy;
                float2 normal = normalize(float2(-dir.y, dir.x));

                float dist = dot(screenPos - _PointA.xy, dir) / length(dir);
                float width = abs(_Thickness / length(dir));
                float alpha = saturate(0.5 - abs(dist) / width);

                return _Color * alpha;
            }
            ENDCG
        }
    }
}
