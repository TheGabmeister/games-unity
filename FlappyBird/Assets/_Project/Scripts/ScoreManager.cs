using ScriptableObjectArchitecture;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] IntVariable currentScore;
    [SerializeField] IntVariable highScore;

    void Start()
    {
        currentScore.Value = 0;
    }

    public void UpdateCurrentScore()
    {
        currentScore.Value++;
        if (currentScore.Value > highScore.Value)
        {
            highScore.Value = currentScore.Value;
        }
    }
}
