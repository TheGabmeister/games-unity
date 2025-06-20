using UnityEngine;
using StoneShelter;

[RequireComponent(typeof(GUID))]
public class ChestComp : MonoBehaviour, IInteractable
{
    [SerializeField] Sprite _chestOpenSprite;
    [SerializeField] Sprite _chestCloseSprite;
    SpriteRenderer _sr;
    bool _isOpen = false;
    string _guid;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _guid = GetComponent<GUID>().guid;

        _isOpen = ES3.Load(_guid, _isOpen);
        if (_isOpen)
        {
            _sr.sprite = _chestOpenSprite;
        }
    }

    public void Interact()
    {
        if (!_isOpen)
        {
            // play SFX
            _sr.sprite = _chestOpenSprite;
            _isOpen = true;
        }
    }

    void OnDestroy()
    {
        ES3.Save(_guid, _isOpen);
    }
}
