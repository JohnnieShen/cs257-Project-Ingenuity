using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    float xRotation = 0f;
    float yRotation = 0f;
    public float rotationSmoothTime = 10f;

    void Update()
    {
        if (InputManager.instance == null) return;

        Vector2 lookValue = Vector2.zero;
        if (InputManager.instance.GetBuildLookAction() != null)
        {
            lookValue = InputManager.instance.GetBuildLookAction().ReadValue<Vector2>();
        }

        float mouseX = lookValue.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookValue.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;

        Quaternion targetCameraRotation = Quaternion.Euler(xRotation, 0f, 0f);
        Quaternion targetPlayerRotation = Quaternion.Euler(0f, yRotation, 0f);

        // transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetCameraRotation, rotationSmoothTime * Time.deltaTime);
        if (playerBody != null)
        {
            playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetPlayerRotation, rotationSmoothTime * Time.deltaTime);
        }
    }
}
