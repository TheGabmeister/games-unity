using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public string Speaker;
    [TextArea(2, 4)] public string Text;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "DigimonWorld/DialogueData")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private DialogueLine[] _lines;

    public DialogueLine[] Lines => _lines;
}
