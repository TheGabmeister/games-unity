using TMPro;
using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private TextMeshPro _promptText;

    private Renderer _renderer;

    public string InteractPrompt => "Press E";

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        HidePrompt();
    }

    public void Interact()
    {
        if (_renderer != null)
            _renderer.material.color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f);
    }

    public void ShowPrompt()
    {
        if (_promptText != null)
            _promptText.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        if (_promptText != null)
            _promptText.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_promptText != null && _promptText.gameObject.activeSelf)
            _promptText.transform.rotation = Camera.main.transform.rotation;
    }
}
