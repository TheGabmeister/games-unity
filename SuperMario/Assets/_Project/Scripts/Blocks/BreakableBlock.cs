using UnityEngine;

public class BreakableBlock : MonoBehaviour, IHeadbuttable
{


    public void OnHeadbutt()
    {
        Destroy(gameObject);
    }
}
