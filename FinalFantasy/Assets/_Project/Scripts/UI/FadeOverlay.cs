using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class FadeOverlay : MonoBehaviour
{
    public static FadeOverlay Instance { get; private set; }

    Image fadeImage;
    Canvas canvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupCanvas();
    }

    void SetupCanvas()
    {
        canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        if (gameObject.GetComponent<CanvasScaler>() == null)
        {
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        }

        // Create or find fade image
        var imageTransform = transform.Find("FadeImage");
        if (imageTransform == null)
        {
            var go = new GameObject("FadeImage");
            go.transform.SetParent(transform, false);
            fadeImage = go.AddComponent<Image>();
            var rect = fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }
        else
        {
            fadeImage = imageTransform.GetComponent<Image>();
        }

        fadeImage.color = new Color(0, 0, 0, 1); // Start black
        fadeImage.raycastTarget = false;
    }

    public async Awaitable FadeOut(float duration = 0.3f)
    {
        fadeImage.raycastTarget = true;
        await Tween.Alpha(fadeImage, 1f, duration);
    }

    public async Awaitable FadeIn(float duration = 0.3f)
    {
        await Tween.Alpha(fadeImage, 0f, duration);
        fadeImage.raycastTarget = false;
    }

    public void SetBlack()
    {
        fadeImage.color = new Color(0, 0, 0, 1);
        fadeImage.raycastTarget = true;
    }

    public void SetClear()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;
    }
}
