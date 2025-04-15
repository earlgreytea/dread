using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Dread.Battle.Turret
{
    /// <summary>
    /// タレットを配置するスロットを表すクラス
    /// </summary>
    public class TurretSlot : MonoBehaviour
    {
        [Header("グリッド情報")]
        [Tooltip("グリッド上の横方向の位置")]
        [SerializeField, ReadOnly]
        private int gridX;

        [Tooltip("グリッド上の縦方向の位置")]
        [SerializeField, ReadOnly]
        private int gridZ;

        [Header("タレット情報")]
        [Tooltip("現在配置されているタレット")]
        [SerializeField]
        private Turret currentTurret;

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
        public Turret CurrentTurret
        {
            get => currentTurret;
            set => currentTurret = value;
        }

        /// <summary>
        /// タレットを配置する
        /// </summary>
        /// <param name="turret">配置するタレット</param>
        /// <returns>配置が成功したかどうか</returns>
        public bool PlaceTurret(Turret turret)
        {
            // 既にタレットが配置されている場合は失敗
            if (currentTurret != null)
            {
                return false;
            }

            // タレットを配置
            currentTurret = turret;
            return true;
        }

        /// <summary>
        /// タレットを撤去する
        /// </summary>
        /// <returns>撤去したタレット、タレットがない場合はnull</returns>
        public Turret RemoveTurret()
        {
            Turret removedTurret = currentTurret;
            currentTurret = null;
            return removedTurret;
        }
    }
}
