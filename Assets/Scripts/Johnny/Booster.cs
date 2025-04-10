using UnityEngine;
using UnityEngine.InputSystem;

public class Booster : MonoBehaviour
{
    public float boostForceMagnitude = 10f;
    public ParticleSystem boostParticles;

    private Rigidbody rb;
    private InputAction driveBoostAction;
    private bool isBoosting = false;
    public float energyConsumptionPerSecond = 5f;
    private float energyConsumptionAccumulator = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody found on this block");
        }
    }

    void OnEnable()
    {
        if (InputManager.instance != null)
        {
            driveBoostAction = InputManager.instance.GetDriveBoostAction();
            if (driveBoostAction != null)
            {
                driveBoostAction.started += StartBoost;
                driveBoostAction.canceled += StopBoost;
            }
        }
    }

    void OnDisable()
    {
        if (driveBoostAction != null)
        {
            driveBoostAction.started -= StartBoost;
            driveBoostAction.canceled -= StopBoost;
        }
    }

    private void StartBoost(InputAction.CallbackContext context)
    {
        if (VehicleResourceManager.Instance != null && VehicleResourceManager.Instance.energyAmmoCount > 0)
        {
            isBoosting = true;
            if (boostParticles != null && !boostParticles.isPlaying)
            {
                boostParticles.Play();
            }
        }
    }

    private void StopBoost(InputAction.CallbackContext context)
    {
        ForceStopBoosting();
    }

    private void ForceStopBoosting()
    {
        isBoosting = false;
        if (boostParticles != null && boostParticles.isPlaying)
        {
            boostParticles.Stop();
        }
        energyConsumptionAccumulator = 0f;
    }

    void FixedUpdate()
    {
        if (isBoosting && rb != null && VehicleResourceManager.Instance != null)
        {
            if (VehicleResourceManager.Instance.energyAmmoCount <= 0)
            {
                ForceStopBoosting();
                return;
            }

            energyConsumptionAccumulator += energyConsumptionPerSecond * Time.fixedDeltaTime;
            int energyToConsume = Mathf.FloorToInt(energyConsumptionAccumulator);
            if (energyToConsume > 0)
            {
                VehicleResourceManager.Instance.energyAmmoCount =
                    Mathf.Max(VehicleResourceManager.Instance.energyAmmoCount - energyToConsume, 0);

                energyConsumptionAccumulator -= energyToConsume;
                VehicleResourceManager.Instance.ResetLastEnergyShotTime();
            }

            rb.AddForce(-transform.up * boostForceMagnitude, ForceMode.Impulse);
            VehicleResourceManager.Instance.UpdateEnergyAmmoUI();
        }
    }
}
