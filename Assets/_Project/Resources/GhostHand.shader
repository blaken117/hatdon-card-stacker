Shader "Hatdon/GhostHand"
{
    Properties { _Color ("Colour", Color) = (0.65,0.88,1,0.20) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct Input { float4 vertex:POSITION; float3 normal:NORMAL; };
            struct Output { float4 position:SV_POSITION; float3 normal:TEXCOORD0; float3 world:TEXCOORD1; };
            Output vert(Input v) { Output o; o.position=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o; }
            fixed4 frag(Output i):SV_Target
            {
                float light=.63+.37*abs(dot(normalize(i.normal),normalize(float3(-.4,.8,-.5))));
                float rim=1-abs(dot(normalize(i.normal),normalize(_WorldSpaceCameraPos-i.world)));
                return fixed4(_Color.rgb*light,_Color.a*(.12+.88*rim*rim));
            }
            ENDCG
        }
    }
}
