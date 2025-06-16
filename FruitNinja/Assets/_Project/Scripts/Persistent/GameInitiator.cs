using UnityEngine;
using PrimeTween;

public class GameInitiator : MonoBehaviour
{
    [SerializeField] GameObject _playerPrefab;
    [SerializeField] GameObject _musicManager;
    [SerializeField] GameObject _sfxManager;
    [SerializeField] GameObject _uiManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Debug.Log("Game Initiator");
    }

}
