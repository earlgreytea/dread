using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

namespace Dread.Common
{
    /// <summary>
    /// ゲーム全体の時間進行を管理するシングルトン。スロー、停止、加速などに対応。
    /// </summary>
    public class GameTimeManager : MonoBehaviour
    {
        public static GameTimeManager Instance { get; private set; }

        [Tooltip("現在のゲーム内時間倍率 (1=通常, 0=停止, 0.5=半分, 2=倍速)")]
        [ShowInInspector, ReadOnly]
        public float TimeScale { get; private set; } = 1f;

        private float timeScaleTimer = 0f;
        private float defaultTimeScale = 1f;
        private bool isPaused = false;

        // === Odin Inspector用のデバッグ表示 ===
        [ShowInInspector, ReadOnly, LabelText("現在のFPS")]
        public float CurrentFPS => 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);

        [ShowInInspector, ReadOnly, LabelText("UnityのTime.timeScale")]
        public float UnityTimeScale => Time.timeScale;

        [ShowInInspector, ReadOnly, LabelText("Fixed Timestep (Project設定)")]
        public float UnityFixedTimestep => Time.fixedDeltaTime;

        [ShowInInspector, ReadOnly, LabelText("GameTimeManagerのFixedDeltaTime")]
        public float ManagedFixedDeltaTime => FixedDeltaTime;

        /// <summary>
        /// スケール済みのdeltaTime（Update相当）
        /// </summary>
        public float DeltaTime => isPaused ? 0f : Time.deltaTime * TimeScale;

        /// <summary>
        /// スケール済みのfixedDeltaTime（FixedUpdate相当）
        /// </summary>
        public float FixedDeltaTime => isPaused ? 0f : Time.fixedDeltaTime * TimeScale;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (isPaused)
                return;
            if (timeScaleTimer > 0f)
            {
                timeScaleTimer -= Time.unscaledDeltaTime;
                if (timeScaleTimer <= 0f)
                {
                    SetTimeScale(defaultTimeScale);
                }
            }
        }

        /// <summary>
        /// 時間倍率を設定（一時的なスローや加速も可）
        /// </summary>
        /// <param name="scale">倍率（1=通常, 0=停止, 0.5=半分, 2=倍速）</param>
        /// <param name="duration">0なら永続、0より大きい場合はその秒数だけ適用</param>
        public void SetTimeScale(float scale, float duration = 0f)
        {
            TimeScale = Mathf.Clamp(scale, 0f, 100f);
            if (duration > 0f)
            {
                timeScaleTimer = duration;
            }
            else
            {
                timeScaleTimer = 0f;
            }
        }

        /// <summary>
        /// ゲームを一時停止
        /// </summary>
        public void Pause()
        {
            isPaused = true;
        }

        /// <summary>
        /// ゲームの一時停止を解除
        /// </summary>
        public void Resume()
        {
            isPaused = false;
        }

        /// <summary>
        /// 現在一時停止中かどうか
        /// </summary>
        public bool IsPaused => isPaused;
    }
}
