using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class VehicleResourceManager : MonoBehaviour
{
    /* 
    * Author: Johnny
    * Summary: This script manages the resources of a vehicle in the game. It keeps track of the ammo count for ballistic and energy weapons,
    * handles the energy recharge rate, and manages the scrap count. It also provides methods to add ammo, update UI elements, and handle scrap transactions.
    * The script is a singleton, ensuring that only one instance exists throughout the game. It uses UnityEvents to notify other components when the scrap count changes.
    */

    public static VehicleResourceManager Instance;
    public int ballisticAmmoCount = 50;
    public int energyAmmoCount = 30;
    public int maxBallisticAmmo = 50;
    public int maxEnergyAmmo = 30;
    public float energyRechargeDelay = 3f;
    public float energyRechargeRate = 1f;

    public Slider ballisticAmmoSlider;
    public Slider energyAmmoSlider;
    private float lastEnergyShotTime = 0f;
    private Coroutine energyRechargeCoroutine = null;
    public int scrapCount = 0;
    public UnityEvent<int> onScrapChanged;

    /* Awake is called when the script instance is being loaded.
    * It initializes the singleton instance and ensures that only one instance of this script exists in the scene.
    * If another instance is found, it destroys the new instance to maintain the singleton pattern.
    */
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /* Start is called before the first frame update.
    * It initializes the ammo sliders with their maximum values and sets the initial values for ballistic and energy ammo.
    * It also invokes the onScrapChanged event to update the scrap count in the UI.
    */
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
        if (onScrapChanged != null)
        {
            onScrapChanged.Invoke(scrapCount);
        }
    }

    /* Update is called once per frame.
    * It checks if the current mode is "Drive" and if the energy ammo count is less than the maximum allowed.
    * If so, it starts the energy recharge coroutine to replenish energy ammo over time.
    * If the mode is not "Drive", it stops the energy recharge coroutine if it is running.
    */
    void Update()
    {
        if (ModeSwitcher.instance != null && ModeSwitcher.instance.currentMode == ModeSwitcher.Mode.Drive)
        {
            if (energyAmmoCount < maxEnergyAmmo && energyRechargeCoroutine == null &&
                Time.time - lastEnergyShotTime >= energyRechargeDelay)
            {
                energyRechargeCoroutine = StartCoroutine(EnergyRechargeRoutine());
            }
        }
        else
        {
            if (energyRechargeCoroutine != null)
            {
                StopCoroutine(energyRechargeCoroutine);
                energyRechargeCoroutine = null;
            }
        }
    }

    /* OnTurretFired is called when a turret fires a shot.
    * It checks if the shot is energy-based or ballistic and deducts the corresponding ammo count.
    * It also updates the UI elements for the ammo count and resets the energy recharge coroutine if it was running.
    * Param1: isEnergy - A boolean indicating if the shot is energy-based.
    * Param2: ammoCost - The amount of ammo consumed for the shot.
    */
    public void OnTurretFired(bool isEnergy, int ammoCost)
    {
        if (isEnergy)
        {
            energyAmmoCount = Mathf.Max(energyAmmoCount - ammoCost, 0);
            UpdateEnergyAmmoUI();
            lastEnergyShotTime = Time.time;
            if (energyRechargeCoroutine != null)
            {
                StopCoroutine(energyRechargeCoroutine);
                energyRechargeCoroutine = null;
            }
        }
        else
        {
            ballisticAmmoCount = Mathf.Max(ballisticAmmoCount - ammoCost, 0);
            UpdateBallisticAmmoUI();
        }
    }

    /* AddAmmo is called to add a specified amount of ammo to either the energy or ballistic ammo count.
    * It ensures that the ammo count does not exceed the maximum allowed value.
    * Param1: isEnergy - A boolean indicating if the ammo being added is energy-based.
    * Param2: amount - The amount of ammo to add.
    */
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

    /* UpdateBallisticAmmoUI updates the ballistic ammo slider to reflect the current ammo count.
    * It sets the slider's value to the current ballistic ammo count.
    */
    public void UpdateBallisticAmmoUI()
    {
        if (ballisticAmmoSlider != null)
        {
            ballisticAmmoSlider.value = ballisticAmmoCount;
        }
    }

    /* UpdateEnergyAmmoUI updates the energy ammo slider to reflect the current ammo count.
    * It sets the slider's value to the current energy ammo count.
    */
    public void UpdateEnergyAmmoUI()
    {
        if (energyAmmoSlider != null)
        {
            energyAmmoSlider.value = energyAmmoCount;
        }
    }

    /* EnergyRechargeRoutine is a coroutine that replenishes the energy ammo over time.
    * It waits for a specified delay before starting to recharge the energy ammo at a specified rate.
    * It continues to add energy ammo until it reaches the maximum allowed value.
    * It also updates the energy ammo UI after each addition.
    */
    private IEnumerator EnergyRechargeRoutine()
    {
        while (energyAmmoCount < maxEnergyAmmo)
        {
            energyAmmoCount++;
            UpdateEnergyAmmoUI();
            yield return new WaitForSeconds(1f / energyRechargeRate);
        }
        energyRechargeCoroutine = null;
    }

    /* AddScrap is called to add a specified amount of scrap to the scrap count.
    * It updates the scrap count and invokes the onScrapChanged event to notify other components of the change.
    * Param1: amount - The amount of scrap to add.
    */
    public void AddScrap(int amount)
    {
        scrapCount += amount;
        onScrapChanged?.Invoke(scrapCount);
    }

    /* TryUseScrap is called to attempt to use a specified amount of scrap.
    * It checks if there is enough scrap available and deducts the amount if possible.
    * It also updates the scrap count and invokes the onScrapChanged event to notify other components of the change.
    * Param1: amount - The amount of scrap to use.
    * Returns: A boolean indicating if the scrap usage was successful.
    */
    public bool TryUseScrap(int amount)
    {
        if (scrapCount < amount)
            return false;

        scrapCount -= amount;
        onScrapChanged?.Invoke(scrapCount);
        return true;
    }

    /* Resets the last energy shot time to the current time.
    * This is used to track when the last energy shot was fired.
    */
    public void ResetLastEnergyShotTime()
    {
    lastEnergyShotTime = Time.time;
    }
}
