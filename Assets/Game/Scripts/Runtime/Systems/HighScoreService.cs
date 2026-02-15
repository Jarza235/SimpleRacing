using System;

namespace Game.Runtime
{
    // Stores best time (lower is better).
    // Pure C# logic: depends only on ISaveProvider.
    public sealed class HighScoreService
    {
        private readonly ISaveProvider _save;
        private readonly string _bestTimeKey;

        public float BestTimeSeconds { get; private set; } = float.PositiveInfinity;

        public HighScoreService(ISaveProvider saveProvider, string bestTimeSaveKey)
        {
            _save = saveProvider ?? throw new ArgumentNullException(nameof(saveProvider));

            if (string.IsNullOrWhiteSpace(bestTimeSaveKey)) {
                throw new ArgumentException("Best time save key cannot be null/empty.", nameof(bestTimeSaveKey));
            }

            _bestTimeKey = bestTimeSaveKey;

            Load();
        }

        public void Load()
        {
            BestTimeSeconds = SanitizeTime(_save.GetFloat(_bestTimeKey, float.PositiveInfinity));
        }

        // Submit a finished time. If it's a new record, it gets saved.
        // Returns true if new record was saved.
        public bool TrySubmitTime(float finishedTimeSeconds)
        {
            var time = SanitizeTime(finishedTimeSeconds);

            // No valid time submitted
            if (float.IsPositiveInfinity(time)) {
                return false;
            }

            bool isNewRecord = time < BestTimeSeconds;

            if (isNewRecord)
            {
                BestTimeSeconds = time;
                _save.SetFloat(_bestTimeKey, BestTimeSeconds);
                _save.Save();
            }

            return isNewRecord;
        }

        private static float SanitizeTime(float time)
        {
            if (float.IsNaN(time) || float.IsInfinity(time) || time < 0f)
                return float.PositiveInfinity;

            return time;
        }
    }
}
