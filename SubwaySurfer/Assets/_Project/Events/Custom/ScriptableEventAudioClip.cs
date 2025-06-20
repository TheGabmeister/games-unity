using UnityEngine;
using Obvious.Soap;

[CreateAssetMenu(fileName = "scriptable_event_" + nameof(AudioClip), menuName = "Soap/ScriptableEvents/"+ nameof(AudioClip))]
public class ScriptableEventAudioClip : ScriptableEvent<AudioClip>
{
    
}

