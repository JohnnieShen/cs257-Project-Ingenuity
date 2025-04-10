using UnityEngine;

public class Battery : MonoBehaviour
{
    public int energyBoostAmount = 10;

    private Hull hull;
    private bool energyBoostApplied = false;

    void Awake()
    {
        hull = GetComponent<Hull>();
        if (hull == null)
        {
            Debug.LogError("BatteryBlock: Hull component not found on battery block.");
        }
    }

    void Start()
    {
        if (hull != null && !hull.canPickup && VehicleResourceManager.Instance != null)
        {
            ApplyBatteryEffect();
        }
    }

    void Update()
    {
        if (energyBoostApplied && hull != null && hull.canPickup)
        {
            RemoveBatteryEffect();
        }
    }

    void OnDestroy()
    {
        if (energyBoostApplied && VehicleResourceManager.Instance != null)
        {
            RemoveBatteryEffect();
        }
    }

    private void ApplyBatteryEffect()
    {
        if (VehicleResourceManager.Instance != null)
        {
            VehicleResourceManager.Instance.maxEnergyAmmo += energyBoostAmount;
            UpdateEnergySlider();
            energyBoostApplied = true;
        }
    }

    private void RemoveBatteryEffect()
    {
        if (VehicleResourceManager.Instance != null)
        {
            VehicleResourceManager.Instance.maxEnergyAmmo -= energyBoostAmount;
            UpdateEnergySlider();
            energyBoostApplied = false;
        }
    }

    private void UpdateEnergySlider()
    {
        if (VehicleResourceManager.Instance.energyAmmoSlider != null)
        {
            VehicleResourceManager.Instance.energyAmmoSlider.maxValue = VehicleResourceManager.Instance.maxEnergyAmmo;
        }
    }
}
