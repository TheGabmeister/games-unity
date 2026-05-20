using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

[RequireComponent(typeof(Canvas))]
public sealed class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private Image fadeImage;

    // Fires at the start of each fade. Exposed for test observability; no production caller uses these.
    public event Action OnFadeOutStarted;
    public event Action OnFadeInStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.raycastTarget = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public async Task FadeOutAsync(float duration)
    {
        if (fadeImage == null) return;
        fadeImage.raycastTarget = true;
        OnFadeOutStarted?.Invoke();
        await Tween.Alpha(fadeImage, endValue: 1f, duration: duration, useUnscaledTime: true);
    }

    public async Task FadeInAsync(float duration)
    {
        if (fadeImage == null) return;
        OnFadeInStarted?.Invoke();
        await Tween.Alpha(fadeImage, endValue: 0f, duration: duration, useUnscaledTime: true);
        fadeImage.raycastTarget = false;
    }
}
