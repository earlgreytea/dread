using UnityEngine;
using TMPro;
using Dread.Battle.Infra;

namespace Dread.Battle.UI
{
    /// <summary>
    /// デバッグ用にゲーム中の情報を一時的に表示するパネルUI。
    /// </summary>
    public class MockPanelUI : MonoBehaviour
    {
        [Header("デバッグ表示用テキスト")]
        public TMP_Text debugText;

        /// <summary>
        /// デバッグ情報を表示する。
        /// </summary>
        /// <param name="text">表示するテキスト</param>
        public void SetText(string text)
        {
            if (debugText != null)
            {
                debugText.text = text;
            }
        }

        void Update()
        {
            var currentScore = BattleStatusManager.Instance.CurrentScore;

            SetText($"Score: {currentScore}");
        }
    }
}
