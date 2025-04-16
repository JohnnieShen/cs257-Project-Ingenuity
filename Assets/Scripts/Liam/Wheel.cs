using Unity.VisualScripting;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public float driveInput = 0f;
    public float suspensionRestDist = 1f;
    public float springStrength = 500f;
    public float springDamper = 10f;
    public float maxDistance = 1.5f;
    public float tireGripFactor = 0.3f;
    public float accelForce = 10f;
    public float rotationSpeed = 10f;
    public float steeringReturnSpeed = 10f;
    public float maxSteeringAngle = 30f;
    public GameObject tire;
    public bool isAI = false;
    private Quaternion neutralRotation;
    public float currentSteerAngle = 0f;
    public bool invertSteering = false;
    public float tireRadius = 0.3f;
    public bool isLeftSide = false;
    public Rigidbody rigidBody;
    private Transform commandModuleTransform;

    void Start()
    {
        // Get reference to command module
        if (transform.parent != null)
        {
            Transform grandParentObject = transform.parent.parent;
            if (grandParentObject != null)
            {
                foreach (Transform sibling in grandParentObject)
                {
                    if ((sibling != transform.parent && sibling.CompareTag("Core"))||(sibling != transform.parent && sibling.name == "CommandModule"))
                    {
                        commandModuleTransform = sibling;
                        break;
                    }
                }
                if ((grandParentObject != transform.parent && grandParentObject.CompareTag("Core"))||(grandParentObject != transform.parent && grandParentObject.name == "CommandModule"))
                {
                    commandModuleTransform = grandParentObject;
                }
            }
        }

        // Remember neutral rotation
        neutralRotation = transform.localRotation;

        // Invert steering if wheel is behind the command module
        if (commandModuleTransform != null)
        {
            Vector3 localPosRelativeToCore = commandModuleTransform.InverseTransformPoint(transform.position);
            if (localPosRelativeToCore.z < 0.4f)
            {
                invertSteering = true;
            }
        }
        isLeftSide = (commandModuleTransform.InverseTransformPoint(transform.position).x < 0f);
    }

    void Update()
    {
        Hull hull = GetComponentInParent<Hull>();

        // Only the wheels currently attached to the player should drive
        if (hull != null && hull.canPickup)
            return;
        if (isAI) return;
        if (InputManager.instance == null) return;

        Vector2 moveValue = Vector2.zero;
        if (InputManager.instance.GetDriveMoveAction() != null)
        {
            moveValue = InputManager.instance.GetDriveMoveAction().ReadValue<Vector2>();
        }
        float steerInput = moveValue.x;
        float driveInput = moveValue.y;

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

    void FixedUpdate()
    {
        Hull hull = GetComponentInParent<Hull>();
        if (hull != null && hull.canPickup)
        {
            driveInput = 0f;
            currentSteerAngle = 0f;
            return;
        }

        Ray ray = new Ray(transform.position, -transform.up);
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
        bool isGrounded = Physics.Raycast(ray, out RaycastHit hit, maxDistance);

        if (isGrounded)
        {
            Vector3 springDir = transform.up;
            Vector3 steeringDir = transform.right;
            Vector3 accelDir = commandModuleTransform.forward;
            
            float springOffset = suspensionRestDist - hit.distance;
            if (rigidBody == null)
            {
                return;
            }
            float springVel = Vector3.Dot(springDir, rigidBody.velocity);
            float steeringVel = Vector3.Dot(steeringDir, rigidBody.velocity);
            
            float desiredVelChange = -steeringVel * tireGripFactor;
            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;
            
            float springForce = springOffset * springStrength - springVel * springDamper;

            float driveInput = 0f;
            driveInput = isAI ? this.driveInput : Input.GetAxis("Vertical");

            if (isAI)
            {
                ApplySteering(currentSteerAngle);
            }

            float driveForce = accelForce * driveInput;
            rigidBody.AddForce(springForce * springDir + desiredAccel * steeringDir + accelDir * driveForce);

            tire.transform.position = hit.point + springDir * 0.5f;
        }
        else
        {
            Vector3 springDir = transform.up;
            Vector3 maxExtendedPos = transform.position - springDir * maxDistance;
            tire.transform.position = maxExtendedPos + springDir * 0.5f;
        }

        float forwardSpeed;
        if (isGrounded)
        {
            forwardSpeed = Vector3.Dot(rigidBody.velocity, transform.forward);
        }
        else
        {
            forwardSpeed = driveInput * accelForce * 0.1f;
        }

        float distanceTraveled = forwardSpeed * Time.fixedDeltaTime;
        float angleDelta = (distanceTraveled / tireRadius) * Mathf.Rad2Deg;
        tire.transform.Rotate(Vector3.right, angleDelta, Space.Self);
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