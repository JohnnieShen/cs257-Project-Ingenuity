using UnityEngine;

public abstract class FollowTarget : MonoBehaviour {
	/* 
	* Author: Johnny
	* Summary: This script is responsible for following a target object in the game. It can be attached to any GameObject that needs to follow a target.
	* The target can be set manually or automatically found using the "Player" tag. The script uses FixedUpdate to follow the target smoothly.
	* The Follow method is abstract and should be implemented in derived classes to define the specific following behavior.
	*/
	[SerializeField] public Transform target;
	[SerializeField] private bool autoTargetPlayer = true;

	virtual protected void Start()
	{
		if (autoTargetPlayer)
		{
			FindTargetPlayer();
			}
		}

	void FixedUpdate () 
	{
	  if (autoTargetPlayer && (target == null || !target.gameObject.activeSelf))
		{
			FindTargetPlayer();
				}
		if (target != null && (target.GetComponent<Rigidbody>() != null && !target.GetComponent<Rigidbody>().isKinematic)) 
		{
			Follow(Time.deltaTime);
				}
	}

	protected abstract void Follow(float deltaTime);
	

	public void FindTargetPlayer()
	{
		if (target == null)
		{
			GameObject targetObj = GameObject.FindGameObjectWithTag("Player");
				if(targetObj)
			{
				SetTarget(targetObj.transform);
			}
				}
		}
	public virtual void SetTarget(Transform newTransform)
	{
		target = newTransform;
	}
	public Transform Target{get {return this.target;}}
}
