using NUnit.Framework;

namespace SMW
{
    public sealed class SaveRoundTripTest
    {
        [Test]
        public void Empty_SaveData_RoundTrips()
        {
            var original = new SaveData();
            original.score = 12345;
            original.lives = 3;
            original.totalCoins = 42;
            original.switchPalaces.yellow = true;
            original.levelCompletions["level_01"] = new LevelCompletionFlags { normalExit = true };
            original.dragonCoinsByLevel["level_01"] = 0b10101;

            var json = SaveSerializer.Serialize(original);
            Assert.IsNotNull(json);
            var copy = SaveSerializer.Deserialize(json);

            Assert.AreEqual(original.score, copy.score);
            Assert.AreEqual(original.lives, copy.lives);
            Assert.AreEqual(original.totalCoins, copy.totalCoins);
            Assert.IsTrue(copy.switchPalaces.yellow);
            Assert.IsTrue(copy.levelCompletions.ContainsKey("level_01"));
            Assert.IsTrue(copy.levelCompletions["level_01"].normalExit);
            Assert.AreEqual(0b10101UL, copy.dragonCoinsByLevel["level_01"]);
            Assert.AreEqual(SaveData.CurrentSaveVersion, copy.saveVersion);
        }
    }
}
