using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    /*
    * Author: Liam
    * Summary: This script handles the player movement in the game. It uses a CharacterController to move the player based on input from the InputManager.
    * The player can move in 3D space using WASD or arrow keys, and can also move up and down using the assigned input actions. The movement is smoothed for a better experience.
    * The script also restricts the player's movement within a specified radius around a center point, if provided.
    */

    public CharacterController controller;
    public float speed = 12f;
    
    public float smoothingSpeed = 10f;
    public Transform moveCenter;
    public float movementRadius = 10f;
    private Vector3 currentMove = Vector3.zero;

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

        Vector3 targetMove = transform.right * x + transform.up * y + transform.forward * z;
        currentMove = Vector3.Lerp(currentMove, targetMove, smoothingSpeed * Time.deltaTime);

        controller.Move(currentMove * speed * Time.deltaTime);

        if (moveCenter != null)
        {
            Vector3 offset = transform.position - moveCenter.position;
            if (offset.magnitude > movementRadius)
            {
                transform.position = moveCenter.position + offset.normalized * movementRadius;
            }
        }
    }
}
