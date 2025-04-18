using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Dread.Battle.Turret
{
    /// <summary>
    /// タレットを配置するスロットを表すクラス
    /// </summary>
    public class TurretSlot : MonoBehaviour
    {
        /// <summary>
        /// タレットを配置するスロットを押すと呼び出されます
        /// </summary>
        private void OnMouseDown()
        {
            Debug.LogWarning($"TurretSlot {GridX}, {GridZ} クリック！{turretDeck.name}");
            // タレットを配置します
            PlaceTurret(useTestTurretData);
        }

        private void OnMouseOver()
        {
            Debug.LogWarning($"TurretSlot {GridX}, {GridZ} マウスオーバー {turretDeck.name}");
        }

        private void OnMouseEnter()
        {
            clickableBoxRenderer.material.SetColor("_EmissionColor", hoverColor);
        }

        private void OnMouseExit()
        {
            clickableBoxRenderer.material.SetColor("_EmissionColor", defaultColor);
        }

        [SerializeField]
        private MeshRenderer clickableBoxRenderer;

        [SerializeField, ColorUsage(false, true)]
        private Color defaultColor;

        [SerializeField, ColorUsage(false, true)]
        private Color hoverColor;

        [Header("グリッド情報")]
        [SerializeField, ReadOnly]
        private int gridX;

        [SerializeField, ReadOnly]
        private int gridZ;

        [Header("タレット情報")]
        [SerializeField, ReadOnly, LabelText("設置中のタレット(logic)")]
        private TurretLogic currentTurretLogic;

        [SerializeField, ReadOnly, LabelText("親タレットデッキ")]
        private TurretDeck turretDeck;

        /// <summary>
        /// 挙動の確認用に一時的に参照するタレットデータ
        /// </summary>
        [LabelText("テスト用参照！！")]
        public TurretData useTestTurretData;

        /// <summary>
        /// グリッド上の横方向の位置
        /// </summary>
        public int GridX
        {
            get => gridX;
            set => gridX = value;
        }

        /// <summary>
        /// グリッド上の縦方向の位置
        /// </summary>
        public int GridZ
        {
            get => gridZ;
            set => gridZ = value;
        }

        /// <summary>
        /// 現在配置されているタレット
        /// </summary>
        public TurretLogic CurrentTurret
        {
            get => currentTurretLogic;
            set => currentTurretLogic = value;
        }

        /// <summary>
        /// 初期設定
        /// </summary>
        /// <param name="turretDeck"></param>
        /// <param name="x"></param>
        /// <param name="z"></param>
        internal void InitialSetup(TurretDeck turretDeck, int x, int z)
        {
            GridX = x;
            GridZ = z;
            this.turretDeck = turretDeck;
        }

        void Start() { }

        /// <summary>
        /// タレットを配置する
        /// </summary>
        /// <param name="turret">配置するタレット</param>
        /// <returns>配置が成功したかどうか</returns>
        public bool PlaceTurret(TurretData turretData)
        {
            // 既にタレットが配置されている場合は失敗
            if (currentTurretLogic != null)
            {
                // エラーログ
                Debug.LogError("Turret is already placed.");
                return false;
            }

            // 配下にTurretView生成
            var turret = Instantiate(turretData.turretViewPrefab, transform);
            // TurretViewの取得
            var turretView = turret.GetComponent<TurretView>();
            // TurretLogicを付与
            currentTurretLogic = turret.AddComponent<TurretLogic>();
            // TurretLogicのセットアップ
            currentTurretLogic.InitialSetup(turretData, turretView, turretDeck);

            return true;
        }

        /// <summary>
        /// タレットを撤去する
        /// </summary>
        /// <returns>撤去したタレット、タレットがない場合はnull</returns>
        public bool RemoveTurret()
        {
            // タレットがない場合は失敗
            if (currentTurretLogic == null)
                return false;
            // オブジェクトの破棄
            Destroy(currentTurretLogic.gameObject);
            currentTurretLogic = null;
            return true;
        }
    }
}
