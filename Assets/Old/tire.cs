using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Tire : MonoBehaviour
{
    public float suspensionRestDist;
    public float springStrength;
    public float springDamper;
    public float tireGripFactor;
    public float tireMass;
    public bool steering;
    public GameObject mesh;
    public GameObject car;

    Ray ray;
    bool rayDidHit;
    Transform tireTransform;
    Rigidbody carRigidBody;

    // Start is called before the first frame update
    void Start()
    {
        tireTransform = transform;
        carRigidBody = car.GetComponent<Rigidbody>();
    }

    // FixedUpdate is called a fixed number of times per second regardless of frame rate
    void FixedUpdate()
    {
        // cast ray
        ray = new Ray(transform.position, -transform.up);

        // check for hit
        rayDidHit = Physics.Raycast(ray, out RaycastHit tireRay);

        // calculate forces
        if (rayDidHit)
        {
            mesh.transform.position = transform.position + new Vector3(0, - tireRay.distance + 0.5f, 0);

            // world-space directions of forces
            Vector3 springDir = tireTransform.up;
            Vector3 steeringDir = tireTransform.right;
            Vector3 accelerationDir = tireTransform.forward;

            // world-space velocity of this tire
            Vector3 tireWorldVel = carRigidBody.GetPointVelocity(tireTransform.position);

            // calculate offset from the raycast
            float offset = suspensionRestDist - tireRay.distance;

            // project velocity in each direction
            // note that directions are unit vectors, so this calculates the magnitude of tireWorldVel
            // as a projection onto each direction
            float springVel = Vector3.Dot(springDir, tireWorldVel);
            float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);

            // the change in velocity that we're looking for is -steeringVal * gripFactor
            // gripFactor is in range 0-1, 0 means no grip, 1 means full grip
            float desiredVelChange = -steeringVel * tireGripFactor;

            // turn change in velocity into an acceleration (acceleration = change in vel / time)
            // this will produce the acceleration necessary to change the velocity by desiredVelChange in 1 physics step
            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

            // calculate the magnitude of the forces
            float springForce = (offset * springStrength) - (springVel * springDamper); // damped spring
            float steeringForce = tireMass * desiredAccel; // tire friction

            // apply the force at the location of this tire, in the direction
            // of the suspension
            carRigidBody.AddForceAtPosition(springDir * springForce + steeringDir * steeringForce + accelerationDir * 10, tireTransform.position);
        }
    }
}
