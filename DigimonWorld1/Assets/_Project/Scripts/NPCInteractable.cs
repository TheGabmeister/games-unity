using TMPro;
using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueData _dialogue;
    [SerializeField] private TextMeshPro _promptText;

    public string InteractPrompt => "Talk";

    private void Awake()
    {
        HidePrompt();
    }

    public void Interact()
    {
        if (DialogueManager.Instance.IsActive) return;

        HidePrompt();
        DialogueManager.Instance.StartDialogue(_dialogue);
    }

    public void ShowPrompt()
    {
        if (_promptText != null)
            _promptText.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        if (_promptText != null)
            _promptText.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_promptText != null && _promptText.gameObject.activeSelf)
            _promptText.transform.rotation = Camera.main.transform.rotation;
    }
}
