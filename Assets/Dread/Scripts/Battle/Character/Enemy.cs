using UnityEngine;
using Sirenix.OdinInspector;
using Dread.Battle.Collision;

namespace Dread.Battle.Character
{
    /// <summary>
    /// 敵キャラクターの基底クラス
    /// </summary>
    public abstract class Enemy : CharacterBase
    {
        [Header("敵の基本パラメータ")]
        [SerializeField]
        protected int scoreValue = 100;

        [SerializeField]
        protected float moveSpeed = 3f;

        [Header("コリジョン設定")]
        [SerializeField, InlineProperty, FoldoutGroup("コリジョン")]
        protected CollisionParameters collisionParameters = new CollisionParameters();

        // 敵の状態
        protected bool isActive = true;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            // EnemyControllerに自身を登録
            if (EnemyController.Instance != null)
            {
                EnemyController.Instance.RegisterEnemy(this);
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
            }
        }

        /// <summary>
        /// 敵の更新処理（派生クラスでオーバーライド）
        /// </summary>
        protected virtual void UpdateEnemy()
        {
            // 派生クラスで実装
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        protected override void Die()
        {
            base.Die();

            // 敵の死亡処理
            isActive = false;

            // スコア加算などの処理はここに実装
            Debug.Log($"敵が倒された: {gameObject.name} (スコア: {scoreValue})");

            // 実際のゲームでは、オブジェクトプールに戻すか、破棄する
            Destroy(gameObject, 1f);
        }

        /// <summary>
        /// オブジェクト破棄時の処理
        /// </summary>
        protected virtual void OnDestroy()
        {
            // EnemyControllerから自身を登録解除
            if (EnemyController.Instance != null)
            {
                EnemyController.Instance.UnregisterEnemy(this);
            }
        }

        /// <summary>
        /// 敵をアクティブ化するメソッド
        /// </summary>
        public virtual void Activate()
        {
            isActive = true;
        }

        /// <summary>
        /// 敵を非アクティブ化するメソッド
        /// </summary>
        public virtual void Deactivate()
        {
            isActive = false;
        }

        /// <summary>
        /// スコア値を取得するプロパティ
        /// </summary>
        public int ScoreValue => scoreValue;

        /// <summary>
        /// コリジョンパラメータを取得するプロパティ
        /// </summary>
        public CollisionParameters CollisionParams => collisionParameters;

        /// <summary>
        /// コリジョンの中心位置を取得
        /// </summary>
        public Vector3 CollisionCenter => transform.position + collisionParameters.offset;

        /// <summary>
        /// コリジョンの半径を取得
        /// </summary>
        public float CollisionRadius => collisionParameters.radius;

        /// <summary>
        /// デバッグ表示用のGizmo描画
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            // コリジョン用の球体を描画
            Gizmos.color = Color.red * 0.8f;
            Gizmos.DrawSphere(CollisionCenter, CollisionRadius);
        }
    }
}
