using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Look : MonoBehaviour
{
    // Variables for looking
    Vector2 look;
    [SerializeField] float sensitivity;

    // Variables for camera
    [SerializeField] Transform cameraTransform;
    float cameraRotation; // Local copy of cameraTransform.eulerAngles.x clamped into the range [-90, 90]

    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>() * sensitivity;
    }

    void Update()
    {
        // Rotate player left or right
        transform.Rotate(0f, look.x * Time.deltaTime, 0f);

        // Rotate camera up or down
        cameraRotation = Mathf.Clamp(cameraRotation - look.y * Time.deltaTime, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(cameraRotation, 0f, 0f);
    }
}
