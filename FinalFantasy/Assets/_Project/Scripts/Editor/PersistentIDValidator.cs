using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

[InitializeOnLoad]
public class PersistentIDValidator
{
    static PersistentIDValidator()
    {
        EditorSceneManager.sceneSaving += OnSceneSaving;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnSceneSaving(UnityEngine.SceneManagement.Scene scene, string path)
    {
        ValidateScene();
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            ValidateScene();
    }

    static void ValidateScene()
    {
        var ids = Object.FindObjectsByType<PersistentID>(FindObjectsSortMode.None);
        var seen = new Dictionary<string, GameObject>();

        foreach (var pid in ids)
        {
            if (string.IsNullOrEmpty(pid.ID)) continue;

            if (seen.TryGetValue(pid.ID, out var existing))
            {
                Debug.LogError(
                    $"[PersistentID] Duplicate ID '{pid.ID}' found on '{pid.gameObject.name}' and '{existing.name}'. " +
                    "This likely happened from copy/paste. Delete the ID field on one object and let it regenerate.",
                    pid.gameObject);
            }
            else
            {
                seen[pid.ID] = pid.gameObject;
            }
        }
    }
}
