using System.Collections.Generic;
using NUnit.Framework;
using Game.Runtime;

public sealed class RaceFlowTests
{
    private sealed class InMemorySaveProvider : ISaveProvider
    {
        private readonly Dictionary<string, float> _storedFloatValues = new();
        public int SaveCalls { get; private set; }

        public bool HasKey(string key) => _storedFloatValues.ContainsKey(key);

        public float GetFloat(string key, float defaultValue) =>
            _storedFloatValues.TryGetValue(key, out var value) ? value : defaultValue;

        public void SetFloat(string key, float value) => _storedFloatValues[key] = value;

        public void Save() => SaveCalls++;
    }

    private static void Advance(RaceFlow flow, float seconds, float step = 0.1f)
    {
        while (seconds > 0f)
        {
            float dt = seconds < step ? seconds : step;
            flow.Tick(dt);
            seconds -= dt;
        }
    }

    private static GameConfig MakeConfig(bool countdownEnabled, float countdownSeconds, string key = "BEST")
    {
        var gameConfig = UnityEngine.ScriptableObject.CreateInstance<GameConfig>();
        gameConfig.countdownEnabled = countdownEnabled;
        gameConfig.countdownSeconds = countdownSeconds;
        gameConfig.bestTimeSaveKey = key;
        return gameConfig;
    }

    [Test]
    public void StartRace_WhenCountdownEnabled_EntersCountdownThenRacing()
    {
        var save = new InMemorySaveProvider();
        var gameConfig = MakeConfig(countdownEnabled: true, countdownSeconds: 1.0f);
        var raceTimer = new RaceTimer();
        var highscoreService = new HighScoreService(save, gameConfig.bestTimeSaveKey);
        var raceFlow = new RaceFlow(gameConfig, raceTimer, highscoreService);

        Assert.AreEqual(RaceFlow.State.Ready, raceFlow.CurrentState);

        raceFlow.StartRace();
        Assert.AreEqual(RaceFlow.State.Countdown, raceFlow.CurrentState);
        Assert.IsFalse(raceTimer.IsRunning);
        Assert.Greater(raceFlow.CountdownRemaining, 0f);

        // After full countdown time, should enter Racing and start timer.
        Advance(raceFlow, 1.0f);

        Assert.AreEqual(RaceFlow.State.Racing, raceFlow.CurrentState);
        Assert.IsTrue(raceTimer.IsRunning);
        Assert.AreEqual(0f, raceFlow.CountdownRemaining, 0.0001f);
    }

    [Test]
    public void StartRace_WhenCountdownDisabled_GoesStraightToRacing()
    {
        var save = new InMemorySaveProvider();
        var gameConfig = MakeConfig(countdownEnabled: false, countdownSeconds: 3.0f);
        var raceTimer = new RaceTimer();
        var highScoreService = new HighScoreService(save, gameConfig.bestTimeSaveKey);
        var raceFlow = new RaceFlow(gameConfig, raceTimer, highScoreService);

        raceFlow.StartRace();

        Assert.AreEqual(RaceFlow.State.Racing, raceFlow.CurrentState);
        Assert.IsTrue(raceTimer.IsRunning);
        Assert.AreEqual(0f, raceFlow.CountdownRemaining, 0.0001f);
    }

    [Test]
    public void Tick_WhenRacing_IncreasesElapsedTime()
    {
        var save = new InMemorySaveProvider();
        var gameConfig = MakeConfig(countdownEnabled: false, countdownSeconds: 0f);
        var raceTimer = new RaceTimer();
        var highScoreService = new HighScoreService(save, gameConfig.bestTimeSaveKey);
        var raceFlow = new RaceFlow(gameConfig, raceTimer, highScoreService);

        raceFlow.StartRace();
        Advance(raceFlow, 2.0f, step: 0.2f);

        Assert.AreEqual(RaceFlow.State.Racing, raceFlow.CurrentState);
        Assert.AreEqual(2.0f, raceTimer.ElapsedTime, 0.0001f);
    }

