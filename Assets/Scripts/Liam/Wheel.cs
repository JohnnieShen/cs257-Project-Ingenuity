using Unity.VisualScripting;
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
    private bool isLeftSide = false;
    private Quaternion originalNeutral;
    private Transform probeCoreTransform;

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
        if (transform.parent != null)
        {
            Transform grandParentObject = transform.parent.parent;
            if (grandParentObject != null)
            {
                foreach (Transform sibling in grandParentObject)
                {
                    if (sibling != transform.parent && sibling.CompareTag("Core"))
                    {
                        probeCoreTransform = sibling;
                        break;
                    }
                }
            }
        }
    }
    void OnEnable()
    {
        neutralRotation = transform.localRotation;
        originalNeutral = neutralRotation;
        isLeftSide = (transform.localPosition.x < 0f);
    }

    public void Initialize(bool enabled, float driveInput, float currentSteerAngle)
    {
        this.enabled = enabled;
        this.driveInput = driveInput;
        this.currentSteerAngle = currentSteerAngle;
    }

    void Update()
    {
        Hull hull = GetComponentInParent<Hull>();
        if (hull != null && hull.canPickup)
            return;
        if (isAI) return;
        if (InputManager.instance == null) return;

        Vector2 moveValue = Vector2.zero;
        if (InputManager.instance.GetDriveMoveAction() != null)
        {
            moveValue = InputManager.instance.GetDriveMoveAction().ReadValue<Vector2>();
        }
        Debug.Log(moveValue);
        float steerInput = moveValue.x;
        float driveInput = moveValue.y;

        if (isTurnWheel)
        {
            if (Mathf.Abs(steerInput) > 0.1f)
            {
                float direction = (steerInput > 0f) ? 1f : -1f;
                float sign = invertSteering ? -1f : 1f;
                sign = isLeftSide ? -sign : sign;
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
            Vector3 accelDir = probeCoreTransform.forward;
            
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
                driveInput = isAI ? this.driveInput : Input.GetAxis("Vertical");
            }

            if (isAI && isTurnWheel)
            {
                ApplySteering(currentSteerAngle);
            }

            float driveForce = accelForce * accelForceMultiplier * driveInput;
            rigidBody.AddForce(springForce * springDir + steeringForce * steeringDir + accelDir * driveForce);

            tire.transform.position = hit.point + springDir * 0.5f;
        }
        else
        {
            Vector3 springDir = transform.up;
            Vector3 maxExtendedPos = transform.position - springDir * maxDistance;
            tire.transform.position = maxExtendedPos + springDir * 0.5f;
        }

        float forwardSpeed;
        if (isDriveWheel)
        {
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
