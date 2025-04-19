using UnityEngine;

namespace Dread.Battle.Wave
{
    /// <summary>
    /// ウェーブ情報をまとめて管理するクラス
    /// </summary>
    public class WaveInfo
    {
        /// <summary>現在のウェーブ番号（0始まり）</summary>
        public int CurrentWaveIndex { get; }

        /// <summary>最大ウェーブ数</summary>
        public int MaxWaveCount { get; }

        /// <summary>残り時間（秒）</summary>
        public float RemainingTime { get; }

        public WaveInfo(int currentWaveIndex, int maxWaveCount, float remainingTime)
        {
            CurrentWaveIndex = currentWaveIndex;
            MaxWaveCount = maxWaveCount;
            RemainingTime = remainingTime;
        }

        public string WaveNumberText()
        {
            var current = CurrentWaveIndex + 1;
            var max = MaxWaveCount;
            // 最大ウェーブ数を超えた場合は最大値を返す
            if (current > max)
                current = max;
            return $"{current} / {max}";
        }
    }
}
