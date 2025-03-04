using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject planet; 
    public float speed;
    public float gravityStrength;
    public float rotationSpeed;

    private Rigidbody rb;
    public Vector3 jump;
    public float jumpForce;
    public bool isGrounded;



    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>(); //assigns the rigidbody component
        rb.useGravity = false; //turns off default gravity
        rb.constraints = RigidbodyConstraints.FreezeRotation; //keeps player upright

        jump = (planet.transform.position - transform.position).normalized;
    }

    void FixedUpdate()
    {
        jump = -(planet.transform.position - transform.position).normalized;
        ApplyGravity();
        HandleMovement();
    }

    void ApplyGravity()
    {
        Vector3 gravityDirection = (planet.transform.position - transform.position).normalized;

        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }
    void HandleMovement()
    {
        if (Input.GetKey(KeyCode.W)) {
            transform.Translate(0, 0, speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S)) {
            transform.Translate(0, 0, -speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A)) {
            transform.Translate(-speed * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey(KeyCode.D)) {
            transform.Translate(speed * Time.deltaTime, 0, 0);
        }
        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(0, mouseX * rotationSpeed * Time.deltaTime, 0);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) {
            rb.AddForce(jump * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }
    void OnCollisionStay() {
        isGrounded = true;
    }
}
