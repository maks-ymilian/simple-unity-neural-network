Shader"Unlit/EdgeInstance"
{
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 color : COlOR0;
            };

            StructuredBuffer<float2> _Points;
            StructuredBuffer<float> _Angles;
            StructuredBuffer<float> _Lengths;
            StructuredBuffer<float> _Colors;

            float2x2 rotationAroundZ(float angle)
            {
                float sint = sin(angle);
                float cost = cos(angle);
                return float2x2(cost, -sint, sint, cost);
            }

            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                InitIndirectDrawArgs(0);
                v2f o;
                uint instanceID = GetIndirectInstanceID(svInstanceID);
    
                float length = _Lengths[instanceID];
                v.vertex.x *= length;
                
                float angle = _Angles[instanceID];
                v.vertex.xy = mul(rotationAroundZ(angle), v.vertex.xy);
    
                v.vertex.xy += _Points[instanceID];
                
                o.color.x = _Colors[instanceID];
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 color;
                if (i.color.x < 0)
                    color = lerp(float4(1, 0.5f, 0, 1), float4(1, 1, 1, 0), saturate(i.color.x + 1));
                else
                    color = lerp(float4(1, 1, 1, 0), float4(0, 0.5f, 1, 1), saturate(i.color.x));
    
                return color;
            }
            ENDCG
        }
    }
}
