using ScriptableObjectArchitecture;
using System.Collections;
using UnityEngine;
using PrimeTween;

public class GameInstance : Singleton<GameInstance>
{
    [SerializeField] LevelData[] _levels;
    [SerializeField] SceneDataReference _currentLevel;
    SceneDataReference _nextLevel;
    
    public int Lives { get; private set; }
    [SerializeField] AudioClip _starModeMusic;

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }


    void HandlePlayerDeath()
    {
        StartCoroutine(HandlePlayerDeathCoroutine());
    }

    IEnumerator HandlePlayerDeathCoroutine()
    {
        yield return new WaitForSeconds(2);

        //_updateLives.Raise(-1);
        if (Lives <= 0)
        {
            StartGameOverSequence();
        }
        else
        {
            StartLevel();
        }
    }

    void RestartGame()
    {

    }

    void StartLevel()
    {

    }

    void StartNextLevel()
    {

    }


    void StartGameOverSequence()
    {
        // Show Game Over UI
    }

    void StartGameFinishSequence()
    {
        Debug.Log("Game Finished!");
    }

    private void ToggleStarMode(bool value)
    {
        if (value)
        {
            //_changeMusic.Raise(_starModeMusic);
        }
        else
        {
            //_changeMusic.Raise(_levels[_currentLevelIndex].music);
        }
    }
}
