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
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float y = 0;
        if (Input.GetKey(KeyCode.Space))
        {
            y += 1;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            y -= 1;
        }


        Vector3 move = transform.right * x + transform.up * y + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);
    }
}
