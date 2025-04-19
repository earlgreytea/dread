using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Dread.Battle.Character;
using Sirenix.OdinInspector;

namespace Dread.Battle.Wave
{
    // 一連のWaves全体のシナリオ
    [CreateAssetMenu(fileName = "WavesScenario", menuName = "Dread/Waves Scenario", order = 0)]
    public class WavesScenario : ScriptableObject
    {
        /// <summary>
        /// シナリオデータのバリデーション。警告メッセージリストを返す。
        /// </summary>
        public List<string> ValidateScenario()
        {
            var warnings = new List<string>();
            if (Waves == null || Waves.Count == 0)
            {
                warnings.Add("Wavesリストが空です。");
                return warnings;
            }
            for (int i = 0; i < Waves.Count; i++)
            {
                var wave = Waves[i];
                if (wave == null)
                {
                    warnings.Add($"Wave[{i}]がnullです。");
                    continue;
                }
                if (wave.duration <= 0f)
                {
                    warnings.Add($"Wave[{i}]のdurationが0以下です。: {wave.duration}");
                }
                if (wave.SpawnInfos == null || wave.SpawnInfos.Count == 0)
                {
                    warnings.Add($"Wave[{i}]のSpawnInfosリストが空です。");
                    continue;
                }
                for (int j = 0; j < wave.SpawnInfos.Count; j++)
                {
                    var spawn = wave.SpawnInfos[j];
                    if (spawn == null)
                    {
                        warnings.Add($"Wave[{i}]-Spawn[{j}]がnullです。");
                        continue;
                    }
                    if (spawn.Count <= 0)
                    {
                        warnings.Add($"Wave[{i}]-Spawn[{j}]のCountが0以下です。: {spawn.Count}");
                    }
                    if (spawn.Interval < 0f)
                    {
                        warnings.Add($"Wave[{i}]-Spawn[{j}]のIntervalが負の値です。: {spawn.Interval}");
                    }
                }
            }
            return warnings;
        }

        // Waveの内容リスト
        [LabelText("ウェーブリスト")]
        public List<WaveContent> Waves = new List<WaveContent>();

        [ShowInInspector, ReadOnly, LabelText("全ウェーブ合計時間（秒）"), PropertyOrder(-1)]
        public float TotalDuration => Waves != null ? Waves.Sum(w => w.duration) : 0f;

        // 1ウェーブの内容を定義したクラス
        [System.Serializable]
        public class WaveContent
        {
            [LabelText("ウェーブ持続時間（秒）")]
            public float duration = 10f;

            // スポーン情報リスト（１ウェーブで、この数だけSpawnerを生成する）
            [LabelText("スポーン情報リスト")]
            public List<SpawnInfo> SpawnInfos = new List<SpawnInfo>();

            // 1スポーンの内容を定義したクラス
            // スポナーにこれごと渡される
            [System.Serializable]
            [BoxGroup("出現パラメータ")]
            public class SpawnInfo
            {
                [LabelText("敵データアセット")]
                public EnemyData EnemyDataAsset;

                [LabelText("出現数")]
                public int Count = 10;

                [LabelText("間隔（秒）")]
                public float Interval = 0.5f;

                // 移動速度補正倍率（必要になったら復活）
                public float MoveSpeedMultiplier
                {
                    get { return 1f; }
                }
            }
        }
    }
}
