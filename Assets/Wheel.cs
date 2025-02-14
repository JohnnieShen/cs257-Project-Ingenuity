using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public Rigidbody rigidBody;
    public float suspensionRestDist;
    public float springStrength;
    public float springDamper;
    public float maxDistance;
    public float tireGripFactor;
    public float tireMass;
    public float accelForce;
    public float rotationSpeed;
    public GameObject tire;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, -rotationSpeed * Time.deltaTime, 0);
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Vector3 springDir = transform.up;
            Vector3 steeringDir = transform.right;
            Vector3 accelDir = transform.forward;
            float springOffset = suspensionRestDist - hit.distance;
            float springVel = Vector3.Dot(springDir, rigidBody.velocity);
            float steeringVel = Vector3.Dot(steeringDir, rigidBody.velocity);
            float desiredVelChange = -steeringVel * tireGripFactor;
            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
            float springForce = springOffset * springStrength - springVel * springDamper;
            float steeringForce = tireMass * desiredAccel;
            rigidBody.AddForce(springForce * springDir + steeringForce * steeringDir + accelDir * accelForce);

            // render tire
            tire.transform.position = hit.point + springDir / 2;
        }
        else
        {
            tire.transform.localPosition = -transform.up;
        }
    }
}
