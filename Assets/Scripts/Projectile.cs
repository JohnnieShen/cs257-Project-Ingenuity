using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float launchForce;
    public float damage = 10f;
    public float timeToDestroy = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddRelativeForce(0, 0, launchForce, ForceMode.Impulse);
        Destroy(gameObject, timeToDestroy);
    }
    public void SetDamage(float dmg)
    {
        damage = dmg;
    }
    void OnTriggerEnter(Collider other)
    {
        BlockHealth blockHealth = other.GetComponent<BlockHealth>();
        if (blockHealth != null)
        {
            blockHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}