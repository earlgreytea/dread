using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProを使う場合
using Dread.Battle.Wave;
using Dread.Battle.Infra;
using Dread.Common;

namespace Dread.Battle.UI.UIParts
{
    public class GameOverPanelUI : MonoBehaviour
    {
        // 前回フレームのHPを記憶
        private int _prevHealth = int.MaxValue;

        // すでにスロー実行済みかどうか
        private bool _isSlowTriggered = false;

        [SerializeField]
        private GameObject panelObject;

        private void Start()
        {
            // 初期HPを記憶
            if (
                BattleStatusManager.Instance != null
                && BattleStatusManager.Instance.PlayerShip != null
            )
            {
                _prevHealth = BattleStatusManager.Instance.PlayerShip.CurrentHealth;
            }
            UpdateGameOverStatus();
        }

        private void Update()
        {
            UpdateGameOverStatus();
        }

        private void UpdateGameOverStatus()
        {
            var playerShip = BattleStatusManager.Instance.PlayerShip;
            if (playerShip == null)
                return;

            int currentHealth = playerShip.CurrentHealth;

            // HPが0になった瞬間のみスローを実行
            if (!_isSlowTriggered && _prevHealth > 0 && currentHealth <= 0)
            {
                // ゲームをスローにする
                if (GameTimeManager.Instance != null)
                {
                    // スローダウンしながらタイムスケール変化
                    GameTimeManager.Instance.StartTimeScaleTransition(0f, 3.0f);
                }
                _isSlowTriggered = true;
            }

            // ゲームオーバーパネル表示
            if (currentHealth <= 0)
            {
                panelObject.SetActive(true);
            }
            else
            {
                panelObject.SetActive(false);
                _isSlowTriggered = false; // リトライやリセット時に再度スロー可能に
            }

            _prevHealth = currentHealth;
        }
    }
}
