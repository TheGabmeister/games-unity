using System;

namespace SMW
{
    public sealed class HudViewModel
    {
        public event Action<int> LivesChanged;
        public event Action<int> CoinsChanged;
        public event Action<int> ScoreChanged;
        public event Action<int> TimerTick;
        public event Action<int> DragonCoinsChanged;
        public event Action<string> PowerStateChanged;

        public void SetLives(int v) => LivesChanged?.Invoke(v);
        public void SetCoins(int v) => CoinsChanged?.Invoke(v);
        public void SetScore(int v) => ScoreChanged?.Invoke(v);
        public void SetTimer(int secs) => TimerTick?.Invoke(secs);
        public void SetDragonCoins(int bitmask) => DragonCoinsChanged?.Invoke(bitmask);
        public void SetPowerState(string id) => PowerStateChanged?.Invoke(id);
    }
}
