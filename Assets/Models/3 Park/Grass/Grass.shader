Shader "BK/Grass"
{
    Properties
    {
        _ColorTop("Color Top", Color) = (0.3,0.9,0.3,1)
        _ColorBottom("Color Bottom", Color) = (0.1,0.4,0.1,1)
        _GradientPower("Gradient Power", Range(0.1,5)) = 1
        _GradientOffset("Gradient Offset", Range(-1,1)) = 0
        _MainTex("Albedo (RGBA)", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Scale", Range(0,2)) = 1
        _Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
        _WindStrength("Wind Strength", Range(0,1)) = 0.2
        _WindSpeed("Wind Speed", Range(0,5)) = 1
        
        [Header(Distance Settings)]
        _GrassFadeDistance("Fade Start Distance", Float) = 40
        _GrassFadeRange("Fade Range", Float) = 10
        _GrassMaxDistance("Max Draw Distance", Float) = 50
        _GrassDensity("Grass Density", Range(0,2)) = 1
        
        [Header(Rendering)]
        [Enum(Off,0,Front,1,Back,2)] _Cull("Cull Mode", Float) = 0
        _ShadowStrength("Shadow Strength", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }

        Cull [_Cull]

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 tangentWS   : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float3 positionWS  : TEXCOORD4;
                float  distanceFade : TEXCOORD5;
                float  gradientValue : TEXCOORD6; // 🔥 Значение градиента (0 = низ, 1 = верх)
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorTop;
                float4 _ColorBottom;
                float4 _MainTex_ST;
                float4 _NormalMap_ST;
                float _Cutoff;
                float _WindStrength;
                float _WindSpeed;
                float _NormalScale;
                float _GradientPower;
                float _GradientOffset;
                float _GrassFadeDistance;
                float _GrassFadeRange;
                float _GrassMaxDistance;
                float _GrassDensity;
                float _ShadowStrength;
            CBUFFER_END

            TEXTURE2D(_MainTex);   SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);

            static const float dither8x8[64] = {
                 0,32, 8,40, 2,34,10,42,
                48,16,56,24,50,18,58,26,
                12,44, 4,36,14,46, 6,38,
                60,28,52,20,62,30,54,22,
                 3,35,11,43, 1,33, 9,41,
                51,19,59,27,49,17,57,25,
                15,47, 7,39,13,45, 5,37,
                63,31,55,23,61,29,53,21
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                // Сначала вычисляем нормали ДО деформации
                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS = normalInput.normalWS;
                OUT.tangentWS = normalInput.tangentWS;
                OUT.bitangentWS = normalInput.bitangentWS;

                // 🔥 Вычисляем градиент на основе UV.y (0 = низ, 1 = верх)
                // Можно использовать UV или локальную Y позицию
                float gradientRaw = IN.uv.y; // или IN.positionOS.y для высоты в object space
                
                // Применяем power для контроля кривой градиента
                gradientRaw = saturate(gradientRaw + _GradientOffset);
                OUT.gradientValue = pow(gradientRaw, _GradientPower);

                // Потом применяем деформацию ветра
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);

                // Расчёт дистанции до камеры
                float dist = distance(_WorldSpaceCameraPos.xyz, posWS);
                
                // Отсечение по максимальной дистанции
                if (dist > _GrassMaxDistance)
                {
                    OUT.positionHCS = float4(0, 0, 0, 0);
                    OUT.uv = 0;
                    OUT.positionWS = 0;
                    OUT.distanceFade = 0;
                    return OUT;
                }

                // Колыхание ветром (только позиция)
                float wave = sin(_Time.y * _WindSpeed + posWS.x * 0.5 + posWS.z * 0.5);
                posWS.x += wave * _WindStrength * IN.uv.y;
                posWS.z += wave * _WindStrength * 0.5 * IN.uv.y;

                OUT.positionHCS = TransformWorldToHClip(posWS);
                OUT.positionWS = posWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                
                // Плавный fade по дистанции
                float fadeStart = _GrassFadeDistance;
                OUT.distanceFade = 1.0 - saturate((dist - fadeStart) / max(_GrassFadeRange, 0.01));
                
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                
                if (IN.distanceFade <= 0) discard;
                
                // Читаем текстуру
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Дизеринг для alpha cutoff
                float2 screenUV = IN.positionHCS.xy / IN.positionHCS.w * 0.5 + 0.5;
                int2 pixel = int2(fmod(screenUV.x * _ScreenParams.x, 8), fmod(screenUV.y * _ScreenParams.y, 8));
                float threshold = dither8x8[pixel.y * 8 + pixel.x] / 64.0;
                
                float densityFactor = 1.0 / max(_GrassDensity, 0.01);
                float ditherThreshold = _Cutoff + threshold * densityFactor * (2.0 - IN.distanceFade);
                if (tex.a < ditherThreshold) discard;

                // 🔥 ГРАДИЕНТ ЦВЕТА ОТ НИЗА К ВЕРХУ
                // 0 = ColorBottom (тёмный, у земли)
                // 1 = ColorTop (светлый, верхушки)
                half3 grassColor = lerp(_ColorBottom.rgb, _ColorTop.rgb, IN.gradientValue);
                
                // Применяем цвет к текстуре
                half3 baseColor = tex.rgb * grassColor;

                // Нормал-мапа (использует нетронутые нормали)
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv), _NormalScale);
                half3 normalWS = TransformTangentToWorld(normalTS, half3x3(IN.tangentWS, IN.bitangentWS, IN.normalWS));
                normalWS = normalize(normalWS);

                // Освещение с тенями
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half shadowAttenuation = lerp(1.0, mainLight.shadowAttenuation, _ShadowStrength);
                half3 lighting = mainLight.color * shadowAttenuation * NdotL;
                
                // Ambient освещение
                half3 ambient = SampleSH(normalWS) * 0.5;
                
                // Финальный цвет
                half3 finalColor = baseColor * (lighting + ambient);
                half finalAlpha = IN.distanceFade;

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Cutoff;
                float _GrassMaxDistance;
                float _GrassDensity;
                float _WindStrength;
                float _WindSpeed;
            CBUFFER_END

            TEXTURE2D(_MainTex); 
            SAMPLER(sampler_MainTex);

            float3 _LightDirection;
            
            static const float dither8x8[64] = {
                 0,32, 8,40, 2,34,10,42,
                48,16,56,24,50,18,58,26,
                12,44, 4,36,14,46, 6,38,
                60,28,52,20,62,30,54,22,
                 3,35,11,43, 1,33, 9,41,
                51,19,59,27,49,17,57,25,
                15,47, 7,39,13,45, 5,37,
                63,31,55,23,61,29,53,21
            };

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                // Применяем ветер в тенях
                float wave = sin(_Time.y * _WindSpeed + positionWS.x * 0.5 + positionWS.z * 0.5);
                positionWS.x += wave * _WindStrength * input.uv.y;
                positionWS.z += wave * _WindStrength * 0.5 * input.uv.y;

                float4 positionCS = TransformWorldToHClip(positionWS);

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionCS = GetShadowPositionHClip(input);
                
                float dist = distance(_WorldSpaceCameraPos.xyz, output.positionWS);
                if (dist > _GrassMaxDistance)
                {
                    output.positionCS = float4(0, 0, 0, 0);
                }
                
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                
                float2 screenUV = input.positionCS.xy / input.positionCS.w * 0.5 + 0.5;
                int2 pixel = int2(fmod(screenUV.x * _ScreenParams.x, 8), fmod(screenUV.y * _ScreenParams.y, 8));
                float threshold = dither8x8[pixel.y * 8 + pixel.x] / 64.0;
                
                float densityFactor = 1.0 / max(_GrassDensity, 0.01);
                clip(alpha - (_Cutoff + threshold * densityFactor));
                
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Cutoff;
                float _GrassMaxDistance;
            CBUFFER_END

            TEXTURE2D(_MainTex); 
            SAMPLER(sampler_MainTex);

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                float dist = distance(_WorldSpaceCameraPos.xyz, output.positionWS);
                if (dist > _GrassMaxDistance)
                {
                    output.positionCS = float4(0, 0, 0, 0);
                }
                
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
}