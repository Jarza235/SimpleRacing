using System;

namespace Game.Runtime
{
    // States: Ready -> (Countdown) -> Racing -> Finished. Countdown is used, if it's enabled in GameConfig
    // C# state machine
    public sealed class RaceFlow
    {
        public enum State
        {
            Ready,
            Countdown,
            Racing,
            Finished
        }

        public event Action<State> OnStateChanged;

        // Fired during countdown with WHOLE seconds remaining.
        public event Action<int> OnCountdownChanged;

        // Fired when race is finished. Args: finalTimeSeconds, isNewRecord
        public event Action<float, bool> OnFinished;

        public State CurrentState { get; private set; } = State.Ready;

        // Continuous countdown (seconds). Mainly for internal use / debugging.
        public float CountdownRemaining { get; private set; }

        // What UI should show (seconds remaining, Ceil).
        public int CountdownSecondsDisplay { get; private set; }

        public float FinalTimeSeconds { get; private set; }
        public bool IsNewRecord { get; private set; }

        private readonly GameConfig _config;
        private readonly RaceTimer _timer;
        private readonly HighScoreService _highScore;

        public RaceFlow(GameConfig config, RaceTimer timer, HighScoreService highScore)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));
            _highScore = highScore ?? throw new ArgumentNullException(nameof(highScore));

            GoTo(State.Ready);
        }

        // Call from Unity Update()
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            switch (CurrentState)
            {
                case State.Countdown:
                    TickCountdown(deltaTime);
                    break;

                case State.Racing:
                    _timer.Tick(deltaTime);
                    break;
            }
        }

        // Resets everything back to Ready state
        public void ResetRace()
        {
            _timer.Reset();

            FinalTimeSeconds = 0f;
            IsNewRecord = false;

            CountdownRemaining = 0f;
            CountdownSecondsDisplay = 0;

            GoTo(State.Ready);
        }

        // Starts the race (enters Countdown or Racing depending on config)
        public void StartRace()
        {
            if (CurrentState != State.Ready && CurrentState != State.Finished) {
                return;
            }

            _timer.Reset(); // ensures time = 0 and stopped

            if (_config.countdownEnabled && _config.countdownSeconds > 0f) {
                StartCountdown();
            }
            else {
                BeginRacing();
            }
        }

        // Call when the player hits finish line
        public void FinishRace()
        {
            if (CurrentState != State.Racing) {
                return;
            }

            _timer.Stop();

            FinalTimeSeconds = _timer.ElapsedTime;
            IsNewRecord = _highScore.TrySubmitTime(FinalTimeSeconds);

            GoTo(State.Finished);
            OnFinished?.Invoke(FinalTimeSeconds, IsNewRecord);
        }

        private void StartCountdown()
        {
            CountdownRemaining = _config.countdownSeconds;
            CountdownSecondsDisplay = CeilToIntNonNegative(CountdownRemaining);

            GoTo(State.Countdown);
            OnCountdownChanged?.Invoke(CountdownSecondsDisplay);
        }

        private void TickCountdown(float deltaTime)
        {
            CountdownRemaining -= deltaTime;
            if (CountdownRemaining < 0f) {
                CountdownRemaining = 0f;
            }

            int newDisplay = CeilToIntNonNegative(CountdownRemaining);
            if (newDisplay != CountdownSecondsDisplay)
            {
                CountdownSecondsDisplay = newDisplay;
                OnCountdownChanged?.Invoke(CountdownSecondsDisplay);
            }

            if (CountdownRemaining <= 0f) {
                BeginRacing();
            }
        }

        private void BeginRacing()
        {
            CountdownRemaining = 0f;
            CountdownSecondsDisplay = 0;

            GoTo(State.Racing);
            _timer.Start();
        }

        private void GoTo(State next)
        {
            if (CurrentState == next) {
                return;
            }

            CurrentState = next;
            OnStateChanged?.Invoke(CurrentState);
        }

        private static int CeilToIntNonNegative(float number)
        {
            if (number <= 0f) return 0;

            int intNumber = (int)number;
            return (number > intNumber) ? (intNumber + 1) : intNumber;
        }
    }
}
