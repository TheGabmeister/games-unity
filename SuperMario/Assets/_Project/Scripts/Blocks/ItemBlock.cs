using UnityEngine;

public class ItemBlock : MonoBehaviour, IHeadbuttable
{
    bool _active = true;
    [SerializeField] GameObject _item;

    public void OnHeadbutt()
    {
        

        if (_active)
        {
            Instantiate(_item, transform.position + new Vector3(0,1,0), Quaternion.identity);
        }
        
        _active = false;
    }
}
