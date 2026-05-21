using UnityEngine;

public class LevelManager : MonoBehaviour
{

    [SerializeField] LevelData _levelData;
    
    [SerializeField] GameObject _cameraPrefab;
    [SerializeField] GameObject _playerPrefab;


    void Start()
    {
        var playerStart = GameObject.FindGameObjectWithTag("PlayerStart").transform.position;

        var player = Instantiate(_playerPrefab, playerStart, Quaternion.identity);

        var cameraStart = new Vector3(playerStart.x, playerStart.y, _cameraPrefab.transform.position.z);
        var levelCameraObject = Instantiate(_cameraPrefab, cameraStart, Quaternion.identity);
        if (levelCameraObject.TryGetComponent<LevelCamera>(out var levelCamera))
        {
            levelCamera.Target = player.transform;
        }
    }
}
