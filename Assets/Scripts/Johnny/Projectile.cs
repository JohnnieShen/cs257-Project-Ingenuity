using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool IsEnemyProjectile = false;
    public float speed;
    public float ballisticDamage = 10f;
    public float energyDamage = 10f;
    public float timeToDestroy = 10f;

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void SetDamage(float ballisticDmg, float energyDmg)
    {
        ballisticDamage = ballisticDmg;
        energyDamage = energyDmg;
    }

    void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Projectile hit: " + other.name + " " + other.tag);
        BlockHealth blockHealth = null;
        if (IsEnemyProjectile)
        {
            if (other.CompareTag("Block") || other.CompareTag("Core"))
            {
                blockHealth = other.GetComponent<BlockHealth>();
            }
            else if (other.CompareTag("ConnectionPoint"))
            {
                if (other.transform.parent != null && other.transform.parent.parent != null)
                {
                    if (other.transform.parent.parent.CompareTag("Block") || other.transform.parent.parent.CompareTag("Core"))
                    {
                        blockHealth = other.transform.parent.parent.GetComponent<BlockHealth>();
                    }
                }
            }
        }
        else
        {
            if (other.CompareTag("EnemyBlock"))
            {
                blockHealth = other.GetComponent<BlockHealth>();
            }
            else if (other.CompareTag("ConnectionPoint"))
            {
                if (other.transform.parent != null && other.transform.parent.parent != null)
                {
                    if (other.transform.parent.parent.CompareTag("EnemyBlock"))
                    {
                        blockHealth = other.transform.parent.parent.GetComponent<BlockHealth>();
                    }
                }
            }
        }
        // if (other.CompareTag("EnemyBlock"))
        // {
        //     blockHealth = other.GetComponent<BlockHealth>();
        // }
        // else if (other.CompareTag("ConnectionPoint"))
        // {
        //     if (other.transform.parent != null && other.transform.parent.parent != null)
        //     {
        //         blockHealth = other.transform.parent.parent.GetComponent<BlockHealth>();
        //     }
        // }

        if (blockHealth != null)
        {
            blockHealth.TakeDamage(ballisticDamage);
            Destroy(gameObject);
        }
    }
}