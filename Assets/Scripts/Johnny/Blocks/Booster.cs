using UnityEngine;
using UnityEngine.InputSystem;

public class Booster : MonoBehaviour
{
    /*
    Author: Johnny
    Summary: This script is attached to a booster block in the game. It manages the boost effect on the vehicle when the player activates it.
    The booster block applies a force to the vehicle in the opposite direction of its up vector, simulating a boost effect.
    The boost effect consumes energy from the vehicle's energy, and the amount of energy consumed is configurable.
    */
    public float boostForceMagnitude = 10f;
    public ParticleSystem boostParticles;

    private Rigidbody rb;
    private InputAction driveBoostAction;
    private bool isBoosting = false;
    public float energyConsumptionPerSecond = 5f;
    private float energyConsumptionAccumulator = 0f;

    /**
    * Awake is called when the script instance is being loaded.
    * It initializes the Rigidbody component and checks if it is present.
    */
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody found on this block");
        }
        hull = GetComponent<Hull>();
        if (hull == null)
        {
            Debug.LogWarning("No Hull component found on this block");
        }
    }

    /**
    * Start is called before the first frame update.
    * It initializes listeners for the drive boost action from the InputManager.
    */
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

    /**
    * OnDisable is called when the behaviour becomes disabled or inactive.
    * It removes listeners for the drive boost action to prevent memory leaks.
    */
    void OnDisable()
    {
        if (driveBoostAction != null)
        {
            driveBoostAction.started -= StartBoost;
            driveBoostAction.canceled -= StopBoost;
        }
    }

    /**
    * StartBoost is called when the drive boost action is started.
    * It checks if the vehicle has enough energy to boost and starts the boost effect.
    * If the vehicle does not have enough energy, it stops the boost effect.
    * Param1: context - The input action context that contains information about the input event.
    */
    private void StartBoost(InputAction.CallbackContext context)
    {
        if (hull != null && hull.canPickup)
        {
            return;
        }
        if (VehicleResourceManager.Instance != null && VehicleResourceManager.Instance.energyAmmoCount > 0)
        {
            isBoosting = true;
            if (boostParticles != null && !boostParticles.isPlaying)
            {
                boostParticles.Play();
            }
        }
    }

    /**
    * StopBoost is called when the drive boost action is canceled.
    * It acts as a wrapper and stops the boost effect and resets the energy consumption accumulator.
    * Param1: context - The input action context that contains information about the input event.
    */
    private void StopBoost(InputAction.CallbackContext context)
    {
        ForceStopBoosting();
    }

    /**
    * ForceStopBoosting is called to forcefully stop the boost effect.
    * It sets the isBoosting flag to false, stops the boost particles, and resets the energy consumption accumulator.
    */
    private void ForceStopBoosting()
    {
        isBoosting = false;
        if (boostParticles != null && boostParticles.isPlaying)
        {
            boostParticles.Stop();
        }
        energyConsumptionAccumulator = 0f;
    }

    /**
    * FixedUpdate is called every fixed frame-rate frame.
    * It checks if the vehicle is boosting and if the Rigidbody component is present.
    * If so, it consumes energy from the vehicle and applies a force to the vehicle in the opposite direction of its up vector.
    * It also updates the energy ammo UI in the VehicleResourceManager.
    */
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
