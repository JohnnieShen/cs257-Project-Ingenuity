using UnityEngine;
using System.Collections;

public class ModeSwitcher : MonoBehaviour
{
    public static ModeSwitcher instance;
    public enum Mode { Build, Drive }
    public Mode currentMode = Mode.Drive;

    public GameObject player;

    // public GameObject vehicle;
    public GameObject driveCameraPivot;
    public Transform vehicleRoot;
    public Transform drivingCamera;
    public float buildModeHeight = 5f;
    public float elevateDuration = 1f;
    public bool canManuallySwitchMode = true;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    void Start()
    {
        SetMode(currentMode);
    }

    void Update()
    {
        if (
            canManuallySwitchMode &&
            (
                (InputManager.instance.GetBuildSwapModeAction() != null &&
                InputManager.instance.GetBuildSwapModeAction().triggered) 
                ||
                (InputManager.instance.GetDriveSwapModeAction() != null &&
                InputManager.instance.GetDriveSwapModeAction().triggered)
            )
        )
        {
            currentMode = (currentMode == Mode.Build) ? Mode.Drive : Mode.Build;
            SetMode(currentMode);
        }
    }

    // Set the mode of the game
    public void SetMode(Mode mode)
    {
        if (mode == Mode.Build)
        {
            InputManager.instance.EnableBuildMap(); // Switching to the build input map
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (player != null)
            {
                player.SetActive(true);
                if(drivingCamera != null)
                {
                    player.transform.position = drivingCamera.position + new Vector3(0f, 6f, 0f); // Set the player position to the driving camera position
                }
            }
            // if(buildCamera != null) buildCamera.gameObject.SetActive(true);

            // if(vehicle != null) vehicle.SetActive(false);
            if(driveCameraPivot != null) driveCameraPivot.SetActive(false); // Disable the drive camera

            if(BlockManager.instance != null)
            {
                BlockManager.instance.DisableVehiclePhysics(); // Disable the vehicle physics
            }
            if (vehicleRoot != null)
            {
                StopAllCoroutines();
                StartCoroutine(ElevateVehicle(buildModeHeight, elevateDuration)); // Elevate the vehicle to +5f on the y-axis
            }
            if (EnemyBlockManager.instance != null)
            {
                foreach (EnemyAI enemy in EnemyBlockManager.instance.GetEnemyVehicles())
                {
                    if (enemy != null) // For enemies that are registered and still alive
                    {
                        enemy.enabled = false; // Disable all enemy AI
                    }
                }
            }
        }
        // Vice versa
        else if (mode == Mode.Drive)
        {
            InputManager.instance.EnableDriveMap();
            // if(vehicle != null) vehicle.SetActive(true);
            if(driveCameraPivot != null) driveCameraPivot.SetActive(true);

            if(player != null) player.SetActive(false);
            // if(buildCamera != null) buildCamera.gameObject.SetActive(false);

            if(BlockManager.instance != null)
            {
                BlockManager.instance.EnableVehiclePhysics();
            }
            if (EnemyBlockManager.instance != null)
            {
                foreach (EnemyAI enemy in EnemyBlockManager.instance.GetEnemyVehicles())
                {
                    if (enemy != null)
                    {
                        enemy.enabled = true;
                    }
                }
            }
        }
    }
    IEnumerator ElevateVehicle(float targetHeight, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = vehicleRoot.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y + targetHeight, startPos.z);
        Quaternion startRot = vehicleRoot.localRotation;
        Quaternion endRot = Quaternion.identity;
        // Debug.Log("Rotating vehicle from " + startRot + " to " + endRot);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            vehicleRoot.position = Vector3.Lerp(startPos, endPos, t);
            vehicleRoot.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        vehicleRoot.position = endPos;
        vehicleRoot.localRotation = endRot;
    }
}
