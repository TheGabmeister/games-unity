using UnityEngine;
using UnityEngine.InputSystem;

public class NameController : MonoBehaviour
{
    private string _playerName = "";
    private string _digimonName = "";
    private bool _editingDigimonName;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (!_editingDigimonName)
            {
                _editingDigimonName = true;
            }
            else if (_digimonName.Length > 0)
            {
                GameManager.Instance.LoadGameplayScene();
            }
        }
    }
}
