using System.Collections.Generic;
using UnityEngine;

namespace SMW
{
    public sealed class LevelRunState : MonoBehaviour
    {
        public string CheckpointId;
        public ulong DragonCoinsCollectedThisAttempt;
        public readonly HashSet<string> BrokenBricks = new();
        public readonly HashSet<string> UsedQuestionBlocks = new();
        public readonly HashSet<string> DeadEnemies = new();
        public bool PSwitchActive;
    }
}
