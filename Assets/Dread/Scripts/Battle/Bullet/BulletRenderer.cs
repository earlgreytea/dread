using UnityEngine;
using System.Runtime.InteropServices;

namespace Dread.Battle.Bullet
{
    /// <summary>
    /// 弾の描画を担当するクラス。GPUインスタンシングを使用して多数の弾を効率的に描画します。
    /// </summary>
    public class BulletRenderer : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential)]
        struct BulletRenderData
        {
            public Vector3 position;
            public Vector3 direction;
            public float length;
            public float width;
            public Color color;
        }

        // 描画に使用するメッシュとマテリアル
        public Mesh bulletMesh;
        public Material bulletMaterial;

        // 最大インスタンス数
        public int maxInstanceCount = 1000;

        // GPUバッファ
        private ComputeBuffer argsBuffer;
        private ComputeBuffer dataBuffer;

        // 描画データ配列
        private BulletRenderData[] bulletRenderDataArray;

        // 初期化済みフラグ
        private bool isInitialized = false;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
                return;

            // デフォルトのメッシュがない場合は球体を使用
            if (bulletMesh == null)
            {
                bulletMesh = Resources.GetBuiltinResource<Mesh>("Sphere.mesh");
            }

            // 描画データ配列の初期化
            bulletRenderDataArray = new BulletRenderData[maxInstanceCount];
            for (int i = 0; i < maxInstanceCount; i++)
            {
                bulletRenderDataArray[i].position = Vector3.zero;
                bulletRenderDataArray[i].direction = Vector3.forward;
                bulletRenderDataArray[i].length = 0f;
                bulletRenderDataArray[i].width = 0f;
                bulletRenderDataArray[i].color = Color.clear;
            }

            // データバッファの作成
            dataBuffer = new ComputeBuffer(
                maxInstanceCount,
                Marshal.SizeOf(typeof(BulletRenderData))
            );
            dataBuffer.SetData(bulletRenderDataArray);
            bulletMaterial.SetBuffer("_BulletBuffer", dataBuffer);

            // 引数バッファの作成
            uint[] args = new uint[5]
            {
                bulletMesh.GetIndexCount(0),
                (uint)maxInstanceCount,
                0,
                0,
                0
            };
            argsBuffer = new ComputeBuffer(
                1,
                args.Length * sizeof(uint),
                ComputeBufferType.IndirectArguments
            );
            argsBuffer.SetData(args);

            isInitialized = true;
        }

        /// <summary>
        /// 弾の描画データを更新するメソッド
        /// </summary>
        public void UpdateBulletData(Bullet[] bullets, int activeCount)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            // アクティブな弾の数だけデータを更新
            int count = Mathf.Min(activeCount, maxInstanceCount);
            for (int i = 0; i < count; i++)
            {
                if (bullets[i].isActive)
                {
                    bulletRenderDataArray[i].position = bullets[i].position;
                    bulletRenderDataArray[i].direction = bullets[i].direction;
                    // 弾速によって長さを調整
                    bulletRenderDataArray[i].length = (bullets[i].speed / 200f) * 8f;
                    bulletRenderDataArray[i].width = 0.33f;
                    bulletRenderDataArray[i].color = bullets[i].color;
                }
                else
                {
                    // 非アクティブな弾は見えないようにする
                    SetBulletInvisible(i);
                }
            }

            // 残りのデータをクリア
            for (int i = count; i < maxInstanceCount; i++)
            {
                SetBulletInvisible(i);
            }

            // バッファにデータを設定
            dataBuffer.SetData(bulletRenderDataArray);
        }

        /// <summary>
        /// 弾を非表示にするメソッド
        /// </summary>
        private void SetBulletInvisible(int index)
        {
            bulletRenderDataArray[index].length = 0f;
            bulletRenderDataArray[index].width = 0f;
            bulletRenderDataArray[index].color = Color.clear;
        }

        /// <summary>
        /// 描画処理
        /// </summary>
        public void Render()
        {
            if (!isInitialized)
                return;

            // GPUインスタンシングを使用して描画
            Graphics.DrawMeshInstancedIndirect(
                bulletMesh,
                0,
                bulletMaterial,
                new Bounds(Vector3.zero, Vector3.one * 300f),
                argsBuffer
            );
        }

        /// <summary>
        /// リソースの解放
        /// </summary>
        private void OnDestroy()
        {
            dataBuffer?.Release();
            argsBuffer?.Release();
            isInitialized = false;
        }
    }
}
