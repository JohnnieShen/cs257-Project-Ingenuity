using UnityEngine;
using TMPro;

public class BossUIManager : MonoBehaviour
{
    public TextMeshProUGUI healthText;

    public GameObject bossCore;
    private HealthSystem healthSystem;

    void Start()
    {
        if (bossCore != null)
            healthSystem = bossCore.GetComponent<HealthSystem>();

        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.AddListener(OnHealthChanged);

            float initialCurrent = healthSystem.CalculateTotalHealth();
            float initialMax = healthSystem.GetMaxHealth();
            healthText.text = $"{initialCurrent:0}/{initialMax:0}";
        }
        else
        {
            Debug.LogWarning("BossUIManager: No HealthSystem found on bossCore.");
        }
    }
    void Update()
    {
        if (bossCore == null)
        {
            if (healthText != null)
                healthText.gameObject.SetActive(false);
            enabled = false;
        }
    }
    private void OnHealthChanged(float currentHealth)
    {
        float maxHealth = healthSystem.GetMaxHealth();
        healthText.text = $"{currentHealth:0}/{maxHealth:0}";
    }
}
