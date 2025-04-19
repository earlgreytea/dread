using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

namespace Dread.Common
{
    /// <summary>
    /// UnityのTime系APIを一元管理するシングルトン。
    /// 独自の時間進行管理は行わず、Time.timeScaleやfixedDeltaTimeの操作のみを提供。
    /// </summary>
    public class GameTimeManager : MonoBehaviour
    {
        public static GameTimeManager Instance { get; private set; }

        // === Odin Inspector用のデバッグ表示 ===
        [ShowInInspector, ReadOnly, LabelText("現在のFPS")]
        public float CurrentFPS => 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);

        [ShowInInspector, ReadOnly, LabelText("UnityのTime.timeScale")]
        public float UnityTimeScale => Time.timeScale;

        [ShowInInspector, ReadOnly, LabelText("Fixed Timestep (Project設定)")]
        public float UnityFixedTimestep => Time.fixedDeltaTime;

        private Coroutine timeScaleTransitionCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// UnityのTime.timeScaleを設定
        /// </summary>
        /// <param name="scale">倍率（1=通常, 0=停止, 0.5=半分, 2=倍速）</param>
        public void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Clamp(scale, 0f, 100f);
        }

        /// <summary>
        /// Time.timeScaleを指定秒数でスムーズに補間する
        /// </summary>
        /// <param name="targetTimeScale">目標TimeScale</param>
        /// <param name="duration">補間にかける秒数</param>
        public void StartTimeScaleTransition(float targetTimeScale, float duration)
        {
            targetTimeScale = Mathf.Clamp(targetTimeScale, 0f, 100f);
            duration = Mathf.Max(0.0001f, duration);
            if (timeScaleTransitionCoroutine != null)
            {
                StopCoroutine(timeScaleTransitionCoroutine);
            }
            DevLog.Log($"GameTimeManager: TimeScaleトランジション開始: {Time.timeScale} → {targetTimeScale} (duration: {duration})", LogCategory.Time);
            timeScaleTransitionCoroutine = StartCoroutine(TimeScaleTransitionCoroutine(targetTimeScale, duration));
        }

        private IEnumerator TimeScaleTransitionCoroutine(float target, float duration)
        {
            float start = Time.timeScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Time.timeScale = Mathf.Lerp(start, target, t);
                yield return null;
            }
            Time.timeScale = target;
            DevLog.Log($"GameTimeManager: TimeScaleトランジション完了: {target}", LogCategory.Time);
            timeScaleTransitionCoroutine = null;
        }

        /// <summary>
        /// UnityのTime.fixedDeltaTimeを設定
        /// </summary>
        /// <param name="dt">FixedDeltaTime値（秒）</param>
        public void SetFixedDeltaTime(float dt)
        {
            Time.fixedDeltaTime = Mathf.Max(0.0001f, dt);
        }

        /// <summary>
        /// ゲームを一時停止（Time.timeScale = 0）
        /// </summary>
        public void Pause()
        {
            Time.timeScale = 0f;
        }

        /// <summary>
        /// ゲームを再開（Time.timeScale = 1）
        /// </summary>
        public void Resume()
        {
            Time.timeScale = 1f;
        }
    }
}
