using UnityEngine;
using UnityEngine.InputSystem;

public class PauseScreen : Singleton<PauseScreen>
{
    [SerializeField] private GameObject _panel;

    private bool _isOpen;

    public bool IsOpen => _isOpen;

    protected override void Awake()
    {
        base.Awake();
        if (_panel != null)
            _panel.SetActive(false);
    }

    private void Update()
    {
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (InventoryScreen.Instance != null && InventoryScreen.Instance.IsOpen) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsActive) return;

        if (_isOpen)
            Resume();
        else
            Pause();
    }

    private void Pause()
    {
        _isOpen = true;
        _panel.SetActive(true);
        Time.timeScale = 0f;
        InputManager.Instance.SetPlayerInputEnabled(false);
    }

    private void Resume()
    {
        _isOpen = false;
        _panel.SetActive(false);
        Time.timeScale = 1f;
        InputManager.Instance.SetPlayerInputEnabled(true);
    }
}
