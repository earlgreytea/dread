using UnityEngine;
using Sirenix.OdinInspector;

namespace Dread.Battle.Character
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Dread/Enemy Data", order = 0)]
    public class EnemyData : ScriptableObject
    {
        [BoxGroup("基本情報"), LabelText("名称")]
        public string enemyName;

        [BoxGroup("基本情報"), LabelText("説明"), TextArea]
        public string description;

        [BoxGroup("基本情報"), LabelText("アイコン")]
        public Sprite icon;

        [BoxGroup("基本情報"), LabelText("敵Prefab"), PreviewField(75), AssetsOnly]
        public GameObject enemyPrefab;

        [BoxGroup("パラメータ"), LabelText("最大HP")]
        public float maxHP = 100f;

        [BoxGroup("パラメータ"), LabelText("移動速度")]
        public float moveSpeed = 5f;

        [BoxGroup("パラメータ"), LabelText("撃破スコア")]
        public int scoreValue = 10;

        [BoxGroup("パラメータ"), LabelText("撃破報酬")]
        public int rewardValue = 5;

        // 必要に応じて他の敵固有データもここに追加できます
    }
}
