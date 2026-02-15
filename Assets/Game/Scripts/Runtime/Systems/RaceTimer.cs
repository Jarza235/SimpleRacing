using System;

namespace Game.Runtime
{
    // Plain C# race timer. Call Tick(dt) from a MonoBehaviour (e.g., Bootstrapper.Update).
    public sealed class RaceTimer
    {
        public event Action<float> OnTimeChanged;
        public bool IsRunning { get; private set; }
        public float ElapsedTime { get; private set; }

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Reset()
        {
            IsRunning = false;
            SetElapsed(0f, notify: true);
        }

        public void Tick(float dt)
        {
            if (!IsRunning) return;
            if (dt <= 0f) return;

            SetElapsed(ElapsedTime + dt, notify: true);
        }

        private void SetElapsed(float value, bool notify)
        {
            ElapsedTime = value;
            if (notify) {
                OnTimeChanged?.Invoke(ElapsedTime);
            }
        }
    }
}
