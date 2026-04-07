using UnityEngine;

public class PersistentID : MonoBehaviour
{
    [SerializeField] string id;

    public string ID => id;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString();
    }
#endif
}
