using Dread.Common;
using UnityEngine;

namespace Dread.Battle.UI
{
    public class BattleUIManager : SingletonMonoBehaviour<BattleUIManager>
    {
        [SerializeField]
        private Camera uiCamera;
        public Camera UICamera => uiCamera;

        // 必要に応じてUI管理用のメソッドを追加
    }
}
