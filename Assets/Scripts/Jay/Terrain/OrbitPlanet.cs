using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitPlanet : MonoBehaviour
{
    /*
    Author: Jay
    Summary: (oudated) allowed for a player object to orbit around a planetary object,
    allowed for the simulation of walking across a tiny planet. Was only used
    for original concept mapping, not implemented in our current version.
    */

    public float rotationSpeed = 100f;
    public Transform center;
    public float verticalRotationLimit = 0f; 

    private float currentVerticalAngle = 0f;

    void Update()
    {
        HandleCameraRotation();
    }

    /*
    Camera rotation scripting to follow the player in an intended manner,
    ensures that the camera doesn't have unintended behavior at the axes
    */
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
