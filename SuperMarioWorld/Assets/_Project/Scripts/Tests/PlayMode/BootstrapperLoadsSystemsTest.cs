using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class BootstrapperLoadsSystemsTest
    {
        [UnityTest]
        public IEnumerator Systems_Scene_Is_Loaded_On_Play()
        {
            // The RuntimeInitializeOnLoadMethod fires automatically when PlayMode starts.
            // Give Unity a frame to load the additive scene.
            yield return null;
            yield return null;
            yield return null;
            var systems = SceneManager.GetSceneByName("Systems");
            Assert.IsTrue(systems.IsValid() && systems.isLoaded,
                "Systems.unity must be loaded additively by Bootstrapper. " +
                "Run Tools → SMW → Setup → Bootstrap Phase 0 Scenes and ensure Systems is in Build Settings.");
        }
    }
}
