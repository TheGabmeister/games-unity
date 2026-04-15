using System;
using System.Collections;
using System.Collections.Generic;
using Eflatun.SceneReference;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace SMW
{
    public sealed class SceneLoaderFadeSequenceTest
    {
        [UnityTest]
        public IEnumerator FadeOut_Peak_FadeIn_In_Order()
        {
            // Let Bootstrapper pull in Systems and GameServices wire up.
            yield return null;
            yield return null;
            Assert.IsTrue(GameServices.IsRegistered,
                "Systems must be loaded via Bootstrapper. Phase 0 setup incomplete?");

            var loader = GameServices.SceneLoader;
            var fader = GameServices.Fader;
            Assert.IsNotNull(loader, "SceneLoader missing from GameServices.");
            Assert.IsNotNull(fader, "ScreenFader missing from GameServices.");

            // Pick a registered, known-safe scene target. Title is set up by Phase 0 bootstrap.
            string titlePath = null;
#if UNITY_EDITOR
            foreach (var s in UnityEditor.EditorBuildSettings.scenes)
            {
                if (!s.enabled) continue;
                if (s.path != null && s.path.EndsWith("Title.unity")) { titlePath = s.path; break; }
            }
#endif
            if (string.IsNullOrEmpty(titlePath))
            {
                Assert.Ignore("Title.unity not in Build Settings. Run Tools → SMW → Setup → Bootstrap Phase 0 Scenes.");
                yield break;
            }

            SceneReference target;
            try { target = SceneReference.FromScenePath(titlePath); }
            catch (Exception ex)
            {
                Assert.Ignore($"Couldn't construct SceneReference for Title: {ex.Message}");
                yield break;
            }

            var sequence = new List<string>();
            void OnFadeOut() => sequence.Add("fadeOut");
            void OnPeak(SceneReference sr, object p) => sequence.Add("peak");
            void OnFadeIn() => sequence.Add("fadeIn");

            fader.OnFadeOutStarted += OnFadeOut;
            loader.OnTransitionPeak += OnPeak;
            fader.OnFadeInStarted += OnFadeIn;

            bool taskComplete = false;
            Exception taskError = null;
            try
            {
                var opts = new SceneLoadOptions
                {
                    FadeOutDuration = 0.02f,
                    FadeInDuration = 0.02f,
                    UnloadPrevious = false
                };
                var task = loader.LoadAsync(target, opts);
                var timeout = System.Diagnostics.Stopwatch.StartNew();
                while (!task.IsCompleted)
                {
                    if (timeout.Elapsed.TotalSeconds > 10.0)
                    {
                        Assert.Fail("SceneLoader.LoadAsync timed out after 10s.");
                        yield break;
                    }
                    yield return null;
                }
                taskComplete = true;
                if (task.IsFaulted) taskError = task.Exception;
            }
            finally
            {
                fader.OnFadeOutStarted -= OnFadeOut;
                loader.OnTransitionPeak -= OnPeak;
                fader.OnFadeInStarted -= OnFadeIn;
            }

            Assert.IsTrue(taskComplete, "LoadAsync did not complete.");
            Assert.IsNull(taskError, $"LoadAsync faulted: {taskError}");
            CollectionAssert.AreEqual(
                new[] { "fadeOut", "peak", "fadeIn" },
                sequence,
                "Expected fade-out → OnTransitionPeak → fade-in ordering.");
        }
    }
}
