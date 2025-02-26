using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float launchForce;
    private float damage;
    public float timeToDestroy;

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
        GetComponent<Rigidbody>().AddRelativeForce(0, 0, launchForce);
    }

    public void SetDamage(float dmg)
    {
        //damage = dmg;
    }

    void OnTriggerEnter(Collider other)
    {
        //Destroy(gameObject);
    }
}
