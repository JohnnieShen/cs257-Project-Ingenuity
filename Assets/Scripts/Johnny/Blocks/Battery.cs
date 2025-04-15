using UnityEngine;

public class Battery : MonoBehaviour
{
    /*
    * Author: Johnny
    * Summary: This script is attached to a battery block in the game. It manages the battery's energy boost effect on the vehicle.
    * Every battery block increases the vehicle's maximum energy by a specified amount.
    */
    public int energyBoostAmount = 10;

    private Hull hull;
    private bool energyBoostApplied = false;

    /**
    * Awake is called when the script instance is being loaded.
    * It initializes the hull component and checks if it is present.
    */
    void Awake()
    {
        hull = GetComponent<Hull>();
        if (hull == null)
        {
            Debug.LogError("BatteryBlock: Hull component not found on battery block.");
        }
    }
    /**
    * Start is called before the first frame update.
    * It checks if the hull is not null and if the battery is attached to some vehicle.
    * If so, it applies the battery effect to the vehicle's energy.
    */
    void Start()
    {
        if (hull != null && !hull.canPickup && VehicleResourceManager.Instance != null)
        {
            ApplyBatteryEffect();
        }
        // TODO: what happens if battery is attached to AI vehicle
    }

    /**
    * Update is called once per frame.
    * It checks if the battery effect is applied and if the hull is no longer attached.
    * If so, it removes the battery effect from the vehicle's energy.
    */
    void Update()
    {
        if (energyBoostApplied && hull != null && hull.canPickup)
        {
            RemoveBatteryEffect();
        }
    }

    /**
    * OnDestroy is called when the MonoBehaviour will be destroyed.
    * It checks if the battery effect is applied and removes it from the vehicle's energy.
    */
    void OnDestroy()
    {
        if (energyBoostApplied && VehicleResourceManager.Instance != null)
        {
            RemoveBatteryEffect();
        }
    }

    /**
    * ApplyBatteryEffect applies the battery effect to the vehicle's energy.
    * It increases the maximum energy by the specified amount and updates the energy slider.
    */
    private void ApplyBatteryEffect()
    {
        if (VehicleResourceManager.Instance != null)
        {
            VehicleResourceManager.Instance.maxEnergyAmmo += energyBoostAmount;
            UpdateEnergySlider();
            energyBoostApplied = true;
        }
    }

    /**
    * RemoveBatteryEffect removes the battery effect from the vehicle's energy.
    * It decreases the maximum energy by the specified amount and updates the energy slider.
    */
    private void RemoveBatteryEffect()
    {
        if (VehicleResourceManager.Instance != null)
        {
            VehicleResourceManager.Instance.maxEnergyAmmo -= energyBoostAmount;
            UpdateEnergySlider();
            energyBoostApplied = false;
        }
    }

    /**
    * UpdateEnergySlider updates the energy slider to reflect the current maximum energy.
    * It sets the maximum value of the energy slider to the vehicle's maximum energy.
    */
    private void UpdateEnergySlider()
    {
        if (VehicleResourceManager.Instance.energyAmmoSlider != null)
        {
            VehicleResourceManager.Instance.energyAmmoSlider.maxValue = VehicleResourceManager.Instance.maxEnergyAmmo;
        }
    }
}
