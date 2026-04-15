using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SMW.Core;
using SMW.State;

namespace SMW.Tests.PlayMode
{
    public sealed class PauseTimeScaleTest
    {
        [UnityTest]
        public IEnumerator Pause_Freezes_Time_And_Audio()
        {
            yield return null;
            yield return null;
            Assert.IsTrue(GameServices.IsRegistered);

            var gsm = GameServices.GameState;

            // Ensure a non-paused baseline.
            Time.timeScale = 1f;
            AudioListener.pause = false;

            gsm.Push(new PausedState());
            yield return null;
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(AudioListener.pause);

            gsm.Pop();
            yield return null;
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(AudioListener.pause);
        }
    }
}
