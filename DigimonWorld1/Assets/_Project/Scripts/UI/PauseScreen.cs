using UnityEngine;

public class PauseScreen : MonoBehaviour
{
    [SerializeField] private GameObject _panel;

    private bool _isOpen;

    public bool IsOpen => _isOpen;

    private void Awake()
    {
        if (_panel != null)
            _panel.SetActive(false);
    }

    public void Open()
    {
        _isOpen = true;
        _panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Close()
    {
        _isOpen = false;
        _panel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (_isOpen)
            Time.timeScale = 1f;
    }
}
