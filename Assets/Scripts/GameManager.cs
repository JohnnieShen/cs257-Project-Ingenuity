using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    void Awake()
    {
        // Hide cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
