using UnityEngine;
using TMPro;
using Dread.Battle.Infra;

namespace Dread.Battle.UI
{
    /// <summary>
    /// スコア表示用UIスクリプト
    /// </summary>
    public class ScoreTextUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI scoreText;

        private int lastScore = -1;

        private void Update()
        {
            int currentScore = BattleStatusManager.Instance.CurrentScore;
            if (currentScore != lastScore)
            {
                scoreText.text = $"Score: {currentScore}";
                lastScore = currentScore;
            }
        }
    }
}
