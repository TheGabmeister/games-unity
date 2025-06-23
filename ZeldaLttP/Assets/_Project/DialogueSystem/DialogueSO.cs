using UnityEngine;

[CreateAssetMenu(fileName = "DialogueSO", menuName = "Scriptable Objects/DialogueSO")]
public class DialogueSO : ScriptableObject
{
    [TextArea] public string[] dialogue;
    public string[] Dialogue => dialogue;
}
