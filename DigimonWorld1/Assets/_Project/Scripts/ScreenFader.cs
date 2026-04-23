using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : Singleton<ScreenFader>
{
    [SerializeField] private Image _fadeImage;
    [SerializeField] private float _fadeDuration = 0.5f;

    public async Awaitable FadeOut()
    {
        await Fade(0f, 1f);
    }

    public async Awaitable FadeIn()
    {
        await Fade(1f, 0f);
    }

    private async Awaitable Fade(float from, float to)
    {
        float elapsed = 0f;
        SetAlpha(from);
        _fadeImage.raycastTarget = true;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _fadeDuration);
            SetAlpha(Mathf.Lerp(from, to, t));
            await Awaitable.NextFrameAsync();
        }

        SetAlpha(to);
        _fadeImage.raycastTarget = to > 0f;
    }

    private void SetAlpha(float alpha)
    {
        Color c = _fadeImage.color;
        c.a = alpha;
        _fadeImage.color = c;
    }
}
