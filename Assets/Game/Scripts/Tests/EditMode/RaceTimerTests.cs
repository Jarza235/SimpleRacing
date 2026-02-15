using NUnit.Framework;
using Game.Runtime;

public sealed class RaceTimerTests
{
    // Naming convention: MethodName_State_ExpectedResult

    [Test]
    public void NewTimer_Stopped_TimeDoesNotIncrease()
    {
        var timer = new RaceTimer();

        Assert.IsFalse(timer.IsRunning);
        Assert.AreEqual(0f, timer.ElapsedTime, 0.0001f);
    }

    [Test]
    public void Tick_Stopped_TimeDoesNotIncrease()
    {
        var timer = new RaceTimer();

        timer.Tick(1.0f);

        Assert.AreEqual(0f, timer.ElapsedTime, 0.0001f);
    }

    [Test]
    public void Start_Tick_IncreasesElapsedTime()
    {
        var timer = new RaceTimer();

        timer.Start();
        timer.Tick(0.5f);

        Assert.IsTrue(timer.IsRunning);
        Assert.AreEqual(0.5f, timer.ElapsedTime, 0.0001f);
    }

    [Test]
    public void Stop_Tick_TimeDoesNotIncrease()
    {
        var timer = new RaceTimer();

        timer.Start();
        timer.Tick(1.0f);
        timer.Stop();
        timer.Tick(5.0f);

        Assert.IsFalse(timer.IsRunning);
        Assert.AreEqual(1.0f, timer.ElapsedTime, 0.0001f);
    }

    [Test]
    public void Reset_SetsTimeToZero_TimerStopped()
    {
        var timer = new RaceTimer();

        timer.Start();
        timer.Tick(2.0f);
        timer.Reset();

        Assert.IsFalse(timer.IsRunning);
        Assert.AreEqual(0f, timer.ElapsedTime, 0.0001f);
    }

    [Test]
    public void OnTimeChanged_FireEvent_OnlyWhenRunning()
    {
        var timer = new RaceTimer();

        int calls = 0;
        timer.OnTimeChanged += t =>
        {
            calls++;
        };

        // Not running -> no event
        timer.Tick(1.0f);
        Assert.AreEqual(0, calls);

        // Running -> event fires on tick
        timer.Start();
        timer.Tick(0.25f);

        Assert.AreEqual(1, calls);
    }
}