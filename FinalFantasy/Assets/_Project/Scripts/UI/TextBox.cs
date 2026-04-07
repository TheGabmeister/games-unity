using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TextBox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textDisplay;
    [SerializeField] float charsPerSecond = 30f;
    [SerializeField] GameObject advanceIndicator; // small arrow showing "press to continue"

    Queue<string> pendingLines = new Queue<string>();
    string currentLine;
    float revealTimer;
    int revealedChars;
    bool isRevealing;
    bool isActive;

    public bool IsActive => isActive;

    public void Show(string[] lines)
    {
        gameObject.SetActive(true);
        isActive = true;
        pendingLines.Clear();
        foreach (var line in lines)
            pendingLines.Enqueue(line);

        ShowNextLine();
    }

    void ShowNextLine()
    {
        if (pendingLines.Count == 0)
        {
            Hide();
            return;
        }

        currentLine = pendingLines.Dequeue();
        textDisplay.text = "";
        textDisplay.maxVisibleCharacters = 0;
        textDisplay.text = currentLine;
        revealedChars = 0;
        revealTimer = 0;
        isRevealing = true;
        if (advanceIndicator != null) advanceIndicator.SetActive(false);
    }

    void Update()
    {
        if (!isActive) return;

        var input = GameManager.Instance?.InputManager;

        if (isRevealing)
        {
            revealTimer += Time.deltaTime;
            int targetChars = Mathf.FloorToInt(revealTimer * charsPerSecond);
            if (targetChars > revealedChars)
            {
                revealedChars = targetChars;
                textDisplay.maxVisibleCharacters = revealedChars;
            }

            if (revealedChars >= currentLine.Length)
            {
                FinishReveal();
            }
            else if (input?.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
            {
                // Skip to end
                FinishReveal();
            }
        }
        else
        {
            // Waiting for confirm to advance
            if (input?.ConfirmAction != null && input.ConfirmAction.WasPressedThisFrame())
            {
                GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
                ShowNextLine();
            }
        }
    }

    void FinishReveal()
    {
        isRevealing = false;
        textDisplay.maxVisibleCharacters = currentLine.Length;
        revealedChars = currentLine.Length;
        if (advanceIndicator != null) advanceIndicator.SetActive(true);
    }

    public void Hide()
    {
        isActive = false;
        gameObject.SetActive(false);
    }
}
