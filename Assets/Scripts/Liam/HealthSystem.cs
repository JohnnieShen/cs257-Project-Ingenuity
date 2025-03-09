using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    public UnityEvent<float> OnHealthChanged;

    void Start()
    {
        foreach (BlockHealth blockHealth in GetComponentsInChildren<BlockHealth>())
        {
            blockHealth.OnDamaged += HandleBlockDamaged;
        }
    }

    void HandleBlockDamaged()
    {
        OnHealthChanged?.Invoke(CalculateTotalHealth());
    }

    public float CalculateTotalHealth()
    {
        float total = 0f;
        foreach (BlockHealth healthComp in GetComponentsInChildren<BlockHealth>())
        {
            total += healthComp.currentHealth;
        }
        return total;
    }

    public float CalculateMaxHealth()
    {
        float total = 0f;
        foreach (BlockHealth healthComp in GetComponentsInChildren<BlockHealth>())
        {
            total += healthComp.blockType.blockHealth;
        }
        return total;
    }
    public float GetHealthPercentage()
    {
        return CalculateTotalHealth() / CalculateMaxHealth();
    }
}