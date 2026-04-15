using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using SMW.Core;
using SMW.Data;
using SMW.State;

namespace SMW.Tests.PlayMode
{
    public sealed class DirectEntryLevelTest
    {
        [UnityTest]
        public IEnumerator EnterDirectLevel_Results_In_LevelState_Within_One_Frame()
        {
            yield return null;
            yield return null;
            Assert.IsTrue(GameServices.IsRegistered);

            var data = ScriptableObject.CreateInstance<LevelData>();
            GameServices.GameState.EnterDirectLevel(data, "default");

            yield return null;

            Assert.IsInstanceOf<LevelState>(GameServices.GameState.Current,
                "GameStateMachine must be in LevelState after EnterDirectLevel.");
            Object.Destroy(data);
        }
    }
}
