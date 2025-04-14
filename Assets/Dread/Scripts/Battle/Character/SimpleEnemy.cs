using UnityEngine;
using Dread.Battle.Path;
using UnityEngine.Splines;
using Dread.Battle.Fx;

namespace Dread.Battle.Character
{
    /// <summary>
    /// 基本的な敵キャラクターの実装クラス
    /// </summary>
    public class SimpleEnemy : Enemy
    {
        [Header("攻撃パラメータ")]
        /*
        [SerializeField]
        private float attackRange = 5f;
*/
        [SerializeField]
        private float attackCooldown = 2f;

        [Header("移動設定")]
        [SerializeField]
        private bool useSplinePath = true;

        [SerializeField]
        private int pathIndex = -1; // -1の場合はランダム選択

        [SerializeField]
        private bool reverseDirection = false;

        [SerializeField]
        private bool loopPath = true;

        [SerializeField]
        private bool lookForward = true;

        [SerializeField]
        private float rotationSpeed = 10f;

        // 内部状態
        private float attackTimer = 0f;
        private SplinePathFollower pathFollower;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            attackTimer = Random.Range(0f, attackCooldown * 0.5f); // ランダムな初期タイマー

            // スプラインパスフォロワーの設定
            if (useSplinePath)
            {
                pathFollower = GetComponent<SplinePathFollower>();
                if (pathFollower == null)
                {
                    pathFollower = gameObject.AddComponent<SplinePathFollower>();
                }

                // パラメータの設定
                pathFollower.MoveSpeed = moveSpeed;

                // パスフォロワーのプロパティを設定
                var followerType = pathFollower.GetType();
                var pathIndexField = followerType.GetField("pathIndex");
                if (pathIndexField != null)
                    pathIndexField.SetValue(pathFollower, pathIndex);

                var reverseDirectionField = followerType.GetField("reverseDirection");
                if (reverseDirectionField != null)
                    reverseDirectionField.SetValue(pathFollower, reverseDirection);

                var loopPathField = followerType.GetField("loopPath");
                if (loopPathField != null)
                    loopPathField.SetValue(pathFollower, loopPath);

                var lookForwardField = followerType.GetField("lookForward");
                if (lookForwardField != null)
                    lookForwardField.SetValue(pathFollower, lookForward);

                var rotationSpeedField = followerType.GetField("rotationSpeed");
                if (rotationSpeedField != null)
                    rotationSpeedField.SetValue(pathFollower, rotationSpeed);
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void Update()
        {
            base.Update();

            if (isActive && IsAlive)
            {
                // 敵の更新処理
                UpdateEnemy();

                // パスの終端に到達したかチェック
                CheckPathEnd();
            }
        }

        /// <summary>
        /// 敵の更新処理
        /// </summary>
        protected override void UpdateEnemy()
        {
            // 攻撃タイマーの更新
            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }

            // スプラインパスを使用しない場合の移動処理
            if (!useSplinePath || pathFollower == null)
            {
                // 従来の移動処理をここに実装
                // 例えば、直線移動や単純なAIによる移動など
            }

            // パスフォロワーの速度を更新（moveSpeedが変更された場合に対応）
            if (pathFollower != null)
            {
                pathFollower.MoveSpeed = moveSpeed;
            }

            // ここに攻撃ロジックを実装
            TryAttack();
        }

        /// <summary>
        /// 攻撃を試みる
        /// </summary>
        private void TryAttack()
        {
            if (attackTimer <= 0)
            {
                // 攻撃範囲内にプレイヤーがいるか確認するロジック
                // 実際のゲームでは、ここにプレイヤー検出と攻撃処理を実装

                // 攻撃タイマーをリセット
                attackTimer = attackCooldown;
            }
        }

        /// <summary>
        /// 敵をアクティブ化するメソッド
        /// </summary>
        public override void Activate()
        {
            base.Activate();

            // パスフォロワーを開始
            if (useSplinePath && pathFollower != null)
            {
                pathFollower.InitializePath();
                pathFollower.StartMoving();
            }
        }

        /// <summary>
        /// 敵を非アクティブ化するメソッド
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();

            // パスフォロワーを停止
            if (pathFollower != null)
            {
                pathFollower.StopMoving();
            }
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        protected override void Die()
        {
            base.Die();

            // 移動速度ベクトルを取得
            Vector3 velocityVector = GetVelocity();

            // パスフォロワーを停止
            if (pathFollower != null)
            {
                pathFollower.StopMoving();
            }

            // SimpleEnemy固有の死亡処理
            // 爆発エフェクトの再生（移動速度ベクトルを渡す）
            FxEmitter.Instance.EmitByType(
                FxType.ExplosionFlash,
                transform.position,
                velocityVector,
                3
            );

            // 敵を即座に破棄
            Destroy(gameObject);
        }

        /// <summary>
        /// 移動速度を設定
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
            if (pathFollower != null)
            {
                pathFollower.MoveSpeed = speed;
            }
        }

        /// <summary>
        /// パスの終端に到達したかチェックし、到達していれば消滅させる
        /// </summary>
        private void CheckPathEnd()
        {
            if (pathFollower != null && pathFollower.HasReachedEnd)
            {
                // パスの終端に到達したら消滅
                Deactivate();

                // 敵オブジェクトを完全に破棄
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 敵の現在の移動速度ベクトルを取得
        /// </summary>
        /// <returns>移動速度ベクトル</returns>
        private Vector3 GetVelocity()
        {
            // パスフォロワーがある場合は、その進行方向と速度から計算
            if (pathFollower != null && pathFollower.IsMoving)
            {
                // 進行方向（前方ベクトル）に速度を掛けて速度ベクトルを作成
                return transform.forward * pathFollower.MoveSpeed;
            }

            // パスフォロワーがない場合や移動していない場合は前方向を返す
            return transform.forward;
        }

        public void SetPath(int newPathIndex, bool resetPosition = true)
        {
            pathIndex = newPathIndex;
            if (pathFollower != null)
            {
                pathFollower.SetPath(newPathIndex, resetPosition);
            }
        }
    }
}
