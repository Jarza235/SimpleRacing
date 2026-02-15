using UnityEngine;

namespace Game.Runtime
{
    public sealed class PlayerInputMB : MonoBehaviour
    {
        [SerializeField] private CarDriver2D _car;
        private RaceFlow _raceFlow;

        public void Initialize(RaceFlow raceFlow)
        {
            _raceFlow = raceFlow;
        }

        private void Update()
        {
            float throttle = 0f;

            if (_raceFlow != null && _raceFlow.CurrentState == RaceFlow.State.Racing)
            {
                throttle = -Input.GetAxis("Horizontal");
            }

            _car.SetThrottle(throttle);
        }
    }
}