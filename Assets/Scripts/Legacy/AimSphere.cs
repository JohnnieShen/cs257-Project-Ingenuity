using UnityEngine;

public class AimSphere : MonoBehaviour
{
    public float sphereRadius = 10f;
    public SphereCollider sphere;
    
    void Start()
    {
        if (sphere == null)
        {
            sphere = gameObject.AddComponent<SphereCollider>();
        }
        sphere.radius = sphereRadius;
        sphere.isTrigger = true;
        
        // gameObject.layer = LayerMask.NameToLayer("Aim");
    }
    
}
