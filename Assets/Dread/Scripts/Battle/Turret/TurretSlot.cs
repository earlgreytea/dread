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

        void Start()
        {
            // タレットをテスト配置します
            // テスト配置のデバッグログ
            PlaceTurret(useTestTurretData);
        }

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
            currentTurretLogic.InitialSetup(turretData, turretView);

            return true;
        }

        /// <summary>
        /// タレットを撤去する
        /// </summary>
        /// <returns>撤去したタレット、タレットがない場合はnull</returns>
        public TurretLogic RemoveTurret()
        {
            TurretLogic removedTurret = currentTurretLogic;
            currentTurretLogic = null;
            return removedTurret;
        }
    }
}
