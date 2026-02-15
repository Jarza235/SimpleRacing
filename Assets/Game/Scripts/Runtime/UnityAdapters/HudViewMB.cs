using TMPro;
using UnityEngine;

namespace Game.Runtime
{
    public sealed class HudViewMB : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _bestTimeText;
        [SerializeField] private TextMeshProUGUI _countdownText;

        private RaceTimer _raceTimer;
        private RaceFlow _raceFlow;
        private HighScoreService _highScoreService;

        private bool _isInitialized;

        public void Initialize(
            RaceTimer raceTimer,
            RaceFlow raceFlow,
            HighScoreService highScoreService)
        {
            _raceTimer = raceTimer;
            _raceFlow = raceFlow;
            _highScoreService = highScoreService;

            // Setup initial UI state only
            if (_bestTimeText != null && _highScoreService != null)
                SetTimeText(_bestTimeText, _highScoreService.BestTimeSeconds);

            if (_countdownText != null) {
                _countdownText.SetText(string.Empty);
            }

            if (_timeText != null) {
                SetTimeText(_timeText, 0f);
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_raceTimer != null)
                _raceTimer.OnTimeChanged += HandleTimeChanged;

            if (_raceFlow != null)
            {
                _raceFlow.OnCountdownChanged += HandleCountdownChanged;
                _raceFlow.OnFinished += HandleRaceFinished;
            }
        }

        private void Unsubscribe()
        {
            if (_raceTimer != null)
                _raceTimer.OnTimeChanged -= HandleTimeChanged;

            if (_raceFlow != null)
            {
                _raceFlow.OnCountdownChanged -= HandleCountdownChanged;
                _raceFlow.OnFinished -= HandleRaceFinished;
            }
        }

        private void HandleTimeChanged(float seconds)
        {
            if (_timeText == null) return;
            SetTimeText(_timeText, seconds);
        }

        private void HandleCountdownChanged(int remainingSeconds)
        {
            if (_countdownText == null) return;

            if (remainingSeconds <= 0)
            {
                _countdownText.SetText(string.Empty);
                return;
            }

            // Non-alloc
            _countdownText.SetText("{0}", remainingSeconds);
        }

        private void HandleRaceFinished(float finalTimeSeconds, bool isNewHighScore)
        {
            if (_bestTimeText == null || _highScoreService == null) return;

            if(isNewHighScore)
            {
                SetTimeText(_bestTimeText, finalTimeSeconds);
            }
        }

        // Non-alloc time formatting using TMP SetText placeholders.
        private static void SetTimeText(TextMeshProUGUI label, float seconds)
        {
            if (label == null) return;

            if (float.IsNaN(seconds) || float.IsInfinity(seconds))
            {
                label.SetText("--:--");
                return;
            }

            if (seconds < 0f) seconds = 0f;

            int totalSeconds = (int)seconds;
            int minutes = totalSeconds / 60;
            int secs = totalSeconds % 60;

            int hundredths = (int)((seconds - totalSeconds) * 100f);
            if (hundredths < 0) hundredths = 0;
            else if (hundredths > 99) hundredths = 99;

            label.SetText("{0}:{1:00}.{2:00}", minutes, secs, hundredths);
        }
    }
}
