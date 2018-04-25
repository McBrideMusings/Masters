//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Pierce R McBride
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	public delegate void PhysGrabEventHandler();
	public enum GrabJoint { FixedJoint, SpringJoint };
	/// <summary>
  /// Physically tracks limb but uses a max-distance value to prevent over-stretching the limb
	/// Distance measured from the parent joint object and is human-tweakable.
  /// </summary>
	public class PhysGrabLimb : PhysBase 
	{
		#region Config
		[Space(10f)]
		public Transform target;
		public Transform interactable;
		public InteractableObject interactableScript;
		public AvatarGrabbables grabbablesScript;
		public Rigidbody handBody;

		[Header("Config")]
		[NonNegative] public float maxDistance = 0.02f;
		public GrabJoint grabJoint = GrabJoint.FixedJoint;
		[NonNegative] public float jointForce = 10f;
		[NonNegative] public float breakForce = 200f;
		#endregion

		#region State
		[Space(10f)]
		[SerializeField] [ReadOnly] private bool isGrabbed = false; // Is the target object for this limb grabbed?
		[SerializeField] [ReadOnly] private bool isGrabbing = false; // Is the limb grabbing an object?
		[SerializeField] [ReadOnly] private Transform bodyParent;
		[SerializeField] [ReadOnly] private Joint grabbedObjectJoint;
		private EventForwarder handEvents;
		#endregion

		#region Events
		public event PhysGrabEventHandler Grabbing;
		public event PhysGrabEventHandler NotGrabbing;
		#endregion

		#region Initialization
    // Get References and Set Initial Values
		protected override void Awake() 
		{
			base.Awake();
			interactableScript.InteractableGrabbed += OnInteractableGrabbed;
			interactableScript.InteractableUngrabbed += OnInteractableUnGrabbed;
			interactableScript.InteractableUsed += OnInteractableUsed;
			interactableScript.InteractableUnused += OnInteractableUnUsed;

			bodyParent = body.transform.parent;
			interactable.position = target.position;

			handEvents = handBody.gameObject.AddComponent<EventForwarder>();
			handEvents.OnJointBreakEvent += JointBreak;
		}
		#endregion

		#region Core
		protected override void FixedUpdate() 
		{
			base.FixedUpdate();
			if (isGrabbed) 
			{
				// This part is the most important part. If max distance is exceeded..
				if (Vector3.Distance(interactable.position, bodyParent.position) > maxDistance)
				{
					// I use a different position for the target that's maxdistance from the parent but between the target and body
					target.position = bodyParent.position + ((interactable.position - bodyParent.position).normalized * maxDistance);
					target.rotation = interactable.rotation;
				}
				else
				{
					target.position = interactable.position;
					target.rotation = interactable.rotation;
				}
				
				Track(target,weight);
				Rotate(target);
			}
			else
			{
				target.position = body.position;
				target.rotation = body.rotation;
				interactable.position = target.position;
				interactable.rotation = target.rotation;
			}
		}
		protected void Grab(Rigidbody toGrab)
		{
			switch (grabJoint)
			{
				case GrabJoint.FixedJoint:
					grabbedObjectJoint = handBody.gameObject.AddComponent<FixedJoint>();
					break;
				case GrabJoint.SpringJoint:
					SpringJoint spring = handBody.gameObject.AddComponent<SpringJoint>();
					spring.spring = jointForce;
					grabbedObjectJoint = spring;
					break;
			}
      grabbedObjectJoint.breakForce = breakForce;
  		grabbedObjectJoint.anchor = handBody.transform.InverseTransformPoint(toGrab.transform.position);
      grabbedObjectJoint.connectedBody = toGrab;
			isGrabbing = true;
			if (Grabbing != null) Grabbing();
		}
		protected void UnGrab()
		{
			grabbedObjectJoint.connectedBody = null;
			Destroy(grabbedObjectJoint);
			ResetGrab();
		}
		protected void ResetGrab()
		{
			grabbedObjectJoint = null;
			isGrabbing = false;
			if (NotGrabbing != null) NotGrabbing();
		}
		#endregion

		#region Listeners
		protected void JointBreak(float breakForce)
		{
			Debug.Log("Joint Broke!, force: " + breakForce);
			ResetGrab();
		}
		public void OnInteractableGrabbed(object sender, InteractableEventArgs e) 
		{
			isGrabbed = true;
		}
		public void OnInteractableUnGrabbed(object sender, InteractableEventArgs e) 
		{
			isGrabbed = false;
			interactable.position = target.position;
		}
		public void OnInteractableUsed(object sender, InteractableEventArgs e) 
		{
			Debug.Log("Used");
			if (isGrabbing == false)
			{
				if (grabbablesScript.grabbables.Count > 0)
				{
					Grab(grabbablesScript.grabbables[0]);
				}
			}
			else
			{
				UnGrab();
			}
		}
		public void OnInteractableUnUsed(object sender, InteractableEventArgs e) 
		{

		}
		#endregion
	}
}
