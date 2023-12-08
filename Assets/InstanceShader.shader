Shader "Unlit/InstanceShader"
{
    SubShader
    {
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
                if (length != 0)
                    v.vertex.x *= length;
                
                float angle = _Angles[instanceID];
                v.vertex.xy = mul(rotationAroundZ(angle), v.vertex.xy);
    
                v.vertex.xy += _Points[instanceID];
                
                o.color = _Colors[instanceID].xxx;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return float4(i.color, 1);
}
            ENDCG
        }
    }
}
