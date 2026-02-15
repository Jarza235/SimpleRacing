using System.Collections.Generic;
using NUnit.Framework;
using Game.Runtime;

public sealed class HighScoreServiceTests
{
    private sealed class FakeSaveProvider : ISaveProvider
    {
        private readonly Dictionary<string, float> _savedValuesByKey = new();
        public int SaveCalls { get; private set; }

        public bool HasKey(string key) => _savedValuesByKey.ContainsKey(key);

        public float GetFloat(string key, float defaultValue) =>
            _savedValuesByKey.TryGetValue(key, out var v) ? v : defaultValue;

        public void SetFloat(string key, float value) => _savedValuesByKey[key] = value;

        public void Save() => SaveCalls++;
    }

    [Test]
    public void Constructor_WhenNoKeyExists_BestTimeIsInfinity()
    {
        var save = new FakeSaveProvider();

        var service = new HighScoreService(save, "BEST");

        Assert.AreEqual(float.PositiveInfinity, service.BestTimeSeconds);
    }

    [Test]
    public void Constructor_WhenKeyExists_LoadsBestTime()
    {
        var save = new FakeSaveProvider();
        save.SetFloat("BEST", 12.5f);

        var service = new HighScoreService(save, "BEST");

        Assert.AreEqual(12.5f, service.BestTimeSeconds, 0.0001f);
    }

    [Test]
    public void TrySubmitTime_WhenNoRecord_SavesAndReturnsTrue()
    {
        var save = new FakeSaveProvider();
        var service = new HighScoreService(save, "BEST");

        bool isNew = service.TrySubmitTime(10f);

        Assert.IsTrue(isNew);
        Assert.AreEqual(10f, service.BestTimeSeconds, 0.0001f);
        Assert.IsTrue(save.HasKey("BEST"));
        Assert.AreEqual(1, save.SaveCalls);
        Assert.AreEqual(10f, save.GetFloat("BEST", -1f), 0.0001f);
    }

    [Test]
    public void TrySubmitTime_WhenTimeIsWorse_DoesNotSaveAndReturnsFalse()
    {
        var save = new FakeSaveProvider();
        save.SetFloat("BEST", 10f);
        var service = new HighScoreService(save, "BEST");

        bool isNew = service.TrySubmitTime(12f);

        Assert.IsFalse(isNew);
        Assert.AreEqual(10f, service.BestTimeSeconds, 0.0001f);
        Assert.AreEqual(0, save.SaveCalls);
        Assert.AreEqual(10f, save.GetFloat("BEST", -1f), 0.0001f);
    }

    [Test]
    public void TrySubmitTime_WhenTimeIsBetter_SavesAndReturnsTrue()
    {
        var save = new FakeSaveProvider();
        save.SetFloat("BEST", 10f);
        var service = new HighScoreService(save, "BEST");

        bool isNew = service.TrySubmitTime(9.5f);

        Assert.IsTrue(isNew);
        Assert.AreEqual(9.5f, service.BestTimeSeconds, 0.0001f);
        Assert.AreEqual(1, save.SaveCalls);
        Assert.AreEqual(9.5f, save.GetFloat("BEST", -1f), 0.0001f);
    }

    [Test]
    public void TrySubmitTime_WhenInvalidTime_DoesNotSaveAndReturnsFalse()
    {
        var save = new FakeSaveProvider();
        var service = new HighScoreService(save, "BEST");

        Assert.IsFalse(service.TrySubmitTime(-1f));
        Assert.IsFalse(service.TrySubmitTime(float.NaN));
        Assert.IsFalse(service.TrySubmitTime(float.PositiveInfinity));

        Assert.AreEqual(float.PositiveInfinity, service.BestTimeSeconds);
        Assert.AreEqual(0, save.SaveCalls);
        Assert.IsFalse(save.HasKey("BEST"));
    }

    [Test]
    public void Load_WhenStoredTimeIsInvalid_SetsInfinity()
    {
        var save = new FakeSaveProvider();
        save.SetFloat("BEST", float.NaN);

        var service = new HighScoreService(save, "BEST");

        Assert.AreEqual(float.PositiveInfinity, service.BestTimeSeconds);
    }
}
