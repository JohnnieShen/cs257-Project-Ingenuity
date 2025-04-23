using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class BossUIManager : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public Image healthBar;

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
            if (healthText != null) healthText.gameObject.SetActive(false);
            if (healthBar != null) healthBar.gameObject.SetActive(false);
            enabled = false;
        }
    }
    private void OnHealthChanged(float currentHealth)
    {
        float maxHealth = healthSystem.GetMaxHealth();
        UpdateUI(currentHealth, maxHealth);
    }
    private void UpdateUI(float current, float max)
    {
        if (healthText != null)
            healthText.text = $"{current:0}/{max:0}";

        if (healthBar != null && max > 0f)
            healthBar.fillAmount = Mathf.Clamp01(current / max);
    }
}
