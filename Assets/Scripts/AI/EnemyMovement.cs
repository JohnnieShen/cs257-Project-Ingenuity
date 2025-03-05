using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float maxSpeed = 20f;
    public float arrivalDistance = 2f;
    public float reverseThreshold = 0.5f;
    public float steeringSharpness = 2f;
    public float accelerationSharpness = 2f;
    public float brakeStrength = 100f;
    public float slowDownRadius = 10f;

    public Transform targetPosition;
    public Rigidbody rb;
    public Wheel[] wheels;

    public float currentDriveInput;
    public float currentSteerInput;
    void Start()
    {
        foreach (Wheel wheel in wheels)
        {
            wheel.isAI = true;
        }
    }
    void FixedUpdate()
    {
        if (targetPosition == null) return;

        Vector3 toTarget = targetPosition.position - transform.position;
        float distanceToTarget = toTarget.magnitude;
        
        Vector3 localTargetDir = transform.InverseTransformDirection(toTarget.normalized);
        currentSteerInput = Mathf.Clamp(localTargetDir.x * steeringSharpness, -1f, 1f);

        float targetSpeed = CalculateTargetSpeed(toTarget, distanceToTarget);
        UpdateDriveInput(targetSpeed, distanceToTarget);
        UpdateWheels();
        ApplyBraking(distanceToTarget);
    }

    float CalculateTargetSpeed(Vector3 toTarget, float distance)
    {
        float forwardDot = Vector3.Dot(transform.forward, toTarget.normalized);
        bool shouldReverse = forwardDot < -reverseThreshold;

        if (shouldReverse) return -maxSpeed * 0.5f;
        if (distance < arrivalDistance) return 0f;
        
        if (distance < slowDownRadius)
        {
            return Mathf.Lerp(0, maxSpeed, distance / slowDownRadius);
        }
        
        return maxSpeed;
    }

    void UpdateDriveInput(float targetSpeed, float distanceToTarget)
    {
        float currentForwardSpeed = Vector3.Dot(rb.velocity, transform.forward);
        float speedError = targetSpeed - currentForwardSpeed;
        currentDriveInput = Mathf.Clamp(speedError * accelerationSharpness, -1f, 1f);

        if (distanceToTarget < arrivalDistance)
        {
            currentDriveInput = -Mathf.Clamp(currentForwardSpeed * 5f, -1f, 1f);
        }
    }

    void ApplyBraking(float distanceToTarget)
    {
        // Debug.Log("Distance to target: " + distanceToTarget);
        if (distanceToTarget < slowDownRadius)
        {
            // Debug.Log("Applying braking");
            Vector3 brakeDirection = -rb.velocity.normalized;
            float brakePower = Mathf.Lerp(1f, 0f, distanceToTarget / slowDownRadius);
            
            rb.AddForce(brakeDirection * brakeStrength * brakePower, ForceMode.Acceleration);
        }
    }

    void UpdateWheels()
    {
        foreach (Wheel wheel in wheels)
        {
            if (wheel.isDriveWheel)
            {
                wheel.driveInput = currentDriveInput;
                float effectiveForce = wheel.accelForce * currentDriveInput;
                if (rb != null) {
                    rb.AddForceAtPosition(
                        wheel.transform.forward * effectiveForce,
                        wheel.transform.position,
                        ForceMode.Acceleration
                    );
                }
            }

            if (wheel.isTurnWheel)
            {
                float steerAngle = currentSteerInput * wheel.maxSteeringAngle;
                wheel.currentSteerAngle = Mathf.Lerp(
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