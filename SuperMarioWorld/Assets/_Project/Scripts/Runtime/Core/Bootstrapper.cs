using UnityEngine;
using UnityEngine.SceneManagement;

namespace SMW.Core
{
    public sealed class Bootstrapper : MonoBehaviour
    {
        public const string SystemsSceneName = "Systems";
        public const string BootSceneName = "Boot";
        public const string TitleSceneName = "Title";
        public const string OverworldSceneName = "Overworld";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureSystemsLoaded()
        {
            var active = SceneManager.GetActiveScene();
            if (active.name == SystemsSceneName)
            {
                Debug.LogError(
                    "[SMW] Don't Play from Systems.unity — it is a persistent layer, not an entry scene. Use Boot, Title, Overworld, or a Level scene.");
                return;
            }

            if (!SceneManager.GetSceneByName(SystemsSceneName).isLoaded)
            {
                SceneManager.LoadScene(SystemsSceneName, LoadSceneMode.Additive);
            }
        }

        private void Start()
        {
            // Systems is loaded by EnsureSystemsLoaded above; GameServices.Awake has already run.
            if (GameServices.IsRegistered && GameServices.GameState != null)
            {
                GameServices.GameState.TransitionToTitle();
            }
        }
    }
}
