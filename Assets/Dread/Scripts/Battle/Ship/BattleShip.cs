using UnityEngine;
using Sirenix.OdinInspector;
using Dread.Battle.Turret;
using Dread.Battle.Util;

namespace Dread.Battle.Ship
{
    /// <summary>
    /// 戦艦本体を制御するクラス
    /// </summary>
    public class BattleShip : MonoBehaviour, IHealthProvider
    {
        [Header("火器管制システム")]
        [SerializeField, Required]
        private FireControlSystem fireControlSystem;

        [Header("戦艦ステータス")]
        [SerializeField, MinValue(1)]
        private int maxHealth = 1000;

        [SerializeField, MinValue(0)]
        private int currentHealth = 1000;

        [SerializeField, MinValue(0)]
        private int maxEnergy = 500;

        [SerializeField, MinValue(0)]
        private int energy = 500;

        /// <summary>
        /// 火器管制システムへの参照を取得
        /// </summary>
        public FireControlSystem FireControlSystem => fireControlSystem;

        /// <summary>
        /// 現在の体力を取得
        /// </summary>
        public int CurrentHealth => currentHealth;

        /// <summary>
        /// 最大体力を取得
        /// </summary>
        public int MaxHealth => maxHealth;

        /// <summary>
        /// 現在のエネルギーを取得
        /// </summary>
        public int Energy => energy;

        /// <summary>
        /// 最大エネルギーを取得
        /// </summary>
        public int MaxEnergy => maxEnergy;

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="amount">ダメージ量</param>
        public void TakeDamage(int amount)
        {
            currentHealth = Mathf.Max(currentHealth - amount, 0);
            // TODO: 死亡処理やエフェクト
        }

        /// <summary>
        /// エネルギーを消費する
        /// </summary>
        /// <param name="amount">消費量</param>
        /// <returns>消費できたか</returns>
        public bool ConsumeEnergy(int amount)
        {
            if (energy < amount)
                return false;
            energy -= amount;
            return true;
        }

        /// <summary>
        /// 体力・エネルギーを回復する
        /// </summary>
        public void Recover(int healthAmount, int energyAmount)
        {
            currentHealth = Mathf.Min(currentHealth + healthAmount, maxHealth);
            energy = Mathf.Min(energy + energyAmount, maxEnergy);
        }

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            energy = Mathf.Clamp(energy, 0, maxEnergy);
        }
    }
}
