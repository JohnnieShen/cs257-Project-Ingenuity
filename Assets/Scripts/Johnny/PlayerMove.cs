using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
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
