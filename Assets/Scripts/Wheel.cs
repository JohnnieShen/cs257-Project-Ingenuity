using UnityEngine;

public class Wheel : MonoBehaviour
{
    [Header("AI Overrides")]
    public float driveInput = 0f;
    public float accelForceMultiplier = 1f;
    public Rigidbody rigidBody;
    public float suspensionRestDist;
    public float springStrength;
    public float springDamper;
    public float maxDistance;
    public float tireGripFactor;
    public float tireMass;
    public float accelForce;
    public float rotationSpeed;
    public float steeringReturnSpeed = 2f;
    public float maxSteeringAngle = 45f;
    public GameObject tire;

    public bool isDriveWheel = false;
    public bool isTurnWheel = false;
    public bool isAI = false;

    private Quaternion neutralRotation;
    public float currentSteerAngle = 0f;
    public bool invertSteering = false;
    public float tireRadius = 0.3f;

    void Start()
    {
        if (isTurnWheel)
        {
            neutralRotation = transform.localRotation;
            if (transform.parent != null)
            {
                float parentLocalZ = transform.parent.localPosition.z;
                if (parentLocalZ < 0.4f)
                {
                    invertSteering = true;
                }
            }
        }
    }

    void Update()
    {
        if (isAI) return;
        if (InputManager.instance == null) return;

        Vector2 moveValue = Vector2.zero;
        if (InputManager.instance.GetDriveMoveAction() != null)
        {
            moveValue = InputManager.instance.GetDriveMoveAction().ReadValue<Vector2>();
        }

        float steerInput = moveValue.x;
        float driveInput = moveValue.y;

        if (isTurnWheel)
        {
            if (Mathf.Abs(steerInput) > 0.1f)
            {
                float direction = (steerInput > 0f) ? 1f : -1f;
                float sign = invertSteering ? -1f : 1f;
                currentSteerAngle += sign * direction * rotationSpeed * Time.deltaTime;
            }
            else
            {
                currentSteerAngle = Mathf.Lerp(currentSteerAngle, 0f, steeringReturnSpeed * Time.deltaTime);
            }

            currentSteerAngle = Mathf.Clamp(currentSteerAngle, -maxSteeringAngle, maxSteeringAngle);
            transform.localRotation = neutralRotation * Quaternion.Euler(0f, currentSteerAngle, 0f);
        }
    }

    void FixedUpdate()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            // Debug.Log("Hit detected.");
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

            float driveInput = 0f;
            if (isDriveWheel)
            {
                if (isAI)
                {
                    driveInput = this.driveInput;
                }
                else
                {
                    // Original player input
                    
                    driveInput = Input.GetAxis("Vertical");
                    
                }
            }
            if (isAI && isTurnWheel)
            {
                ApplySteering(currentSteerAngle);
            }
            float driveForce = accelForce * accelForceMultiplier * driveInput;

            rigidBody.AddForce(springForce * springDir + steeringForce * steeringDir + accelDir * driveForce);

            tire.transform.position = hit.point + springDir * 0.5f;
            float forwardSpeed = Vector3.Dot(rigidBody.velocity, transform.forward);
            float distanceTraveled = forwardSpeed * Time.fixedDeltaTime;
            float angleDelta = (distanceTraveled / tireRadius) * Mathf.Rad2Deg;
            tire.transform.Rotate(Vector3.right, angleDelta, Space.Self);
        }
        else
        {
            // Debug.Log("No hit detected.");
            tire.transform.localPosition = -transform.up;
        }
    }
    private void ApplySteering(float targetAngle)
    {
        currentSteerAngle = Mathf.Lerp(
            currentSteerAngle, 
            targetAngle, 
            Time.fixedDeltaTime * steeringReturnSpeed
        );
        
        currentSteerAngle = Mathf.Clamp(currentSteerAngle, -maxSteeringAngle, maxSteeringAngle);
        transform.localRotation = neutralRotation * Quaternion.Euler(0f, currentSteerAngle, 0f);
    }
}
