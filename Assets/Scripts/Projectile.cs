using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    private float damage;
    public float timeToDestroy;

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }
    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other) {
        Destroy(gameObject);
    }
}
