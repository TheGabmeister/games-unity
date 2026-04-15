using System;
using UnityEngine;

namespace SMW.Score
{
    public enum ScoreReason
    {
        Stomp, StompCombo, Coin, DragonCoin, OneUp, GoalMid, GoalHigh, GoalLow,
        EnemyFireball, EnemyCape, ShellKill, TimeBonus, PickupBonus
    }

    public sealed class ScoreService : MonoBehaviour
    {
        public event Action<ScoreReason, int, Vector3> Awarded;
        public event Action<int> ScoreChanged;

        public int TotalScore { get; private set; }

        public void Award(ScoreReason reason, int basePoints, Vector3 worldPos)
        {
            TotalScore += basePoints;
            Awarded?.Invoke(reason, basePoints, worldPos);
            ScoreChanged?.Invoke(TotalScore);
        }

        public void Reset() { TotalScore = 0; ScoreChanged?.Invoke(TotalScore); }
    }
}
