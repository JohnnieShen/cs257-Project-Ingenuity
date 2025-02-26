using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    private float damage;
    public float timeToDestroy;

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
        GetComponent<Rigidbody>().AddRelativeForce(0, 0, speed);
    }

    public void SetDamage(float dmg)
    {
        //damage = dmg;
    }

    void OnTriggerEnter(Collider other) {
        //Destroy(gameObject);
    }
}
