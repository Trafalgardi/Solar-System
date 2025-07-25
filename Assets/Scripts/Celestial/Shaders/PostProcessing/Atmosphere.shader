Shader "Hidden/Atmosphere_Mobile"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            // Вставьте нужные матем. функции отсюда, если используете внешний Math.cginc

            // Настройки циклов под мобильные:
            #define NUM_IN_SCATTERING_POINTS 8
            #define NUM_OPTICAL_DEPTH_POINTS 8

            sampler2D _BlueNoise;
            sampler2D _MainTex;
            sampler2D _BakedOpticalDepth;
            sampler2D _CameraDepthTexture;

            float4 _MainTex_ST;

            float3 dirToSun;
            float3 planetCentre;
            float atmosphereRadius;
            float oceanRadius;
            float planetRadius;

            float intensity;
            float4 scatteringCoefficients;
            float ditherStrength;
            float ditherScale;
            float densityFalloff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Универсальный способ — восстанавливаем view vector через обратную проекцию
                float2 ndc = o.uv * 2.0 - 1.0;
                float4 proj = mul(unity_CameraInvProjection, float4(ndc, 0, -1));
                float3 viewVec = proj.xyz;
                o.viewVector = mul((float3x3)unity_CameraToWorld, viewVec);
                return o;
            }

            float2 squareUV(float2 uv)
            {
                float x = uv.x * _ScreenParams.x;
                float y = uv.y * _ScreenParams.y;
                return float2(x / 1000.0, y / 1000.0);
            }

            // Функция получения плотности атмосферы
            float densityAtPoint(float3 p)
            {
                float h = length(p - planetCentre) - planetRadius;
                float h01 = h / (atmosphereRadius - planetRadius);
                float d = exp(-h01 * densityFalloff) * (1.0 - h01);
                return saturate(d);
            }

            // Оптическая глубина вдоль луча — цикл с фикс. шагом и лимитом
            float opticalDepth(float3 origin, float3 dir, float len)
            {
                float3 p = origin;
                float stepSize = len / (NUM_OPTICAL_DEPTH_POINTS - 1);
                float sum = 0;
                [loop]
                for (int i = 0; i < NUM_OPTICAL_DEPTH_POINTS; ++i)
                {
                    float d = densityAtPoint(p);
                    sum += d * stepSize;
                    p += dir * stepSize;
                }
                return sum;
            }

            float opticalDepthBaked(float3 origin, float3 dir)
            {
                float h = length(origin - planetCentre) - planetRadius;
                float h01 = saturate(h / (atmosphereRadius - planetRadius));
                float uvX = 1.0 - (dot(normalize(origin - planetCentre), dir) * 0.5 + 0.5);
                return tex2Dlod(_BakedOpticalDepth, float4(uvX, h01, 0, 0)).r;
            }

            float opticalDepthBaked2(float3 origin, float3 dir, float len)
            {
                float3 endPt = origin + dir * len;
                float d = dot(dir, normalize(origin - planetCentre));
                const float blendStrength = 1.5;
                float w = saturate(d * blendStrength + 0.5);

                float d1 = opticalDepthBaked(origin, dir) - opticalDepthBaked(endPt, dir);
                float d2 = opticalDepthBaked(endPt, -dir) - opticalDepthBaked(origin, -dir);

                return lerp(d2, d1, w);
            }

            // Заглушка raySphere: должен возвращать float2(hitStart, hitEnd)
            float2 raySphere(float3 sphereCentre, float sphereRadius, float3 rayOrigin, float3 rayDir)
            {
                float3 oc = rayOrigin - sphereCentre;
                float b = dot(oc, rayDir);
                float c = dot(oc, oc) - sphereRadius * sphereRadius;
                float h = b * b - c;
                if (h < 0) return float2(0, -1);
                h = sqrt(h);
                return float2(-b - h, -b + h);
            }

            float calculateLight(float3 rayOrigin, float3 rayDir, float rayLength, float3 originalCol, float2 uv)
            {
                float blueNoise = tex2Dlod(_BlueNoise, float4(squareUV(uv) * ditherScale, 0, 0)).r;
                blueNoise = (blueNoise - 0.5) * ditherStrength;

                float3 inScatterPoint = rayOrigin;
                float stepSize = rayLength / (NUM_IN_SCATTERING_POINTS - 1);
                float3 inScatteredLight = 0;
                float viewRayOpticalDepth = 0;

                [unroll(NUM_IN_SCATTERING_POINTS)]
                for (int i = 0; i < NUM_IN_SCATTERING_POINTS; ++i)
                {
                    float3 sunDir = dirToSun;
                    float sunRayLen = raySphere(planetCentre, atmosphereRadius, inScatterPoint, sunDir).y;
                    float sunRayOpticalDepth = opticalDepthBaked(inScatterPoint + sunDir * ditherStrength, sunDir);
                    float localDensity = densityAtPoint(inScatterPoint);
                    viewRayOpticalDepth = opticalDepthBaked2(rayOrigin, rayDir, stepSize * i);
                    float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * scatteringCoefficients.xyz);

                    inScatteredLight += localDensity * transmittance;
                    inScatterPoint += rayDir * stepSize;
                }
                inScatteredLight *= scatteringCoefficients.xyz * intensity * stepSize / max(planetRadius, 0.0001);
                inScatteredLight += blueNoise * 0.01;

                // Упрощённое затухание для оригинального цвета (от поверхности)
                const float brightnessAdaptionStrength = 0.15;
                const float reflectedLightOutScatterStrength = 3.0;
                float brightnessAdaption = dot(inScatteredLight, 1.0) * brightnessAdaptionStrength;
                float brightnessSum = viewRayOpticalDepth * intensity * reflectedLightOutScatterStrength + brightnessAdaption;
                float reflectedLightStrength = exp(-brightnessSum);
                float hdrStrength = saturate(dot(originalCol, 1.0) / 3.0 - 1.0);
                reflectedLightStrength = lerp(reflectedLightStrength, 1.0, hdrStrength);
                float3 reflectedLight = originalCol * reflectedLightStrength;

                float3 finalCol = reflectedLight + inScatteredLight;
                return finalCol;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 originalCol = tex2D(_MainTex, i.uv);
                float sceneDepthNonLinear = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float sceneDepth = LinearEyeDepth(sceneDepthNonLinear) * length(i.viewVector);

                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.viewVector);

                float dstToOcean = raySphere(planetCentre, oceanRadius, rayOrigin, rayDir).x;
                float dstToSurface = min(sceneDepth, dstToOcean);

                float2 hitInfo = raySphere(planetCentre, atmosphereRadius, rayOrigin, rayDir);
                float dstToAtmosphere = hitInfo.x;
                float dstThroughAtmosphere = min(hitInfo.y, dstToSurface - dstToAtmosphere);

                if (dstThroughAtmosphere > 0)
                {
                    const float epsilon = 0.0001;
                    float3 pointInAtmosphere = rayOrigin + rayDir * (dstToAtmosphere + epsilon);
                    float3 light = calculateLight(pointInAtmosphere, rayDir, dstThroughAtmosphere - epsilon * 2.0, originalCol.rgb, i.uv);
                    return float4(light, 1.0);
                }
                return originalCol;
            }

            ENDCG
        }
    }
}
