using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitPlanet : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public Transform center;
    public float verticalRotationLimit = 0f; 

    private float currentVerticalAngle = 0f;

    void Update()
    {
        HandleCameraRotation();
    }

    void HandleCameraRotation()
    {
        if (Input.GetMouseButton(0)) 
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            transform.RotateAround(center.position, Vector3.up, mouseX * rotationSpeed * Time.deltaTime);

            float newVerticalAngle = currentVerticalAngle - mouseY * rotationSpeed * Time.deltaTime;
            newVerticalAngle = Mathf.Clamp(newVerticalAngle, -verticalRotationLimit, verticalRotationLimit);

            float angleDifference = newVerticalAngle - currentVerticalAngle;
            currentVerticalAngle = newVerticalAngle;

            transform.RotateAround(center.position, transform.right, angleDifference);

            transform.LookAt(center.position);
        }
    }
}
