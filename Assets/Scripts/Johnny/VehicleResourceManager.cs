using UnityEngine;
using UnityEngine.UI;

public class VehicleResourceManager : MonoBehaviour
{
    public static VehicleResourceManager Instance;
    public int ballisticAmmoCount = 50;
    public int energyAmmoCount = 30;
    public int maxBallisticAmmo = 50;
    public int maxEnergyAmmo = 30;

    public Slider ballisticAmmoSlider;
    public Slider energyAmmoSlider;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (ballisticAmmoSlider != null)
        {
            ballisticAmmoSlider.maxValue = maxBallisticAmmo;
            ballisticAmmoSlider.value = ballisticAmmoCount;
        }
        if (energyAmmoSlider != null)
        {
            energyAmmoSlider.maxValue = maxEnergyAmmo;
            energyAmmoSlider.value = energyAmmoCount;
        }
    }

    public void OnTurretFired(bool isEnergy, int ammoCost)
    {
        if (isEnergy)
        {
            energyAmmoCount = Mathf.Max(energyAmmoCount - ammoCost, 0);
            UpdateEnergyAmmoUI();
        }
        else
        {
            ballisticAmmoCount = Mathf.Max(ballisticAmmoCount - ammoCost, 0);
            UpdateBallisticAmmoUI();
        }
    }

    public void AddAmmo(bool isEnergy, int amount)
    {
        if (isEnergy)
        {
            energyAmmoCount = Mathf.Min(energyAmmoCount + amount, maxEnergyAmmo);
            UpdateEnergyAmmoUI();
        }
        else
        {
            ballisticAmmoCount = Mathf.Min(ballisticAmmoCount + amount, maxBallisticAmmo);
            UpdateBallisticAmmoUI();
        }
    }

    private void UpdateBallisticAmmoUI()
    {
        if (ballisticAmmoSlider != null)
        {
            ballisticAmmoSlider.value = ballisticAmmoCount;
        }
    }

    private void UpdateEnergyAmmoUI()
    {
        if (energyAmmoSlider != null)
        {
            energyAmmoSlider.value = energyAmmoCount;
        }
    }
}
