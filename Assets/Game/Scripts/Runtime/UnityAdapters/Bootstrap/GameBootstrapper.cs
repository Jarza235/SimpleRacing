using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Runtime
{
    // Composition root of the game.
    // Wires together config, systems, and Unity adapters.
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [Header("Game config")]
        [SerializeField] private GameConfig _gameConfig;

        [Header("References")]
        [SerializeField] private FinishTriggerMB _finishTrigger;
        [SerializeField] private HudViewMB _hudView;
        [SerializeField] private PlayerInputMB _playerInput;

        [Header("Scene Reload")]
        [SerializeField] private bool _reloadActiveScene = true;
        [SerializeField] private int _reloadBuildIndex = 0;

        private ISaveProvider _saveProvider;

        private RaceTimer _raceTimer;
        private HighScoreService _highScoreService;
        private RaceFlow _raceFlow;

        private bool _initialized;

        private void Awake()
        {
            if (_gameConfig == null)
            {
                Debug.LogError($"{nameof(GameBootstrapper)}: GameConfig not assigned!", this);
                enabled = false;
                return;
            }

            InitSystems();
            InitAdapters();

            _initialized = true;
        }

        private void Start()
        {
            if (_initialized) {
                _raceFlow.StartRace();
            }
        }

        private void Update()
        {
            if (!_initialized) {
                return;
            }

            _raceFlow.Tick(Time.deltaTime);
        }

        private void InitSystems()
        {
            _saveProvider = new PlayerPrefsSaveProvider();

            _raceTimer = new RaceTimer();
            _highScoreService = new HighScoreService(_saveProvider, _gameConfig.bestTimeSaveKey);
            _raceFlow = new RaceFlow(_gameConfig, _raceTimer, _highScoreService);
        }

        private void InitAdapters()
        {
            if (_finishTrigger != null) {
                _finishTrigger.Initialize(_raceFlow);
            }
            else {
                Debug.LogWarning($"{nameof(GameBootstrapper)}: FinishTriggerMB not assigned.", this);
            }

            if (_hudView != null) {
                _hudView.Initialize(_raceTimer, _raceFlow, _highScoreService);
            }
            else {
                Debug.LogWarning($"{nameof(GameBootstrapper)}: HudViewMB not assigned.", this);
            }

            if (_playerInput != null) {
                _playerInput.Initialize(_raceFlow);
            }
            else {
                Debug.LogWarning($"{nameof(GameBootstrapper)}: PlayerInputMB not assigned.", this);
            }
        }

        public void ReloadScene()
        {
            if (_reloadActiveScene)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                SceneManager.LoadScene(_reloadBuildIndex);
            }
        }
    }
}
