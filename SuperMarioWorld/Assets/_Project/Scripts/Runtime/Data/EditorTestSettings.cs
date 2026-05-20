using UnityEngine;

[CreateAssetMenu(fileName = "EditorTestSettings", menuName = "SMW/Settings/Editor Test Settings")]
public sealed class EditorTestSettings : ScriptableObject
{
    public enum DirectEntrySaveMode { FreshData, LoadSlot1 }

    [SerializeField] private DirectEntrySaveMode directEntryMode = DirectEntrySaveMode.FreshData;

    public DirectEntrySaveMode Mode => directEntryMode;
}
