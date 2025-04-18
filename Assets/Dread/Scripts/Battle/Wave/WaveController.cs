using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dread.Battle.Spawner;
using Dread.Battle.Character;
using Dread.Common;
using Sirenix.OdinInspector;

namespace Dread.Battle.Wave
{
    // ウェーブ全体コントローラー
    public class WaveController : SingletonMonoBehaviour<WaveController>
    {
        [Header("Wavesシナリオ設定")]
        [SerializeField]
        private WavesScenario wavesScenario;

        // 生成したSpawnerの管理
        private readonly List<EnemySpawner> activeSpawners = new List<EnemySpawner>();

        // インスペクタ表示用インデックス
        [ShowInInspector, ReadOnly, LabelText("ウェーブINDEX")]
        private int waveIndex = -1;

        [SerializeField, LabelText("自動開始")]
        private bool isAutoStart = true;

        private Coroutine waveCoroutine;
        private bool isWaveRunning = false;

        protected override void Awake()
        {
            base.Awake();
            // シナリオバリデーション
            if (wavesScenario != null)
            {
                var warnings = wavesScenario.ValidateScenario();
                if (warnings.Count > 0)
                {
                    foreach (var warn in warnings)
                    {
                        DevLog.LogWarning($"[WavesScenario Validation] {warn}", LogCategory.Spawn);
                    }
                }
            }
            // 自動開始
            if (isAutoStart)
            {
                StartWave();
            }
        }

        public void StartWave()
        {
            if (isWaveRunning)
            {
                DevLog.LogWarning("Waveはすでに実行中です。", LogCategory.Spawn);
                return;
            }
            DevLog.Log($"wavesシナリオ開始: {wavesScenario.name}", LogCategory.Spawn);
            isWaveRunning = true;
            waveCoroutine = StartCoroutine(WaveRoutine());
        }

        private IEnumerator WaveRoutine()
        {
            // 全ウェーブを順番に処理
            for (waveIndex = 0; waveIndex < wavesScenario.Waves.Count; waveIndex++)
            {
                var wave = wavesScenario.Waves[waveIndex];
                DevLog.Log(
                    $"Wave開始: {waveIndex + 1} / {wavesScenario.Waves.Count} (duration: {wave.duration}秒)",
                    LogCategory.Spawn
                );

                // 各SpawnInfoごとにSpawnerを生成
                foreach (var spawnInfo in wave.SpawnInfos)
                {
                    string enemyName =
                        (spawnInfo.EnemyDataAsset != null) ? spawnInfo.EnemyDataAsset.name : "不明な敵";

                    var spawnerName =
                        $"EnemySpawner_Wave{waveIndex}_Spawn{wave.SpawnInfos.IndexOf(spawnInfo)}";

                    DevLog.Log(
                        $"  {spawnerName} 生成: {enemyName} x{spawnInfo.Count}体 間隔:{spawnInfo.Interval}",
                        LogCategory.Spawn
                    );
                    var spawner = new GameObject(spawnerName).AddComponent<EnemySpawner>();
                    spawner.transform.SetParent(transform);
                    spawner.Initialize(spawnInfo, RemoveSpawner);
                    activeSpawners.Add(spawner);
                }

                // duration分だけウェーブを継続
                float elapsed = 0f;
                while (elapsed < wave.duration)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                DevLog.Log($"Wave終了: {waveIndex + 1}", LogCategory.Spawn);
            }
            DevLog.Log($"全ウェーブ終了: {wavesScenario.name}", LogCategory.Spawn);
            // 最終ウェーブを終了してもSpawnerは破棄しない

            activeSpawners.Clear();
            isWaveRunning = false;
            waveCoroutine = null;
        }

        /// <summary>
        /// Spawnerからの削除依頼に応じてリストから除去しDestroy
        /// </summary>
        private void RemoveSpawner(EnemySpawner spawner)
        {
            if (activeSpawners.Contains(spawner))
            {
                activeSpawners.Remove(spawner);
            }
            if (spawner != null)
            {
                Destroy(spawner.gameObject);
            }
        }
    }
}
