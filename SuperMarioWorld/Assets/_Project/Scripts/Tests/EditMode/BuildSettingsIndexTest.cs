using NUnit.Framework;
using UnityEditor;

namespace SMW.Tests.EditMode
{
    public sealed class BuildSettingsIndexTest
    {
        private static bool SceneRegistered(string fileName)
        {
            foreach (var s in EditorBuildSettings.scenes)
                if (s.enabled && s.path != null && s.path.EndsWith($"{fileName}.unity"))
                    return true;
            return false;
        }

        [Test]
        public void Boot_At_Index_0()
        {
            var scenes = EditorBuildSettings.scenes;
            Assert.IsTrue(scenes.Length > 0, "No scenes in build settings. Run Tools → SMW → Setup → Bootstrap Phase 0 Scenes.");
            Assert.IsTrue(scenes[0].path.EndsWith("Boot.unity"),
                $"Boot must be at index 0; found {scenes[0].path}");
        }

        [Test] public void Systems_Registered()   => Assert.IsTrue(SceneRegistered("Systems"));
        [Test] public void Title_Registered()     => Assert.IsTrue(SceneRegistered("Title"));
        [Test] public void Overworld_Registered() => Assert.IsTrue(SceneRegistered("Overworld"));
    }
}
