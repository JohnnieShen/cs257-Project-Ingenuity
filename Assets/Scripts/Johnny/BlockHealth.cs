using UnityEngine;

public class BlockHealth : MonoBehaviour
{
    /*
    * Author: Johnny
    * Summary: This script is responsible for managing the health of a block in the game.
    * It allows the block to take damage and triggers an event when the block is damaged.
    * If the block's health reaches zero, it destroys the block.
    */

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

            // Reward player with scraps when an enemy core is destroyed
            if (blockType.BlockName == "Core" && tag == "EnemyBlock")
            {
                GameObject.Find("/BlockParent/CommandModule").GetComponent<VehicleResourceManager>().scrapCount += 100;
            }
        }
    }
}