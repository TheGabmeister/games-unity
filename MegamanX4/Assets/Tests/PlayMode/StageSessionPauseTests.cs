using NUnit.Framework;
using UnityEngine;

public class StageSessionPauseTests
{
    [SetUp]
    public void SetUp()
    {
        Time.timeScale = 1f;
    }

    [TearDown]
    public void TearDown()
    {
        Time.timeScale = 1f;
    }

    [Test]
    public void TogglePause_TogglesTimeScale()
    {
        var go = new GameObject("StageSession");
        var stageSession = go.AddComponent<StageSession>();

        try
        {
            stageSession.TogglePause();
            Assert.That(stageSession.IsPaused, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(0f));

            stageSession.TogglePause();
            Assert.That(stageSession.IsPaused, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void DestroyingPausedStageSession_RestoresTimeScale()
    {
        var go = new GameObject("StageSession");
        var stageSession = go.AddComponent<StageSession>();

        stageSession.TogglePause();
        Assert.That(Time.timeScale, Is.EqualTo(0f));

        Object.DestroyImmediate(go);

        Assert.That(Time.timeScale, Is.EqualTo(1f));
    }
}
