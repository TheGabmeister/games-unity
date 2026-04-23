using TMPro;
using UnityEngine;

public class DialogueManager : Singleton<DialogueManager>
{
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private TMP_Text _speakerText;
    [SerializeField] private TMP_Text _bodyText;

    private DialogueData _currentDialogue;
    private int _lineIndex;
    private bool _isActive;
    private bool _justOpened;

    public bool IsActive => _isActive;

    protected override void Awake()
    {
        base.Awake();
        if (_dialoguePanel != null)
            _dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!_isActive) return;

        if (InputManager.Instance.Actions.Player.Interact.WasPressedThisFrame())
        {
            if (_justOpened)
            {
                _justOpened = false;
                return;
            }
            AdvanceLine();
        }
    }

    public void StartDialogue(DialogueData dialogue)
    {
        _currentDialogue = dialogue;
        _lineIndex = 0;
        _isActive = true;
        _justOpened = true;

        InputManager.Instance.SetPlayerInputEnabled(false);

        _dialoguePanel.SetActive(true);
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        DialogueLine line = _currentDialogue.Lines[_lineIndex];
        _speakerText.text = line.Speaker;
        _bodyText.text = line.Text;
    }

    private void AdvanceLine()
    {
        _lineIndex++;
        if (_lineIndex >= _currentDialogue.Lines.Length)
        {
            EndDialogue();
            return;
        }
        ShowCurrentLine();
    }

    private void EndDialogue()
    {
        _isActive = false;
        _currentDialogue = null;
        _dialoguePanel.SetActive(false);

        InputManager.Instance.SetPlayerInputEnabled(true);
    }
}
