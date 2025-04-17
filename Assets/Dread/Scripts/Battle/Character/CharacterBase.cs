using UnityEngine;
using UnityEngine.Events;

namespace Dread.Battle.Character
{
    /// <summary>
    /// ゲーム内のすべてのキャラクター（プレイヤー、敵など）の基底クラス
    /// </summary>
    public abstract class CharacterBase : MonoBehaviour
    {
        [Header("体力パラメータ")]
        [SerializeField]
        protected float maxHealth = 100f;

        [SerializeField]
        protected float currentHealth;

        [SerializeField]
        protected float invincibilityTime = 0.5f;

        // 無敵時間の計測用
        protected float invincibilityTimer = 0f;

        // イベント
        public UnityEvent<float> OnDamaged = new UnityEvent<float>();
        public UnityEvent OnDeath = new UnityEvent();

        /// <summary>
        /// キャラクターが生きているかどうか
        /// </summary>
        public bool IsAlive => currentHealth > 0;

        /// <summary>
        /// キャラクターが現在無敵状態かどうか
        /// </summary>
        public bool IsInvincible => invincibilityTimer > 0;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected virtual void Awake()
        {
            currentHealth = maxHealth;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected virtual void FixedUpdate()
        {
            // 無敵時間の更新
            if (invincibilityTimer > 0)
            {
                invincibilityTimer -= Time.fixedDeltaTime;
            }
        }

        /// <summary>
        /// ダメージを受けるメソッド
        /// </summary>
        public virtual void TakeDamage(float damage)
        {
            // 無敵状態または死亡している場合はダメージを受けない
            if (IsInvincible || !IsAlive)
                return;

            // ダメージを適用
            currentHealth -= damage;

            // ダメージイベントを発火
            OnDamaged.Invoke(damage);

            // 無敵時間を設定
            invincibilityTimer = invincibilityTime;

            // 体力が0以下になった場合は死亡処理
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        protected virtual void Die()
        {
            // 死亡イベントを発火
            OnDeath.Invoke();
        }

        /// <summary>
        /// 体力を回復するメソッド
        /// </summary>
        public virtual void Heal(float amount)
        {
            if (!IsAlive)
                return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }

        /// <summary>
        /// 最大体力を取得するプロパティ
        /// </summary>
        public float MaxHealth => maxHealth;

        /// <summary>
        /// 現在の体力を取得するプロパティ
        /// </summary>
        public float CurrentHealth => currentHealth;
    }
}
