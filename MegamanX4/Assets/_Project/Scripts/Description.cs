using UnityEngine;

[DisallowMultipleComponent]
public class Description : MonoBehaviour
{
    [SerializeField]
    [TextArea(3, 8)]
    string descriptionText;

    public string DescriptionText => descriptionText;
}
