using TMPro;
using UnityEngine;

public class NameController : MonoBehaviour
{
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private TMP_InputField _digimonNameInput;

    private void Start()
    {
        _playerNameInput.Select();
    }

    public void OnConfirm()
    {
        if (_playerNameInput.text.Length == 0 || _digimonNameInput.text.Length == 0)
            return;

        GameManager.Instance.LoadGameplayScene();
    }
}
