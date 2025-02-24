using UnityEngine;
using System;
//using UnityEditor;

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
	}

	void OnDisable()
	{
        // Cursor.lockState = CursorLockMode.None;
		// Cursor.visible = true;
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
		int combinedMask = terrainLayerMask | aimMask;

		if (Physics.Raycast(forwardRay, out RaycastHit forwardHit, 1000f, combinedMask, QueryTriggerInteraction.Collide))
		{
			aimTarget.position = Vector3.Lerp(aimTarget.position, forwardHit.point, Time.deltaTime * 10f);
		}
		else
		{
			Vector3 behindPos = forwardRay.origin - forwardRay.direction * 100f;
			Ray backwardRay = new Ray(behindPos, forwardRay.direction);

			Debug.DrawRay(backwardRay.origin, backwardRay.direction * 200f, Color.blue, 0.1f);

			if (Physics.Raycast(backwardRay, out RaycastHit backwardHit, 200f, aimMask, QueryTriggerInteraction.Collide))
			{
				aimTarget.position = Vector3.Lerp(aimTarget.position, -backwardHit.point, Time.deltaTime * 10f);
			}
		}
	}


		
    
	void HandleRotationMovement()
	{
		float x = Input.GetAxis("Mouse X");
		float y = Input.GetAxis("Mouse Y");

		if (turnsmoothing > 0)
		{
			smoothX = Mathf.SmoothDamp (smoothX, x, ref smoothXvelocity, turnsmoothing);
			smoothY = Mathf.SmoothDamp (smoothY, y, ref smoothYvelocity, turnsmoothing);
				} 
		else
		{
			smoothX = x;
			smoothY = y;
				}
		lookAngle += smoothX * turnSpeed;

		transform.rotation = Quaternion.Euler(0f, lookAngle, 0);

		tiltAngle -= smoothY * turnSpeed;
		tiltAngle = Mathf.Clamp (tiltAngle, -tiltMin, tiltMax);

		pivot.localRotation = Quaternion.Euler(tiltAngle,0,0);
	}
	private void HandleZoom()
	{
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		if (Mathf.Abs(scroll) > 0.001f)
		{
			targetZoom -= scroll * zoomSpeed;
			targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
		}

		currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSmoothFactor);
		
		cam.localPosition = new Vector3(0, 0, -currentZoom);
	}

}
