using UnityEngine;

namespace SMW
{
    public sealed class PausedState : IGameState
    {
        public void OnEnter()
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
            GameServices.Audio?.PlayUiSfx(UiSfxId.Pause);
        }

        public void OnExit()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            GameServices.Audio?.PlayUiSfx(UiSfxId.Unpause);
        }
    }
}
