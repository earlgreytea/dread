using UnityEngine;
using Sirenix.OdinInspector;

namespace Dread.Battle.Turret
{
    [CreateAssetMenu(
        fileName = "TurretDeckParameters",
        menuName = "Dread/Battle/Turret/TurretDeckParameters"
    )]
    public class TurretDeckParameters : ScriptableObject
    {
        [Header("グリッド設定")]
        [Tooltip("グリッドの1マスのサイズ")]
        public float cellSize = 5.0f;

        [Tooltip("グリッドの横方向のマス数")]
        [MinValue(1)]
        public int gridWidth = 2;

        [Tooltip("グリッドの縦方向のマス数")]
        [MinValue(1)]
        public int gridHeight = 3;

        [Header("防空範囲設定")]
        [Tooltip("防空範囲までの距離")]
        public float defenseDistance = 100f;

        [Tooltip("防空範囲の半径")]
        public float defenseRadius = 100f;

        [Header("デバッグ設定")]
        [Tooltip("設置グリッドの色")]
        public Color gridColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);

        [Tooltip("防空範囲の色")]
        public Color defenseRangeColor = new Color(0.1f, 0.9f, 1f, 0.3f);

        // 範囲内の敵ギズモON/OFF
        [ShowInInspector]
        public bool showEnemiesInDefenseRange = false;

        // 防空範囲ギズモON/OFF
        [ShowInInspector]
        public bool showDefenseRange = false;

        // 設置グリッドギズモON/OFF
        [ShowInInspector]
        public bool showGrid = false;
    }
}
