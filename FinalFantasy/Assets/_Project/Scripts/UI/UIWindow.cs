using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIWindow : MonoBehaviour
{
    [SerializeField] float borderWidth = 2f;

    Image background;
    Outline outline;

    void Awake()
    {
        SetupWindow();
    }

    void SetupWindow()
    {
        // Background
        background = GetComponent<Image>();
        if (background == null) background = gameObject.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0.267f, 0.95f); // dark blue

        // White border using Outline component
        outline = GetComponent<Outline>();
        if (outline == null) outline = gameObject.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(borderWidth, -borderWidth);

        // Also add a second outline for full border effect
        var outlines = GetComponents<Outline>();
        if (outlines.Length < 2)
        {
            var outline2 = gameObject.AddComponent<Outline>();
            outline2.effectColor = Color.white;
            outline2.effectDistance = new Vector2(-borderWidth, borderWidth);
        }
    }
}
