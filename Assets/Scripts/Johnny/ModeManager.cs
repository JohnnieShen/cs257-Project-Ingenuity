using UnityEngine;
using System.Collections;

public class ModeSwitcher : MonoBehaviour
{
    public static ModeSwitcher instance;
    public BuildSystem build;
    public enum Mode { Build, Drive , Craft };
    public Mode currentMode = Mode.Drive;

    public GameObject player;

    // public GameObject vehicle;
    public GameObject driveCameraPivot;
    public Transform vehicleRoot;
    public Transform drivingCamera;
    public GameObject buildUI;
    public GameObject driveUI;
    public GameObject craftUI;
    public float buildModeHeight = 5f;
    public float elevateDuration = 1f;
    public bool canManuallySwitchMode = true;
    public float modeSwitchCooldown = 0.5f;
    private float lastModeSwitchTime = 0f;
    private Vector3 originalVehiclePosition;
    private Quaternion originalVehicleRotation;
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
        if(vehicleRoot != null)
        {
            originalVehiclePosition = vehicleRoot.position;
            originalVehicleRotation = vehicleRoot.localRotation;
        }
        SetMode(currentMode);
    }

    void Update()
    {
        if (!canManuallySwitchMode || Time.time - lastModeSwitchTime < modeSwitchCooldown)
            return;
        if (canManuallySwitchMode)
        {
            bool buildSwapTriggered = InputManager.instance.GetBuildSwapModeAction() != null && InputManager.instance.GetBuildSwapModeAction().triggered;
            bool driveSwapTriggered = InputManager.instance.GetDriveSwapModeAction() != null && InputManager.instance.GetDriveSwapModeAction().triggered;

            if (buildSwapTriggered || driveSwapTriggered)
            {
                if (currentMode == Mode.Drive)
                {
                    currentMode = Mode.Build;
                    SetMode(currentMode);
                    lastModeSwitchTime = Time.time;
                    if(vehicleRoot != null)
                    {
                        originalVehiclePosition = vehicleRoot.position;
                        originalVehicleRotation = vehicleRoot.localRotation;
                    }
                    if (vehicleRoot != null)
                    {
                        StopAllCoroutines();
                        StartCoroutine(ElevateVehicle(buildModeHeight, elevateDuration));
                    }
                    if (player != null)
                    {
                        if(drivingCamera != null)
                        {
                            player.transform.position = drivingCamera.position;
                        }
                    }
                    return;
                }
                else if (currentMode == Mode.Build)
                {
                    currentMode = Mode.Drive;
                    SetMode(currentMode);
                    // if (vehicleRoot != null)
                    // {
                    //     StopAllCoroutines();
                    //     vehicleRoot.position = originalVehiclePosition;
                    //     vehicleRoot.localRotation = originalVehicleRotation;
                    // }
                    lastModeSwitchTime = Time.time;
                    return;
                }
            }
        }
        if (canManuallySwitchMode && currentMode == Mode.Build){
            if (InputManager.instance.GetBuildMenuAction() != null && InputManager.instance.GetBuildMenuAction().triggered)
            {
                currentMode = Mode.Craft;
                SetMode(currentMode);
                lastModeSwitchTime = Time.time;
                return;
            }
        }
        if (canManuallySwitchMode && currentMode == Mode.Craft)
        {
            if ((InputManager.instance.GetUIMenuAction() != null && InputManager.instance.GetUIMenuAction().triggered) || InputManager.instance.GetUIEscapeAction() != null && InputManager.instance.GetUIEscapeAction().triggered)
            {
                currentMode = Mode.Build;
                SetMode(currentMode);
                lastModeSwitchTime = Time.time;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                return;
            }
        }
    }

    public void SetMode(Mode mode)
    {
        if (mode == Mode.Build)
        {
            // Time.timeScale = 1f;
            if (buildUI != null) buildUI.SetActive(true);
            if (driveUI != null) driveUI.SetActive(false);
            if (craftUI != null) craftUI.SetActive(false);
            InputManager.instance.EnableBuildMap();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (player != null)
            {
                player.SetActive(true);
            }
            // if(buildCamera != null) buildCamera.gameObject.SetActive(true);

            // if(vehicle != null) vehicle.SetActive(false);
            if(driveCameraPivot != null) driveCameraPivot.SetActive(false);

            if(BlockManager.instance != null)
            {
                BlockManager.instance.DisableVehiclePhysics();
            }
            // if (vehicleRoot != null)
            // {
            //     StopAllCoroutines();
            //     StartCoroutine(ElevateVehicle(buildModeHeight, elevateDuration));
            // }
            if (EnemyBlockManager.instance != null)
            {
                foreach (EnemyAI enemy in EnemyBlockManager.instance.GetEnemyVehicles())
                {
                    if (enemy != null)
                    {
                        enemy.enabled = false;
                    }
                }
            }
        }
        // Vice versa
        else if (mode == Mode.Drive)
        {
            // Time.timeScale = 1f;
            if (buildUI != null) buildUI.SetActive(false);
            if (driveUI != null) driveUI.SetActive(true);
            if (craftUI != null) craftUI.SetActive(false);
            InputManager.instance.EnableDriveMap();
            // if(vehicle != null) vehicle.SetActive(true);
            if(driveCameraPivot != null) driveCameraPivot.SetActive(true);
            build.destroyPreviewBlock();
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
        } else if (mode == Mode.Craft)
        {
            // Time.timeScale = 0f;
            if (buildUI != null) buildUI.SetActive(false);
            if (driveUI != null) driveUI.SetActive(false);
            if (craftUI != null) craftUI.SetActive(true);
            InputManager.instance.EnableUIMap();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
