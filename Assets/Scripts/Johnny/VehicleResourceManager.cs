using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class VehicleResourceManager : MonoBehaviour
{
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
        if (onScrapChanged != null)
        {
            onScrapChanged.Invoke(scrapCount);
        }
    }
    void Update()
    {
        if (energyAmmoCount < maxEnergyAmmo && energyRechargeCoroutine == null &&
            Time.time - lastEnergyShotTime >= energyRechargeDelay)
        {
            energyRechargeCoroutine = StartCoroutine(EnergyRechargeRoutine());
        }
    }

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
    public void AddScrap(int amount)
    {
        scrapCount += amount;
        onScrapChanged?.Invoke(scrapCount);
    }

    public bool TryUseScrap(int amount)
    {
        if (scrapCount < amount)
            return false;

        scrapCount -= amount;
        onScrapChanged?.Invoke(scrapCount);
        return true;
    }
}
