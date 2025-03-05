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
        Debug.Log("Projectile hit: " + other.name+" "+other.tag);
        if (blockHealth != null && other.CompareTag("EnemyBlock"))
        {
            blockHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}