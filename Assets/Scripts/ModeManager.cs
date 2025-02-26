using UnityEngine;
using System.Collections;

public class ModeSwitcher : MonoBehaviour
{
    public enum Mode { Build, Drive }
    public Mode currentMode = Mode.Drive;

    public GameObject player;

    // public GameObject vehicle;
    public GameObject driveCameraPivot;
    public Transform vehicleRoot;
    public Transform drivingCamera;
    public float buildModeHeight = 5f;
    public float elevateDuration = 1f;
    
    void Start()
    {
        SetMode(currentMode);
    }

    void Update()
    {
        if ((InputManager.instance.GetBuildSwapModeAction()  != null &&
             InputManager.instance.GetBuildSwapModeAction().triggered) ||
            (InputManager.instance.GetDriveSwapModeAction()  != null &&
             InputManager.instance.GetDriveSwapModeAction().triggered))
        {
            currentMode = (currentMode == Mode.Build) ? Mode.Drive : Mode.Build;
            SetMode(currentMode);
        }
    }

    void SetMode(Mode mode)
    {
        if (mode == Mode.Build)
        {
            InputManager.instance.EnableBuildMap();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (player != null)
            {
                player.SetActive(true);
                if(drivingCamera != null)
                {
                    player.transform.position = drivingCamera.position;
                }
            }
            // if(buildCamera != null) buildCamera.gameObject.SetActive(true);

            // if(vehicle != null) vehicle.SetActive(false);
            if(driveCameraPivot != null) driveCameraPivot.SetActive(false);

            if(BlockManager.instance != null)
            {
                BlockManager.instance.DisableVehiclePhysics();
            }
            if (vehicleRoot != null)
            {
                StopAllCoroutines();
                StartCoroutine(ElevateVehicle(buildModeHeight, elevateDuration));
            }
        }
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
        }
    }
    IEnumerator ElevateVehicle(float targetHeight, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = vehicleRoot.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y + targetHeight, startPos.z);
        Quaternion startRot = vehicleRoot.localRotation;
        Quaternion endRot = Quaternion.identity;
        Debug.Log("Rotating vehicle from " + startRot + " to " + endRot);

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