    [Test]
    public void FinishRace_WhenNotRacing_DoesNothing()
    {
        var save = new InMemorySaveProvider();
        var gameConfig = MakeConfig(countdownEnabled: true, countdownSeconds: 2.0f);
        var raceTimer = new RaceTimer();
        var highScoreService = new HighScoreService(save, gameConfig.bestTimeSaveKey);
        var raceFlow = new RaceFlow(gameConfig, raceTimer, highScoreService);

        // Ready -> Finish should do nothing
        raceFlow.FinishRace();
        Assert.AreEqual(RaceFlow.State.Ready, raceFlow.CurrentState);

        // Countdown -> Finish should do nothing
        raceFlow.StartRace();
        Assert.AreEqual(RaceFlow.State.Countdown, raceFlow.CurrentState);
        raceFlow.FinishRace();
        Assert.AreEqual(RaceFlow.State.Countdown, raceFlow.CurrentState);
    }

    [Test]
    public void FinishRace_WhenWorseThanExistingRecord_DoesNotOverwrite()
    {
        var save = new InMemorySaveProvider();
        save.SetFloat("BEST", 1.0f); // existing best
        var gameConfig = MakeConfig(countdownEnabled: false, countdownSeconds: 0f, key: "BEST");
        var raceTimer = new RaceTimer();
        var highScoreService = new HighScoreService(save, gameConfig.bestTimeSaveKey);
        var raceFlow = new RaceFlow(gameConfig, raceTimer, highScoreService);

        raceFlow.StartRace();
        Advance(raceFlow, 2.0f, step: 0.1f);
        raceFlow.FinishRace();

        Assert.AreEqual(RaceFlow.State.Finished, raceFlow.CurrentState);
        Assert.IsFalse(raceFlow.IsNewRecord);

        // Should not save again
        Assert.AreEqual(0, save.SaveCalls);
        Assert.AreEqual(1.0f, save.GetFloat("BEST", -1f), 0.0001f);
        Assert.AreEqual(1.0f, highScoreService.BestTimeSeconds, 0.0001f);
    }

    [Test]
    public void ResetRace_ReturnsToReady_AndClearsFlags()
    {
        var save = new InMemorySaveProvider();
        var gameConfig = MakeConfig(countdownEnabled: false, countdownSeconds: 0f);
        var timer = new RaceTimer();
        var highScoreService = new HighScoreService(save, gameConfig.bestTimeSaveKey);
        var raceFlow = new RaceFlow(gameConfig, timer, highScoreService);

        raceFlow.StartRace();
        Advance(raceFlow, 0.5f);
        raceFlow.FinishRace();

        Assert.AreEqual(RaceFlow.State.Finished, raceFlow.CurrentState);

        raceFlow.ResetRace();

        Assert.AreEqual(RaceFlow.State.Ready, raceFlow.CurrentState);
        Assert.IsFalse(timer.IsRunning);
        Assert.AreEqual(0f, timer.ElapsedTime, 0.0001f);
        Assert.AreEqual(0f, raceFlow.FinalTimeSeconds, 0.0001f);
        Assert.IsFalse(raceFlow.IsNewRecord);
        Assert.AreEqual(0f, raceFlow.CountdownRemaining, 0.0001f);
    }

    [Test]
    public void StartRace_FromFinished_AllowsRestart()
    {
        var save = new InMemorySaveProvider();
        var gameConfig = MakeConfig(countdownEnabled: false, countdownSeconds: 0f);
        var timer = new RaceTimer();
        var highScoreService = new HighScoreService(save, gameConfig.bestTimeSaveKey);
        var raceFlow = new RaceFlow(gameConfig, timer, highScoreService);

        raceFlow.StartRace();
        Advance(raceFlow, 0.2f);
        raceFlow.FinishRace();

        Assert.AreEqual(RaceFlow.State.Finished, raceFlow.CurrentState);

        raceFlow.StartRace();
        Assert.AreEqual(RaceFlow.State.Racing, raceFlow.CurrentState);
        Assert.IsTrue(timer.IsRunning);
        Assert.AreEqual(0f, timer.ElapsedTime, 0.0001f);
    }
}
