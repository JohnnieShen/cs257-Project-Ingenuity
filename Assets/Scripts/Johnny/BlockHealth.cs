using UnityEngine;

public class BlockHealth : MonoBehaviour
{
    public Block blockType;
    public float currentHealth;
    
    public event System.Action OnDamaged;

    void Start()
    {
        currentHealth = blockType.blockHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        OnDamaged?.Invoke();
        if (currentHealth <= 0)
        {
            Debug.Log("Block destroyed "+gameObject.name);
            Destroy(gameObject);
        }
    }
}