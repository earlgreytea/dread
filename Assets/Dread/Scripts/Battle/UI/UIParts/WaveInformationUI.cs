using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProを使う場合
using Dread.Battle.Wave;

namespace Dread.Battle.UI.UIParts
{
    public class WaveInformationUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI waveText;

        private void Start()
        {
            UpdateWaveInfo();
        }

        private void Update()
        {
            UpdateWaveInfo();
        }

        private void UpdateWaveInfo()
        {
            // 現在の情報
            var info = WaveController.Instance.CurrentWaveInfo;
            string formatted = string.Format("{0,6:##0.00}", info.RemainingTime);
            waveText.text = $"Wave: {info.WaveNumberText()}\n 次のWaveまで: {formatted} 秒";
        }
    }
}
