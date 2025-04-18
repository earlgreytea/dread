using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dread.Battle;
using Dread.Battle.Util;
using Sirenix.OdinInspector;

namespace Dread.Battle.UI
{
    /// <summary>
    /// 戦艦の体力表示用UI（旧BattleShipHealthUI）
    /// </summary>
    public class HealthGaugeUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI healthText;

        [SerializeField]
        private Slider healthSlider;

        /// <summary>
        /// インスペクタからアサインする体力情報の参照先（IHealthProviderを実装したMonoBehaviour）
        /// 例：BattleShipやCharacterBaseなど、IHealthProviderを実装しているコンポーネントを指定すること
        /// 注意：interface型はインスペクタでアサインできないため、MonoBehaviour型で受け取る
        /// </summary>
        [SerializeField, LabelText("参照先(オプション)")]
        private MonoBehaviour healthProviderComponent;

        /// <summary>
        /// 実際に体力値を取得するための内部参照（IHealthProviderとして利用）
        /// healthProviderComponentをIHealthProviderにキャストしてセットする
        /// UI更新時はこのhealthProviderから値を取得する
        /// </summary>
        private IHealthProvider healthProvider;

        public void SetHealthProvider(IHealthProvider provider)
        {
            healthProvider = provider;
            UpdateHealthText();
        }

        private void Awake()
        {
            if (healthProviderComponent is IHealthProvider provider)
            {
                healthProvider = provider;
            }
        }

        private void Update()
        {
            if (healthProvider != null)
            {
                UpdateHealthText();
            }
        }

        private void UpdateHealthText()
        {
            healthText.text = $"{healthProvider.CurrentHealth} / {healthProvider.MaxHealth}";
            if (healthSlider != null && healthProvider != null)
            {
                healthSlider.maxValue = healthProvider.MaxHealth;
                healthSlider.value = healthProvider.CurrentHealth;
            }
        }
    }
}
