using UnityEngine;

public class Enemy : MonoBehaviour, IHittable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnHit()
    {
        Destroy(gameObject);
    }
}
