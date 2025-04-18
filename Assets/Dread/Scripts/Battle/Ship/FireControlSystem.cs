using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Dread.Battle.Turret;

namespace Dread.Battle.Ship
{
    /// <summary>
    /// 戦艦の火器管制システム（タレットデッキのホルダー）
    /// </summary>
    public class FireControlSystem : MonoBehaviour
    {
        [Header("搭載タレットデッキ一覧")]
        [SerializeField, Required]
        private List<TurretDeck> turretDecks = new List<TurretDeck>();

        /// <summary>
        /// 管理中のタレットデッキ一覧を取得
        /// </summary>
        public IReadOnlyList<TurretDeck> TurretDecks => turretDecks;
    }
}
