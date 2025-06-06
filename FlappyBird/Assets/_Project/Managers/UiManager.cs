using EventBus;
using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    [SerializeField] TMP_Text _scoreText;

    private void OnEnable()
    {
        Bus.UiUpdateScore.Sub(UpdateScore);
    }

    private void OnDisable()
    {
        Bus.UiUpdateScore.Unsub(UpdateScore);
    }

    void UpdateScore(int value)
    {
        _scoreText.text = value.ToString();
    }
}
