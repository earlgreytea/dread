using UnityEngine;
using Dread.Common;

using Sirenix.OdinInspector;

namespace Dread.Battle.Infra
{
    /// <summary>
    /// バトルシーン内でスコアや撃墜数などの状態を管理するシングルトン
    /// </summary>
    public class BattleStatusManager : SingletonMonoBehaviour<BattleStatusManager>
    {
        /// <summary>現在のスコア</summary>
        [ShowInInspector, ReadOnly]
        [PropertyOrder(0)]
        [LabelText("現在のスコア")]
        public int CurrentScore { get; private set; }

        [ShowInInspector, ReadOnly]
        [PropertyOrder(1)]
        [LabelText("撃墜数")]
        public int DefeatedEnemies { get; private set; }

        /// <summary>スコアを加算</summary>
        public void AddScore(int value)
        {
            CurrentScore += value;
        }

        /// <summary>撃墜数を加算</summary>
        public void AddDefeatedEnemy()
        {
            DefeatedEnemies++;
        }

        /// <summary>状態をリセット</summary>
        public void ResetStatus()
        {
            Debug.Log("[BattleStatusManager] 状態をリセットしました。スコア・撃墜数を0に戻します。");
            CurrentScore = 0;
            DefeatedEnemies = 0;
        }
    }
}
