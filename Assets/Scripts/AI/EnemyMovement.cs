using UnityEngine;


/**
    * This script is responsible for the movement of the enemy.
    * It should be used in conjunction with the EnemyAI script.
    * For now, to manipulate movement, you just need to set the targetPosition variable to the target position you want the enemy to move towards.
    */  
public class EnemyMovement : MonoBehaviour
{
    public float maxSpeed = 20f;
    public float arrivalDistance = 2f;
    public float reverseThreshold = 0.5f;
    public float steeringSharpness = 2f;
    public float accelerationSharpness = 2f;
    public float brakeStrength = 100f;
    public float slowDownRadius = 10f;

    public Transform targetPosition; // Change this for movement
    public Rigidbody rb;
    public Wheel[] wheels;

    public float currentDriveInput;
    public float currentSteerInput;
    
    // TODO bool for if movement is enabled
    void Start()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.isAI = true; // Set the isAI flag to true for all wheels, so they don't respond to player input.
        }
    }
    void FixedUpdate()
    {
        if (targetPosition == null) return;

        Vector3 toTarget = targetPosition.position - transform.position; // Calculate the vector to the target position.
        float distanceToTarget = toTarget.magnitude; // Calculate the distance to the target position.
        
        Vector3 localTargetDir = transform.InverseTransformDirection(toTarget.normalized); // Calculate the local target direction.
        currentSteerInput = Mathf.Clamp(localTargetDir.x * steeringSharpness, -1f, 1f); // Calculate the steering input based on the local target direction.

        float targetSpeed = CalculateTargetSpeed(toTarget, distanceToTarget); // Calculate the target speed based on the distance to the target.
        UpdateDriveInput(targetSpeed, distanceToTarget);
        UpdateWheels();
        ApplyBraking(distanceToTarget);
    }

    // Calculate the target speed based on the distance to the target.
    float CalculateTargetSpeed(Vector3 toTarget, float distance)
    {
        float forwardDot = Vector3.Dot(transform.forward, toTarget.normalized); // Calculate the dot product between the forward vector and the vector to the target to see if we are going forward or not.
        bool shouldReverse = forwardDot < -reverseThreshold; // Check if we should reverse based on the dot product.

        if (shouldReverse) return -maxSpeed * 0.5f; // If we should reverse, return a negative speed.
        if (distance < arrivalDistance) return 0f;
        
        if (distance < slowDownRadius)
        {
            return Mathf.Lerp(0, maxSpeed, distance / slowDownRadius); // If we are within the slow down radius, lerp the speed to 0 based on the distance.
        }
        
        return maxSpeed; // Otherwise, return the maximum speed.
    }

    // Update the drive input based on the target speed and the distance to the target.
    void UpdateDriveInput(float targetSpeed, float distanceToTarget)
    {
        float currentForwardSpeed = Vector3.Dot(rb.velocity, transform.forward); // Calculate the current forward speed.
        float speedError = targetSpeed - currentForwardSpeed; // Calculate how far off the current speed is from the target speed.
        currentDriveInput = Mathf.Clamp(speedError * accelerationSharpness, -1f, 1f); // Clamp the drive input based on the speed error and the acceleration sharpness.

        if (distanceToTarget < arrivalDistance)
        {
            currentDriveInput = -Mathf.Clamp(currentForwardSpeed * 5f, -1f, 1f); // If we are within the arrival distance, reverse the drive input based on the current forward speed.
        }
    }

    // Apply braking based on the distance to the target.
    void ApplyBraking(float distanceToTarget)
    {
        // Debug.Log("Distance to target: " + distanceToTarget);
        if (distanceToTarget < slowDownRadius) // If we are within the slow down radius, start breaking.
        {
            // Debug.Log("Applying braking");
            Vector3 brakeDirection = -rb.velocity.normalized;
            float brakePower = Mathf.Lerp(1f, 0f, distanceToTarget / slowDownRadius);
            
            rb.AddForce(brakeDirection * brakeStrength * brakePower, ForceMode.Acceleration); 
            // TODO: only do this when there are actual wheels
        }
    }

    // Update the wheels based on the current drive and steer input.
    void UpdateWheels()
    {
        foreach (Wheel wheel in wheels)
        {
            if (wheel == null) continue; // Skip the wheel if it is null (aka destroyed in this context).
            Hull hull = wheel.GetComponentInParent<Hull>();
            if (hull != null && hull.canPickup) // Skip the wheel if it is attached to a block that can be picked up, aka detached.
                continue;
            if (wheel.isDriveWheel)
            {
                wheel.driveInput = currentDriveInput; // Set the drive input of the wheel to the current drive input.
                float effectiveForce = wheel.accelForce * currentDriveInput; //
                if (rb != null && wheel != null) {
                    rb.AddForceAtPosition( // Add a force at the position of the wheel to help movement, dirty fix for now.
                        wheel.transform.forward * effectiveForce,
                        wheel.transform.position,
                        ForceMode.Acceleration
                    );
                }
            }

            if (wheel.isTurnWheel)
            {
                float steerAngle = currentSteerInput * wheel.maxSteeringAngle;
                wheel.currentSteerAngle = Mathf.Lerp( // Lerp the current steer angle to the target steer angle based on the steering return speed.
                    wheel.currentSteerAngle, 
                    steerAngle, 
                    Time.fixedDeltaTime * wheel.steeringReturnSpeed
                );
            }
        }
    }

    void OnDrawGizmos()
    {
        if (targetPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPosition.position);
            Gizmos.DrawWireSphere(targetPosition.position, arrivalDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition.position, slowDownRadius);
        }
    }
}