using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ScreenFader : MonoBehaviour
{
    Image _image;

    void Awake()
    {
        _image = GetComponent<Image>();
    }

    public Tween FadeToColor(Color color, float duration) =>
        Tween.Color(_image, color, duration);
}
