using UnityEngine;

namespace Game.Runtime
{
    public sealed class FinishTriggerMB : MonoBehaviour
    {
        private RaceFlow _raceFlow;

        public void Initialize(RaceFlow raceFlow)
        {
            _raceFlow = raceFlow;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_raceFlow == null) return;

            if (other.CompareTag("Player"))
            {
                _raceFlow.FinishRace();
            }
        }
    }
}