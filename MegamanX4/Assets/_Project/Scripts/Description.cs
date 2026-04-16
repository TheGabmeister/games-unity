using UnityEngine;

[DisallowMultipleComponent]
public class Description : MonoBehaviour
{
    [SerializeField]
    [TextArea(3, 8)]
    string _descriptionText;

    public string DescriptionText => _descriptionText;
}
