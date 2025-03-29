using UnityEngine;
using System;
//using UnityEditor;
using UnityEngine.InputSystem;
public class FreeCameraLook : Pivot {
	public static event Action OnFire;

	public static FreeCameraLook instance;
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float turnSpeed = 1.5f;
	[SerializeField] private float turnsmoothing = .1f;
	[SerializeField] private float tiltMax = 75f;
	[SerializeField] private float tiltMin = 45f;
	[SerializeField] private float zoomSpeed = 10f;
	[SerializeField] private float minZoom = 5f;
	[SerializeField] private float maxZoom = 30f;
	[SerializeField] private float zoomSmoothFactor = 5f;
	[SerializeField] private LayerMask terrainLayerMask;
	[SerializeField] private float collisionOffset = 0.5f;

	public Transform aimTarget;
	public Transform commandModule;
    public LayerMask rayCastLayers;
    public LayerMask shieldLayer;

	private float targetZoom = 10f;
	private float currentZoom = 10f;
	[SerializeField] private bool lockCursor = false;

	private float lookAngle;
	private float tiltAngle;

	private const float LookDistance = 100f;

	private float smoothX = 0;
	private float smoothY = 0;
	private float smoothXvelocity = 0;
	private float smoothYvelocity = 0;
	private bool isFiring = false;
	protected override void Awake()
	{
		base.Awake();

        Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		cam = GetComponentInChildren<Camera>().transform;
		pivot = cam.parent;
		targetZoom = currentZoom;
		if (instance == null)
            instance = this;
        else
            Debug.LogError("Duplicated FreeCam", gameObject);
		if (aimTarget == null) {
            GameObject aimTargetGO = new GameObject("AimTarget");
            aimTarget = aimTargetGO.transform;
            Debug.Log("aimTarget was not assigned. A new AimTarget GameObject has been created.");
        }
	}
	private void OnEnable()
    {
        if (InputManager.instance != null && InputManager.instance.GetDriveShootAction() != null)
        {
            InputManager.instance.GetDriveShootAction().performed += OnFireStarted;
            InputManager.instance.GetDriveShootAction().canceled  += OnFireCanceled;
        }
		if (InputManager.instance != null && InputManager.instance.GetDriveScrollAction() != null)
		{
			InputManager.instance.GetDriveScrollAction().performed += OnScrollPerformed;
		}
    }

    private void OnDisable()
    {
        if (InputManager.instance != null && InputManager.instance.GetDriveShootAction() != null)
        {
            InputManager.instance.GetDriveShootAction().performed -= OnFireStarted;
            InputManager.instance.GetDriveShootAction().canceled  -= OnFireCanceled;
        }
		if (InputManager.instance != null && InputManager.instance.GetDriveScrollAction() != null)
		{
			InputManager.instance.GetDriveScrollAction().performed -= OnScrollPerformed;
		}
    }

	void OnFireStarted(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        isFiring = true;
    }
    void OnFireCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        isFiring = false;
    }
	
	// Update is called once per frame
	protected override	void Update ()
	{
		base.Update();

		HandleRotationMovement();
		HandleZoom();
		UpdateTarget();
		// if (lockCursor && Input.GetMouseButtonUp (0))
		// {
        //     Cursor.lockState = CursorLockMode.Confined;
		// }
		if (Input.GetMouseButtonDown(0))
        {
            isFiring = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isFiring = false;
        }

        if (isFiring)
        {
            OnFire?.Invoke();
        }
		Ray forwardRay = cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        Hull hoveredHull = FindHoveredHull(forwardRay);
		if (Input.GetKeyDown(KeyCode.F) && hoveredHull != null)
		{
			PickupBlock(hoveredHull);
		}
	}


	protected override void Follow (float deltaTime)
	{
		transform.position = Vector3.Lerp(transform.position, target.position, deltaTime * moveSpeed);

	}
	private void UpdateTarget() 
	{
		Ray forwardRay = cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		Debug.DrawRay(forwardRay.origin, forwardRay.direction * 1000f, Color.red, 0.1f);

		int aimLayer = LayerMask.NameToLayer("Aim");
		if (aimLayer == -1)
		{
			Debug.LogError("Layer 'Aim' does not exist in Project Settings > Tags and Layers.");
			return;
		}
		int aimMask = 1 << aimLayer;

		RaycastHit hit;
		if (Physics.Raycast(forwardRay, out hit, 1000f, aimMask, QueryTriggerInteraction.Collide))
		{
			aimTarget.position = Vector3.Lerp(aimTarget.position, hit.point, Time.deltaTime * 10f);
		}
		else
		{
			aimTarget.position = forwardRay.origin + forwardRay.direction * 1000f;
		}
	}


		
    
	void HandleRotationMovement()
    {
        Vector2 lookVal = Vector2.zero;
        if (InputManager.instance != null && InputManager.instance.GetDriveLookAction() != null)
        {
            lookVal = InputManager.instance.GetDriveLookAction().ReadValue<Vector2>();
        }

        float x = lookVal.x;
        float y = lookVal.y;

        if (turnsmoothing > 0)
        {
            smoothX = Mathf.SmoothDamp(smoothX, x, ref smoothXvelocity, turnsmoothing);
            smoothY = Mathf.SmoothDamp(smoothY, y, ref smoothYvelocity, turnsmoothing);
        }
        else
        {
            smoothX = x;
            smoothY = y;
        }

        lookAngle += smoothX * turnSpeed;
        transform.rotation = Quaternion.Euler(0f, lookAngle, 0f);

        tiltAngle -= smoothY * turnSpeed;
        tiltAngle = Mathf.Clamp(tiltAngle, -tiltMin, tiltMax);
        pivot.localRotation = Quaternion.Euler(tiltAngle, 0f, 0f);
    }

    private void OnScrollPerformed(InputAction.CallbackContext ctx)
	{
		Vector2 scrollValue = ctx.ReadValue<Vector2>();
		// Debug.Log("ScrollPerformed: " + scrollValue);
		float scroll = scrollValue.y;

		targetZoom -= scroll * zoomSpeed;
		targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
	}
	void HandleZoom()
	{
		currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSmoothFactor);
		cam.localPosition = new Vector3(0f, 0f, -currentZoom);
	}
	private Hull FindHoveredHull(Ray ray)
    {
        LayerMask combinedMask = rayCastLayers & ~shieldLayer;
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, combinedMask))
        {
			// Debug.Log("Hit: " + hitInfo.transform.name);
            if (hitInfo.transform.CompareTag("Block") ||
               (hitInfo.transform.CompareTag("EnemyBlock")) && hitInfo.transform.GetComponent<Hull>().canPickup)
            {
                return hitInfo.transform.GetComponent<Hull>();
            }
        }
        return null;
    }
	private void PickupBlock(Hull hull)
    {
        if (hull == null || hull.sourceBlock == null) {
			// Debug.LogWarning("Hull or sourceBlock is null. Cannot pick up block.");
			return;
		}

        BlockInventoryManager.instance.AddBlock(hull.sourceBlock, 1);
		//Remove blocks from manager here?

        Destroy(hull.gameObject);
    }
}
