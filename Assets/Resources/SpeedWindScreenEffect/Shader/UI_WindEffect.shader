Shader "Custom/UI_WindEffect"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _MainTexScaleScrollSpeed("MainTexScaleScrollSpeed", vector) = (1,1,0,0)
        _Center("Center", Vector) = (0.5, 0.5, 0.0, 0.0)

        [Header(Opacity)]
        _OpacityPower("OpacityPower", float) = 1.0
        _OpacityIntensity("OpacityIntensity", float) = 1.0

        _RadialMask_Value("RadialMask_Value", range(-1,1)) = 0.0
        _RadialMask_Blur("RadialMask_Blur", range(-1,1)) = 0.0


        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask("Color Mask", Float) = 15

      
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ UNITY_UI_CLIP_RECT

            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;

                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                #if UNITY_UI_CLIP_RECT
                    float4	mask : TEXCOORD1;
                #endif
            };

            sampler2D _MainTex;
            float _Radius;
            float4 _Center;
            float4 _MainTexScaleScrollSpeed;

            sampler2D _MaskTex;
            float _OpacityPower;
            float _OpacityIntensity;

            float _RadialMask_Value;
            float _RadialMask_Blur;

            half4 _ClipRect;
            half _MaskSoftnessX, _MaskSoftnessY;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;

                #if UNITY_UI_CLIP_RECT
					float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
					o.mask = half4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * float2((_MaskSoftnessX+1), (_MaskSoftnessY+1))));
				#endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 centerUV = _Center.xy;
                float2 polarCoord = i.uv - centerUV;

                float lengthValue = length(float3(polarCoord.xy, 0.0));
                float centerMask = smoothstep(_RadialMask_Value, _RadialMask_Value + _RadialMask_Blur, lengthValue);
       



                float angle = atan2(polarCoord.y, polarCoord.x);
                float radius = length(polarCoord) / 0.5;
                float2 polarUV = float2(angle / (2 * 3.14159), radius);
                polarUV = polarUV * _MainTexScaleScrollSpeed.xy + _MainTexScaleScrollSpeed.zw * _Time.y;


        

                // sample the texture
                float4 col = float4(1,1,1,1);
                col.a = tex2D(_MainTex, polarUV).r;
                col.a = saturate(pow(col.a, _OpacityPower) * _OpacityIntensity) * centerMask;
                col.rgb *=  i.color.rgb;


                #if UNITY_UI_CLIP_RECT	
                    half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(i.mask.xy)) * i.mask.zw);
                    col *= m.x * m.y;
                #endif
          
      
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
