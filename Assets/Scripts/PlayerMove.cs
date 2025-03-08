using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerMove : MonoBehaviour
{
    // Variables for moving
    Vector3 move;
    [SerializeField] float moveSpeed;

    // Variables for looking
    Vector2 look;
    [SerializeField] float lookSpeed;

    // Variables for camera
    [SerializeField] Transform cameraTransform;
    float cameraRotation; // Local copy of cameraTransform.eulerAngles.x clamped into the range [-90, 90]

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector3>() * moveSpeed;
    }

    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>() * lookSpeed;
    }

    void Update()
    {
        // Translate player
        transform.Translate(move.x * Time.deltaTime, move.y * Time.deltaTime, move.z * Time.deltaTime);

        // Rotate player left or right
        transform.Rotate(0f, look.x * Time.deltaTime, 0f);

        // Rotate camera up or down
        cameraRotation = Mathf.Clamp(cameraRotation - look.y * Time.deltaTime, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(cameraRotation, 0f, 0f);
    }
}
