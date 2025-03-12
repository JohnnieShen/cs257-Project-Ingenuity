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
    [SerializeField] float speed;

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector3>() * speed;
    }

    void Update()
    {
        // Translate player
        transform.Translate(move.x * Time.deltaTime, move.y * Time.deltaTime, move.z * Time.deltaTime);
    }
}
