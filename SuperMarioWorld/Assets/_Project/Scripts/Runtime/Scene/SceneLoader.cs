using System;
using System.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private ScreenFader _fader;
    private bool _isTransitioning;
    private string _previousSceneName;
    private string _currentLevelSceneName;

    public event Action<SceneReference, object> OnTransitionPeak;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public async Task LoadAsync(SceneReference target, SceneLoadOptions opts = default)
    {
        if (target == null || string.IsNullOrEmpty(target.Name))
        {
            Debug.LogError("[SceneLoader] LoadAsync called with null/empty SceneReference.");
            return;
        }
        if (_isTransitioning)
        {
            Debug.LogWarning("[SceneLoader] re-entered during transition — ignoring.");
            return;
        }
        if (opts.Equals(default(SceneLoadOptions))) opts = SceneLoadOptions.Default;
        _isTransitioning = true;

        _previousSceneName = SceneManager.GetActiveScene().name;

        if (_fader != null) await _fader.FadeOutAsync(opts.FadeOutDuration);
        await SceneManager.LoadSceneAsync(target.Name, LoadSceneMode.Additive);

        OnTransitionPeak?.Invoke(target, opts.Payload);

        if (opts.UnloadPrevious
            && !string.IsNullOrEmpty(_previousSceneName)
            && _previousSceneName != target.Name
            && _previousSceneName != "Systems"
            && _previousSceneName != "Boot")
        {
            await SceneManager.UnloadSceneAsync(_previousSceneName);
        }

        var loaded = SceneManager.GetSceneByName(target.Name);
        if (loaded.IsValid()) SceneManager.SetActiveScene(loaded);

        if (_fader != null) await _fader.FadeInAsync(opts.FadeInDuration);

        if (IsLevelScene(target.Name)) _currentLevelSceneName = target.Name;
        _isTransitioning = false;
    }

    public async Task ReloadLevelAsync()
    {
        if (string.IsNullOrEmpty(_currentLevelSceneName))
        {
            Debug.LogWarning("[SceneLoader] ReloadLevelAsync with no current level tracked.");
            return;
        }
        if (_isTransitioning) return;
        _isTransitioning = true;

        var name = _currentLevelSceneName;
        if (_fader != null) await _fader.FadeOutAsync(0.15f);
        await SceneManager.UnloadSceneAsync(name);
        await SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        var loaded = SceneManager.GetSceneByName(name);
        if (loaded.IsValid()) SceneManager.SetActiveScene(loaded);
        if (_fader != null) await _fader.FadeInAsync(0.15f);

        _isTransitioning = false;
    }

    private static bool IsLevelScene(string sceneName) =>
        sceneName != null
        && sceneName != "Boot"
        && sceneName != "Systems"
        && sceneName != "Title"
        && sceneName != "Overworld";
}
