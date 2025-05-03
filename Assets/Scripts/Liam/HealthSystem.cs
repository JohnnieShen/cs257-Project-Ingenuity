using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    /* 
    * Author: Johnny
    * Summary: This script is responsible for managing the health of a vehicle in the game. It calculates the total health of the vehicle based on the health of its individual blocks.
    * The script listens for damage events from the blocks and updates the total health accordingly. It also provides methods to calculate the total health, maximum health, and health percentage.
    * The script uses UnityEvents to notify other components when the health changes.
    * This script is used to decide flee behavior in enemy AI.
    */
    public float maxHealth;
    public UnityEvent<float> OnHealthChanged = new UnityEvent<float>();

    /* Start is called before the first frame update.
    * It initializes the health system by subscribing to the OnDamaged event of each BlockHealth component in the children of this GameObject.
    * The OnDamaged event is triggered when the block takes damage, and it calls the HandleBlockDamaged method to update the total health.
    */
    void Start()
    {
        foreach (BlockHealth blockHealth in transform.parent.GetComponentsInChildren<BlockHealth>())
        {
            blockHealth.OnDamaged += HandleBlockDamaged;
        }
    }

    public void InitializeMaxHealth(float initialMax)
    {
        maxHealth = initialMax;
        OnHealthChanged?.Invoke(CalculateTotalHealth());
    }

    /* HandleBlockDamaged is called when a block takes damage.
    * It calculates the total health of the vehicle and invokes the OnHealthChanged event to notify other components about the health change.
    */
    void HandleBlockDamaged()
    {
        OnHealthChanged?.Invoke(CalculateTotalHealth());
    }

    /* CalculateTotalHealth calculates the total health of the vehicle by summing up the current health of all BlockHealth components in the children of this GameObject.
    * It returns the total health as a float value.
    */
    public float CalculateTotalHealth()
    {
        float total = 0f;
        foreach (BlockHealth healthComp in transform.parent.GetComponentsInChildren<BlockHealth>())
        {
            if (healthComp == null) continue;
            //Debug.Log("Current object: " + healthComp.gameObject.name);
            if (healthComp == null) continue;
            if (healthComp.transform.GetComponent<Hull>() != null && healthComp.transform.GetComponent<Hull>().canPickup) continue;
            total += healthComp.currentHealth;
            //Debug.Log("Current health: " + healthComp.currentHealth+" / " + total);
            
        }
        return total;
    }

    public float GetMaxHealth() => maxHealth;

    /* GetHealthPercentage calculates the health percentage of the vehicle by dividing the current health by the maximum health.
    * It returns the health percentage as a float value between 0 and 1.
    */
    public float GetHealthPercentage()
    {
        return CalculateTotalHealth() / maxHealth;
    }
}