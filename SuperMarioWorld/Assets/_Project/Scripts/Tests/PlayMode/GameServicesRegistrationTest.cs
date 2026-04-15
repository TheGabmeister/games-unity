using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class GameServicesRegistrationTest
    {
        [UnityTest]
        public IEnumerator All_Services_Available_After_Systems_Loads()
        {
            yield return null;
            yield return null;
            yield return null;
            Assert.IsTrue(GameServices.IsRegistered, "GameServices must register after Systems scene awakes.");
            Assert.IsNotNull(GameServices.Save);
            Assert.IsNotNull(GameServices.SceneLoader);
            Assert.IsNotNull(GameServices.Fader);
            Assert.IsNotNull(GameServices.GameState);
            Assert.IsNotNull(GameServices.Feedback);
            Assert.IsNotNull(GameServices.Session);
            Assert.IsNotNull(GameServices.Audio);
        }
    }
}
