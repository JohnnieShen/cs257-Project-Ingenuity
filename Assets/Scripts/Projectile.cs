using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool IsEnemyProjectile = false;
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
        Debug.Log("Projectile hit: " + other.name + " " + other.tag);
        BlockHealth blockHealth = null;

        if (other.CompareTag("EnemyBlock"))
        {
            blockHealth = other.GetComponent<BlockHealth>();
        }
        else if (other.CompareTag("ConnectionPoint"))
        {
            if (other.transform.parent != null && other.transform.parent.parent != null)
            {
                blockHealth = other.transform.parent.parent.GetComponent<BlockHealth>();
            }
        }

        if (blockHealth != null)
        {
            blockHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}