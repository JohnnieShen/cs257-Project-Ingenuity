using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class Pivot : FollowTarget {

	/*
	* Author: Johnny
	* Summary: This script is responsible for managing the camera pivot in the game. It allows the camera to follow a target object smoothly.
	* The script is designed to be used in the Unity Editor and can be attached to a GameObject with a Camera component.
	* It uses the FollowTarget class to handle the target following logic. The script also includes functionality to adjust the camera's local position based on the target's position.
	* The Follow method is abstract and should be implemented in derived classes to define the specific following behavior.
	*/

	protected Transform cam;
	protected Transform pivot;
	protected Vector3 lastTargetPosition;

	protected virtual void Awake()
	{
		cam = GetComponentInChildren<Camera>().transform;
		pivot = cam.parent;
		}
	
	// Use this for initialization
 protected override	void Start () {
	
		base.Start();
	}
	
	// Update is called once per frame
	virtual protected void Update () {
	   
		if (!Application.isPlaying)
		{  
			if(target != null)
			{
				Follow(999);
				lastTargetPosition = target.position;
			}

			if(Mathf.Abs(cam.localPosition.x) > .5f || Mathf.Abs(cam.localPosition.y) > .5f)
			{
				cam.localPosition = Vector3.Scale(cam.localPosition, Vector3.forward);
			}

			cam.localPosition = Vector3.Scale(cam.localPosition, Vector3.forward);
		}
	}
	protected override void Follow(float deltaTime)
	{

	}
}
