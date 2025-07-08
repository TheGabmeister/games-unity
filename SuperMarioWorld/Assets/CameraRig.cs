using UnityEngine;

public class CameraRig : MonoBehaviour
{
    public Transform target;
    
    void Update()
    {
        if (target)
        {
            transform.position = new Vector3(target.position.x, target.position.y, -10f);
        }
            
    }
}
