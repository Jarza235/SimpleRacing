using UnityEngine;

namespace Game.Runtime
{
    [CreateAssetMenu(
        fileName = "GameConfig",
        menuName = "Game/Configs/Game Config",
        order = 2)]
    public sealed class GameConfig : ScriptableObject
    {
        [Header("Countdown")]
        [Tooltip("If enabled, the race starts after a countdown.")]
        public bool countdownEnabled = true;

        [Tooltip("Countdown duration in seconds (e.g. 3). Only used if countdownEnabled is true.")]
        [Min(0f)]
        public float countdownSeconds = 3f;

        [Header("Saving")]
        [Tooltip("PlayerPrefs key (or SaveProvider key) for the best time (lower is better).")]
        public string bestTimeSaveKey = "BEST_TIME_SECONDS";

        [Header("Time Format")]
        public TimeFormat timeFormat = TimeFormat.MinutesSecondsMilliseconds;

        [Tooltip("How many millisecond digits to show (1..3). Example: 2 => 12.34")]
        [Range(1, 3)]
        public int millisecondDigits = 2;

        public enum TimeFormat
        {
            Seconds,                     // 12.34
            MinutesSeconds,              // 1:23
            MinutesSecondsMilliseconds   // 1:23.45
        }
    }
}
