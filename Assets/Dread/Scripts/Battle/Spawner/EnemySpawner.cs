using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dread.Battle.Character;
using Dread.Battle.Path;
using Sirenix.OdinInspector;

namespace Dread.Battle.Spawner
{
    /// <summary>
    /// 敵を定期的に発生させるスポナークラス
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [FoldoutGroup("スポーン設定", expanded: true)]
        [Tooltip("スポーンする敵のプレハブ")]
        [SerializeField, Required]
        private GameObject enemyPrefab;

        [FoldoutGroup("スポーン設定")]
        [Tooltip("スポーン間隔（秒）")]
        [SerializeField, Range(0.1f, 10.0f)]
        private float spawnInterval = 2.0f;

        [FoldoutGroup("スポーン設定")]
        [Tooltip("スポーン時の敵の移動速度")]
        [SerializeField]
        private float enemyMoveSpeed = 50.0f;

        [FoldoutGroup("パス設定", expanded: true)]
        [Tooltip("使用するパスのインデックス（-1でランダム）")]
        [SerializeField]
        private int pathIndex = -1;

        [FoldoutGroup("パス設定")]
        [Tooltip("パスの移動方向を反転するか")]
        [SerializeField, LabelText("移動方向を反転")]
        private bool reverseDirection = false;

        [FoldoutGroup("パス設定")]
        [Tooltip("パスをループするか")]
        [SerializeField, LabelText("ループする")]
        private bool loopPath = true;

        [FoldoutGroup("パスオフセット設定", expanded: true)]
        [Tooltip("パスオフセットを有効にするか")]
        [SerializeField, LabelText("ランダムオフセットを有効化")]
        private bool useRandomOffset = true;

        [FoldoutGroup("パスオフセット設定")]
        [SerializeField]
        private float horizontalOffset = 50.0f;

        [FoldoutGroup("パスオフセット設定")]
        [SerializeField]
        private float verticalOffset = 10.0f;

        [FoldoutGroup("スポナー制御", expanded: true)]
        [Tooltip("起動時に自動的にスポーンを開始するか")]
        [SerializeField, LabelText("自動開始")]
        private bool autoStart = false;

        [FoldoutGroup("スポナー制御")]
        [Tooltip("スポーンする最大敵数（0は無制限）")]
        [SerializeField, Range(0, 100), LabelText("最大敵数")]
        private int maxSpawnCount = 0;

        // 内部状態
        private float spawnTimer = 0f;
        private int spawnedCount = 0;
        private bool isSpawning = false;

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Start()
        {
            if (autoStart)
            {
                StartSpawning();
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void FixedUpdate()
        {
            if (!isSpawning)
                return;

            // 最大スポーン数に達したかチェック
            if (maxSpawnCount > 0 && spawnedCount >= maxSpawnCount)
            {
                StopSpawning();
                return;
            }

            // スポーンタイマーの更新
            spawnTimer -= Time.fixedDeltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                spawnTimer = spawnInterval;
            }
        }

        /// <summary>
        /// 敵のスポーンを開始
        /// </summary>
        [Button("スポーン開始", ButtonSizes.Medium), GUIColor(0, 1, 0), ButtonGroup("SpawnerControls")]
        public void StartSpawning()
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("敵のプレハブが設定されていません。");
                return;
            }

            isSpawning = true;
            spawnTimer = 0f; // すぐに最初の敵をスポーン
        }

        /// <summary>
        /// 敵のスポーンを停止
        /// </summary>
        [Button("スポーン停止", ButtonSizes.Medium), GUIColor(1, 0, 0), ButtonGroup("SpawnerControls")]
        public void StopSpawning()
        {
            isSpawning = false;
        }

        /// <summary>
        /// 敵を1体スポーン
        /// </summary>
        [Button("敵を1体スポーン", ButtonSizes.Medium), ButtonGroup("SpawnerControls")]
        public void SpawnEnemy()
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("敵のプレハブが設定されていません。");
                return;
            }

            // 敵をインスタンス化
            GameObject enemyObj = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

            // SimpleEnemyコンポーネントを取得
            SimpleEnemy enemy = enemyObj.GetComponent<SimpleEnemy>();
            if (enemy != null)
            {
                // パスと移動速度を設定
                enemy.SetPath(pathIndex);
                enemy.SetMoveSpeed(enemyMoveSpeed);

                // SplinePathFollowerコンポーネントを取得して設定
                if (enemyObj.TryGetComponent<SplinePathFollower>(out var pathFollower))
                {
                    // パスの設定
                    pathFollower.SetPath(pathIndex);

                    // 方向とループの設定
                    if (reverseDirection != pathFollower.IsReversed())
                    {
                        pathFollower.ReverseDirection();
                    }

                    // ループ設定の反映（リフレクションを使用）
                    var followerType = pathFollower.GetType();
                    var loopPathField = followerType.GetField("loopPath");
                    if (loopPathField != null)
                        loopPathField.SetValue(pathFollower, loopPath);

                    // ランダムオフセットの設定
                    if (useRandomOffset)
                    {
                        // 右方向オフセットをランダムに設定
                        float randomRightOffset = Random.Range(
                            horizontalOffset / 2,
                            -horizontalOffset / 2
                        );
                        pathFollower.RightOffset = randomRightOffset;

                        // 上方向オフセットをランダムに設定
                        float randomUpOffset = Random.Range(
                            verticalOffset / 2,
                            -verticalOffset / 2
                        );
                        pathFollower.UpOffset = randomUpOffset;
                    }
                }

                // 敵をアクティブ化
                enemy.Activate();
            }

            spawnedCount++;
        }

        /// <summary>
        /// スポーン間隔を設定
        /// </summary>
        public void SetSpawnInterval(float interval)
        {
            spawnInterval = Mathf.Max(0.1f, interval);
        }

        /// <summary>
        /// 敵の移動速度を設定
        /// </summary>
        public void SetEnemyMoveSpeed(float speed)
        {
            enemyMoveSpeed = Mathf.Max(0.1f, speed);
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

        /// <summary>
        /// ランダムオフセットの使用を設定
        /// </summary>
        public void SetUseRandomOffset(bool use)
        {
            useRandomOffset = use;
        }

        /// <summary>
        /// スポーンカウントをリセット
        /// </summary>
        [Button("スポーンカウントリセット", ButtonSizes.Medium), ButtonGroup("SpawnerControls")]
        public void ResetSpawnCount()
        {
            spawnedCount = 0;
        }
    }
}
