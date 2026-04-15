using UnityEngine;
using SMW.Core;

namespace SMW.State
{
    public sealed class PausedState : IGameState
    {
        public void OnEnter()
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
            GameServices.Audio?.PlayUiSfx(Audio.UiSfxId.Pause);
        }

        public void OnExit()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameServices.Audio?.PlayUiSfx(Audio.UiSfxId.Unpause);
        }
    }
}
