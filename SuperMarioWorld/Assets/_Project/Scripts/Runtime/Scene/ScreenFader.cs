using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

namespace SMW
{
    [RequireComponent(typeof(Canvas))]
    public sealed class ScreenFader : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;

        private void Awake()
        {
            if (fadeImage != null)
            {
                var c = fadeImage.color;
                c.a = 0f;
                fadeImage.color = c;
                fadeImage.raycastTarget = false;
            }
        }

        public async Task FadeOutAsync(float duration)
        {
            if (fadeImage == null) return;
            fadeImage.raycastTarget = true;
            await Tween.Alpha(fadeImage, endValue: 1f, duration: duration, useUnscaledTime: true);
        }

        public async Task FadeInAsync(float duration)
        {
            if (fadeImage == null) return;
            await Tween.Alpha(fadeImage, endValue: 0f, duration: duration, useUnscaledTime: true);
            fadeImage.raycastTarget = false;
        }
    }
}
