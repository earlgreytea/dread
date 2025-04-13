Shader "Custom/SimpleYAxisBillboard" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Brightness ("Brightness", Range(1.0, 3.0)) = 1.0
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Toggle] _AlphaCutoffEnabled ("Alpha Cutoff Enabled", Float) = 0
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha // アルファブレンド
        Cull Off // 両面描画（必要に応じて）

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            struct Beam {
                float3 pos;
                float3 dir;
                float length;
                float width;
                float4 color;
            };

            sampler2D _MainTex;
            float _Brightness;
            float _Cutoff;
            float _AlphaCutoffEnabled;
            StructuredBuffer<Beam> _BulletBuffer;
            v2f vert(appdata v) {
                v2f o;
                
                Beam beam = _BulletBuffer[v.instanceID];
                
                // Quadメッシュは、Unity標準のものを前提としています。
                // Unity標準のQuadはXY平面上にあるので、それに合わせて調整
                // スケールを適用（X=幅、Y=長さ）
                float3 scaledVertex = float3(v.vertex.x * beam.width, v.vertex.y * beam.length, v.vertex.z);
                
                // ビルボード回転行列の作成
                float4 worldPos = float4(beam.pos, 1.0);
                float3 lookDir = normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                
                // beam.dirを軸として使用
                float3 axisVec = normalize(beam.dir);
                
                // 軸に対して直交する視線ベクトルを計算
                float3 perpLookDir = lookDir - dot(lookDir, axisVec) * axisVec;
                perpLookDir = normalize(perpLookDir);
                
                // 右ベクトルと前方ベクトルを計算（向きを反転） メッシュの表側がカメラを向くように
                float3 rightVec = normalize(cross(perpLookDir, axisVec)); // 順序を入れ替え
                float3 forwardVec = normalize(cross(axisVec, rightVec));  // 順序を入れ替え
                
                // 回転行列を適用（axisVecがY軸の代わりになる）
                float3 rotatedPos = rightVec * scaledVertex.x + axisVec * scaledVertex.y + forwardVec * scaledVertex.z;
                
                // ワールド位置に移動
                float3 finalPos = beam.pos + rotatedPos;
                
                // クリップ空間に変換
                o.vertex = UnityWorldToClipPos(float4(finalPos, 1.0));
                o.uv = v.uv;
                o.color = beam.color;
                
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color * _Brightness;
                
                // if文を使わずにアルファカットオフを実装
                // _AlphaCutoffEnabledが1の場合はカットオフが有効、0の場合は無効
                // step関数を使って条件分岐を避ける
                clip((col.a - _Cutoff) * _AlphaCutoffEnabled);
                
                return col;
            }
            ENDCG
        }
    }
}