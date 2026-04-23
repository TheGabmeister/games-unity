public interface IInteractable
{
    string InteractPrompt { get; }
    void Interact();
    void ShowPrompt();
    void HidePrompt();
}
