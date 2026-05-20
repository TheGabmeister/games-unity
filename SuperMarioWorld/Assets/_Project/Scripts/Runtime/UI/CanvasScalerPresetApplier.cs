using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
[DefaultExecutionOrder(-100)]
public sealed class CanvasScalerPresetApplier : MonoBehaviour
{
    public const float ReferenceWidth = 1280f;
    public const float ReferenceHeight = 720f;

    private void Awake()
    {
        var scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }
}
