using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;

    // Update is called once per frame
    void Update()
    {
        if (InputManager.instance == null) return;

        Vector2 move2D = Vector2.zero;
        if (InputManager.instance.GetBuildMoveAction() != null)
        {
            move2D = InputManager.instance.GetBuildMoveAction().ReadValue<Vector2>();
        }
        float x = move2D.x;
        float z = move2D.y;
        float y = 0f;
        if (InputManager.instance.GetBuildUpAction() != null &&
            InputManager.instance.GetBuildUpAction().IsPressed())
        {
            y += 1;
        }
        if (InputManager.instance.GetBuildDownAction() != null &&
            InputManager.instance.GetBuildDownAction().IsPressed())
        {
            y -= 1;
        }

        Vector3 move = transform.right * x + transform.up * y + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);
    }
}
