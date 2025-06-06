using ScriptableObjectArchitecture;
using UnityEngine;
using TMPro;

public class UpdateScoreUI : MonoBehaviour
{
    [SerializeField] IntVariable currentScore;
    [SerializeField] TMP_Text scoreDisplay;
    [SerializeField] TMP_Text gameOverText;
    [SerializeField] GameObject restartButton;

    void Start()
    {
        scoreDisplay.SetText("Score: 0");
        gameOverText.alpha = 0f;
        restartButton.SetActive(false);

        currentScore.AddListener(UpdateScoreDisplay);
    }

    public void UpdateScoreDisplay()
    {
         scoreDisplay.SetText("Score: " + currentScore.Value);
    }

    public void StartGameOverUISequence()
    {
        FadeInText();
    }

    void FadeInText()
    {
        //gameOverText.DOFade(1f, 1f) 
          //  .OnComplete(() =>
          //  {
          //      restartButton.SetActive(true);
          //  });
    }
}
