// タレットを配置するための場所やスロットを管理する

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Dread.Battle.Turret
{
    /// <summary>
    /// タレットを配置するための場所やスロットを管理する
    /// </summary>
    public class TurretDeck : MonoBehaviour
    {
        [Header("パラメータ設定")]
        [SerializeField, Required]
        private TurretDeckParameters parameters;

        [Header("タレットスロット設定")]
        [Tooltip("タレットスロットのPrefab")]
        [SerializeField]
        [RequiredIn(PrefabKind.PrefabAsset)]
        private GameObject turretSlotPrefab;

        // タレットのスロットを管理する
        [SerializeField, ReadOnly, LabelText("配下のタレットスロットリスト")]
        private List<TurretSlot> slots = new List<TurretSlot>();

        // 防空範囲内の敵キャッシュ
        [ShowInInspector, ReadOnly, ListDrawerSettings(ShowIndexLabels = true)]
        [LabelText("現在防空範囲内の敵リスト")]
        private readonly List<GameObject> enemiesInDefenseRange = new List<GameObject>();

        private void Awake()
        {
            InitializeTurretSlots();
        }

        /// <summary>
        /// グリッド上の各位置にタレットスロットを生成して初期化
        /// </summary>
        private void InitializeTurretSlots()
        {
            // 既存のスロットをクリア
            ClearExistingSlots();

            // 各グリッド位置にスロットを生成
            for (int x = 0; x < parameters.gridWidth; x++)
            {
                for (int z = 0; z < parameters.gridHeight; z++)
                {
                    CreateTurretSlotAt(x, z);
                }
            }
        }

        /// <summary>
        /// 既存のスロットをすべて削除
        /// </summary>
        private void ClearExistingSlots()
        {
            // 既存のスロットを削除
            foreach (var slot in slots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    // エディタでの実行中と実行中以外で削除方法を分ける
                    if (Application.isPlaying)
                    {
                        Destroy(slot.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(slot.gameObject);
                    }
                }
            }

            // リストをクリア
            slots.Clear();
        }

        /// <summary>
        /// 指定したグリッド位置にタレットスロットを生成
        /// </summary>
        /// <param name="x">横方向のインデックス</param>
        /// <param name="z">縦方向のインデックス</param>
        private void CreateTurretSlotAt(int x, int z)
        {
            if (turretSlotPrefab == null)
            {
                Debug.LogError("タレットスロットのプレハブが設定されていません");
                return;
            }

            // スロットの位置を取得
            Vector3 position = GetCellCenter(x, z);

            // プレハブからインスタンスを生成
            GameObject slotObject = Instantiate(
                turretSlotPrefab,
                position,
                Quaternion.identity,
                transform
            );
            slotObject.name = $"TurretSlot_X{x}_Z{z}";

            // TurretSlotコンポーネントを取得
            if (!slotObject.TryGetComponent(out TurretSlot turretSlot))
            {
                Debug.LogError("プレハブにTurretSlotコンポーネントが含まれていません");
                Destroy(slotObject);
                return;
            }

            // スロットの初期設定
            turretSlot.InitialSetup(this, x, z);

            // リストに追加
            slots.Add(turretSlot);
        }

        /// <summary>
        /// グリッドの中心座標を取得
        /// </summary>
        /// <param name="x">横方向のインデックス（0から始まる）</param>
        /// <param name="z">縦方向のインデックス（0から始まる）</param>
        /// <returns>ワールド座標での中心位置</returns>
        public Vector3 GetCellCenter(int x, int z)
        {
            if (x < 0 || x >= parameters.gridWidth || z < 0 || z >= parameters.gridHeight)
            {
                Debug.LogWarning($"セル座標が範囲外です: ({x}, {z})");
                return Vector3.zero;
            }

            // グリッドの原点（左下）
            Vector3 gridOrigin =
                transform.position
                - new Vector3(
                    (parameters.gridWidth * parameters.cellSize) / 2.0f,
                    0,
                    (parameters.gridHeight * parameters.cellSize) / 2.0f
                );

            // セルの中心に移動
            return gridOrigin
                + new Vector3(
                    (x * parameters.cellSize) + (parameters.cellSize / 2.0f),
                    0,
                    (z * parameters.cellSize) + (parameters.cellSize / 2.0f)
                );
        }

        /// <summary>
        /// エディタでグリッドを可視化
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // 現在の位置を基準にグリッドを描画
            Vector3 position = transform.position;

            // グリッドの原点（左下）
            Vector3 gridOrigin =
                position
                - new Vector3(
                    (parameters.gridWidth * parameters.cellSize) / 2.0f,
                    0,
                    (parameters.gridHeight * parameters.cellSize) / 2.0f
                );

            // グリッドの色を設定
            Gizmos.color = parameters.gridColor;

            // 横方向のライン
            for (int z = 0; z <= parameters.gridHeight; z++)
            {
                Vector3 start = gridOrigin + new Vector3(0, 0, z * parameters.cellSize);
                Vector3 end = start + new Vector3(parameters.gridWidth * parameters.cellSize, 0, 0);
                Gizmos.DrawLine(start, end);
            }

            // 縦方向のライン
            for (int x = 0; x <= parameters.gridWidth; x++)
            {
                Vector3 start = gridOrigin + new Vector3(x * parameters.cellSize, 0, 0);
                Vector3 end =
                    start + new Vector3(0, 0, parameters.gridHeight * parameters.cellSize);
                Gizmos.DrawLine(start, end);
            }

            // セルの中心点を表示
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); // オレンジ色
            for (int x = 0; x < parameters.gridWidth; x++)
            {
                for (int z = 0; z < parameters.gridHeight; z++)
                {
                    Vector3 cellCenter = GetCellCenter(x, z);
                    Gizmos.DrawSphere(cellCenter, 0.1f);
                }
            }

            // 防空範囲を表示
            Gizmos.color = new Color(0.1f, 0.9f, 1f, 0.3f);
            Gizmos.DrawLine(
                transform.position,
                transform.position + transform.forward * parameters.defenseDistance
            );
            Gizmos.DrawSphere(
                transform.position + transform.forward * parameters.defenseDistance,
                parameters.defenseRadius
            );
        }

        /// <summary>
        /// 毎フレーム、防空範囲内の敵をサーチしキャッシュ
        /// </summary>
        private void FixedUpdate()
        {
            SearchEnemiesInDefenseRange();
        }

        /// <summary>
        /// 防空範囲内の敵を検索しキャッシュする
        /// </summary>
        private void SearchEnemiesInDefenseRange()
        {
            enemiesInDefenseRange.Clear();
            var enemies = Character.EnemyController.Instance.GetEnemiesInSphere(
                transform.position + transform.forward * parameters.defenseDistance,
                parameters.defenseRadius
            );
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    enemiesInDefenseRange.Add(enemy.gameObject);
                }
            }
        }

        /// <summary>
        /// 現在キャッシュされている防空範囲内の敵リストを取得
        /// </summary>
        public IReadOnlyList<GameObject> EnemiesInDefenseRange => enemiesInDefenseRange;
    }
}
