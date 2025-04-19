using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dread.Battle.Character;
using Dread.Battle.Path;
using Sirenix.OdinInspector;
using static Dread.Battle.Wave.WavesScenario.WaveContent;

namespace Dread.Battle.Spawner
{
    /// <summary>
    /// 敵を定期的に発生させるスポナークラス
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        // スポナーの設定情報
        private SpawnInfo runtimeSpawnInfo;

        // Spawner削除依頼用コールバック
        private System.Action<EnemySpawner> onRequestRemove;

        [FoldoutGroup("パス設定", expanded: true)]
        [SerializeField, LabelText("使用するパスのインデックス（-1でランダム）")]
        private int pathIndex = -1;

        [FoldoutGroup("パスオフセット設定")]
        [SerializeField, LabelText("X軸ランダムオフセット")]
        private float horizontalOffset = 50.0f;

        [FoldoutGroup("パスオフセット設定")]
        [SerializeField, LabelText("Y軸ランダムオフセット")]
        private float verticalOffset = 10.0f;

        // 内部状態
        [ShowInInspector, ReadOnly, LabelText("生成タイマー")]
        private float spawnTimer = 0f;

        [ShowInInspector, ReadOnly, LabelText("現在生成数")]
        private int spawnedCount = 0;

        /// <summary>
        /// 更新処理
        /// </summary>
        private void FixedUpdate()
        {
            // 最大スポーン数に達したかチェック
            if (runtimeSpawnInfo.Count > 0 && spawnedCount >= runtimeSpawnInfo.Count)
            {
                FinishSpawning();
                return;
            }

            // スポーンタイマーの更新
            spawnTimer -= Time.fixedDeltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = runtimeSpawnInfo.Interval;
            }
        }

        /// <summary>
        /// スポナーの役割終了
        /// </summary>
        private void FinishSpawning()
        {
            // スポナー自身の削除をWaveControllerに依頼
            if (onRequestRemove != null)
            {
                onRequestRemove(this);
            }
            else
            {
                DevLog.LogWarning(
                    $"{gameObject.name} コールバック未設定につき自身を削除。WaveControllerの設定を確認してください。",
                    LogCategory.Spawn
                );
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 外部からスポーン設定を初期化する（WaveController用）
        /// </summary>
        /// <param name="spawnInfo">スポーン設定情報</param>
        /// <param name="onRemove">スポナー削除依頼用コールバック</param>
        public void Initialize(SpawnInfo spawnInfo, System.Action<EnemySpawner> onRemove = null)
        {
            if (spawnInfo.EnemyDataAsset == null)
            {
                DevLog.LogWarning("EnemySpawner: enemyDataがnullです", LogCategory.Spawn);
                return;
            }

            runtimeSpawnInfo = spawnInfo;
            onRequestRemove = onRemove;
        }

        /// <summary>
        /// 敵を1体スポーン
        /// </summary>
        [Button("敵を1体スポーン", ButtonSizes.Medium), ButtonGroup("SpawnerControls")]
        public void SpawnEnemy()
        {
            if (runtimeSpawnInfo == null)
            {
                DevLog.LogWarning("EnemySpawner: runtimeSpawnInfoがnullです", LogCategory.Spawn);
                return;
            }

            if (runtimeSpawnInfo.EnemyDataAsset == null)
            {
                DevLog.LogWarning("EnemySpawner: enemyDataがnullです", LogCategory.Spawn);
                return;
            }

            // 敵をインスタンス化
            GameObject enemyObj = Instantiate(
                runtimeSpawnInfo.EnemyDataAsset.enemyPrefab,
                transform.position,
                Quaternion.identity
            );

            // SimpleEnemyコンポーネントを取得
            if (enemyObj.TryGetComponent<SimpleEnemy>(out var enemy))
            {
                // パスと移動速度を設定
                enemy.SetPath(pathIndex);
                // 基準移動速度と補正倍率を掛けた値を設定
                float speed =
                    runtimeSpawnInfo.EnemyDataAsset.moveSpeed
                    * runtimeSpawnInfo.MoveSpeedMultiplier;
                enemy.SetMoveSpeed(speed);

                // SplinePathFollowerコンポーネントを取得して設定
                if (enemyObj.TryGetComponent<SplinePathFollower>(out var pathFollower))
                {
                    // パスの設定
                    pathFollower.SetPath(pathIndex);

                    // X軸オフセットをランダムに設定
                    float randomRightOffset = Random.Range(
                        horizontalOffset / 2,
                        -horizontalOffset / 2
                    );
                    pathFollower.RightOffset = randomRightOffset;

                    // Y軸オフセットをランダムに設定
                    float randomUpOffset = Random.Range(verticalOffset / 2, -verticalOffset / 2);
                    pathFollower.UpOffset = randomUpOffset;
                }

                // 敵をアクティブ化
                enemy.Activate();
            }

            spawnedCount++;
        }

        /// <summary>
        /// パスインデックスを設定
        /// </summary>
        public void SetPathIndex(int index)
        {
            pathIndex = index;
        }

        /// <summary>
        /// パスの右方向オフセットの範囲を設定
        /// </summary>
        public void SetPathOffsetRange(float horizontal, float vertical)
        {
            horizontalOffset = horizontal;
            verticalOffset = vertical;
        }
    }
}
