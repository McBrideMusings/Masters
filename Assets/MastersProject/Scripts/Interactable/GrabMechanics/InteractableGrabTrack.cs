//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Mostly VRTK with logic from NewtonVR
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	/// <summary>
  /// Grab Mechanic Implemented from NewtonVR that does not use joints
  /// </summary>
	public class InteractableGrabTrack : InteractableGrabBase 
	{
		#region Public Fields
		#endregion


		#region Private Fields
		[ReadOnly] [SerializeField] private Transform pickupTransform;
		#endregion


		#region Bookkeeping 
		private const float MaxVelocityChange = 10f;
		private const float MaxAngularVelocityChange = 20f;
		private const float VelocityMagic = 6000f;
		private const float AngularVelocityMagic = 50f;

		protected Vector3 ExternalVelocity;
		protected Vector3 ExternalAngularVelocity;
		protected Vector3?[] VelocityHistory;
		protected Vector3?[] AngularVelocityHistory;
		protected int CurrentVelocityHistoryStep = 0;
		protected float StartingDrag = -1;
		protected float StartingAngularDrag = -1;
		#endregion

		#region Initilization 
    /// <summary>
    /// The StartGrab method sets up the grab attach mechanic as soon as an object is grabbed.
    /// </summary>
    /// <param name="grabbingController">The object that is doing the grabbing.</param>
    /// <param name="grabbingAttachPoint">The point on the grabbing object that the grabbed object should be attached to after grab occurs.</param>
    /// <returns>Is true if the grab is successful, false if the grab is unsuccessful.</returns>
    public override bool StartGrab(GameObject grabbingController, Rigidbody grabbingAttachPoint) {
			base.StartGrab(grabbingController, grabbingAttachPoint);
			pickupTransform = new GameObject(string.Format("[{0}] PickupTransform", this.gameObject.name)).transform;
			pickupTransform.parent = grabbingController.transform;
			pickupTransform.position = transform.position;
			pickupTransform.rotation = transform.rotation;
      return true;
    }
		#endregion

		#region Core 
    /// <summary>
    /// The ProcessFixedUpdate method is run in every FixedUpdate method on the interactable object.
    /// </summary>
    public override void ProcessFixedUpdate() {
			UpdateVelocities();
			AddExternalVelocities();
		}
		#endregion

		#region Helper 
		protected void UpdateVelocities() {
			Vector3 targetItemPosition;
			Quaternion targetItemRotation;

			Vector3 targetHandPosition;
			Quaternion targetHandRotation;

			GetTargetValues(out targetHandPosition, out targetHandRotation, out targetItemPosition, out targetItemRotation);

			float velocityMagic = VelocityMagic / (Time.deltaTime / GameManager.ExpectedDeltaTime);
			float angularVelocityMagic = AngularVelocityMagic / (Time.deltaTime / GameManager.ExpectedDeltaTime);

			Vector3 positionDelta;
			Quaternion rotationDelta;

			float angle;
			Vector3 axis;

			positionDelta = (targetHandPosition - targetItemPosition);
			rotationDelta = targetHandRotation * Quaternion.Inverse(targetItemRotation);

			Vector3 velocityTarget = (positionDelta * velocityMagic) * Time.deltaTime;
			if (float.IsNaN(velocityTarget.x) == false) {
				rigidbody.velocity = Vector3.MoveTowards(rigidbody.velocity, velocityTarget, MaxVelocityChange);
			}

			rotationDelta.ToAngleAxis(out angle, out axis);
			if (angle > 180) { angle -= 360; }
			if (angle != 0)	{
				Vector3 angularTarget = angle * axis;
				if (float.IsNaN(angularTarget.x) == false) {
					angularTarget = (angularTarget * angularVelocityMagic) * Time.deltaTime;
					rigidbody.angularVelocity = Vector3.MoveTowards(rigidbody.angularVelocity, angularTarget, MaxAngularVelocityChange);
				}
			}


			if (VelocityHistory != null) {
				CurrentVelocityHistoryStep++;
				if (CurrentVelocityHistoryStep >= VelocityHistory.Length) {
					CurrentVelocityHistoryStep = 0;
				}

				VelocityHistory[CurrentVelocityHistoryStep] = rigidbody.velocity;
				AngularVelocityHistory[CurrentVelocityHistoryStep] = rigidbody.angularVelocity;
			}
		}
		protected void GetTargetValues(out Vector3 targetHandPosition, out Quaternion targetHandRotation, out Vector3 targetItemPosition, out Quaternion targetItemRotation) {
			targetItemPosition = transform.position;
			targetItemRotation = transform.rotation;

			targetHandPosition = pickupTransform.transform.position;
			targetHandRotation = pickupTransform.transform.rotation;
		}
		protected void AddExternalVelocities() {
			if (ExternalVelocity != Vector3.zero) {
				rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, ExternalVelocity, 0.5f);
				ExternalVelocity = Vector3.zero;
			}

			if (ExternalAngularVelocity != Vector3.zero) {
				rigidbody.angularVelocity = Vector3.Lerp(rigidbody.angularVelocity, ExternalAngularVelocity, 0.5f);
				ExternalAngularVelocity = Vector3.zero;
			}
		}
		#endregion
		/* 
		#region Initilization 
    /// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		protected override void Awake() {
			base.Awake();
			rigidbody.maxAngularVelocity = 100f;
		}
		#endregion

		#region Core 
		/// <summary>
    /// The ProcessFixedUpdate method is run in every FixedUpdate method on the interactable object.
    /// </summary>
    public void FixedUpdate() { 
			Debug.Log("process fixed update");
			if (IsGrabbed()) {
				UpdateVelocities();
			}
			AddExternalVelocities();
		}
		#endregion

    #region Grab Start
    /// <summary>
    /// The StartGrab method sets up the grab attach mechanic as soon as an object is grabbed.
    /// </summary>
    /// <param name="grabbingController">The object that is doing the grabbing.</param>
    /// <param name="grabbingAttachPoint">The point on the grabbing object that the grabbed object should be attached to after grab occurs.</param>
    /// <returns>Is true if the grab is successful, false if the grab is unsuccessful.</returns>
    public override bool StartGrab(GameObject grabbingController, Rigidbody grabbingAttachPoint) {
      base.StartGrab(grabbingController, grabbingAttachPoint);

			StartingDrag = rigidbody.drag;
			StartingAngularDrag = rigidbody.angularDrag;
			rigidbody.drag = 0;
			rigidbody.angularDrag = 0.05f;

			//DisablePhysicalMaterials();

			Transform pickupTransform = new GameObject(string.Format("[{0}] PickupTransform", this.gameObject.name)).transform;
			pickupTransform.parent = grabbingController.transform;
			pickupTransform.position = transform.position;
			pickupTransform.rotation = transform.rotation;
			//PickupTransforms.Add(hand, pickupTransform);

			Debug.Log("grabbing");
      return true;
    }
    #endregion

    #region Grab Stop
    /// <summary>
    /// The StopGrab method ends the grab of the current object and cleans up the state.
    /// </summary>
    /// <param name="applyGrabbingObjectVelocity">If true will apply the current velocity of the grabbing object to the grabbed object on release.</param>
    public override void StopGrab(bool applyGrabbingObjectVelocity) {
			Debug.Log("stop grabbing");
      grabbingObject = null;
      //interactableObject = null;
      attachPoint = null;
    }
    protected override void ForceReleaseGrab() {
			Debug.Log("force releasing");
      if (interactableObject) {
        GameObject grabbingObject = interactableObject.GetGrabbingObject();
        if (grabbingObject != null) {
          ControllerGrab grabbingObjectScript = grabbingObject.GetComponent<ControllerGrab>();
          if (grabbingObjectScript != null) {
            grabbingObjectScript.ForceRelease();
          }
        }
      }
    }
    protected override void ReleaseObject(bool applyGrabbingObjectVelocity) {
			Debug.Log("releasing");
			Rigidbody releasedObjectRigidBody = ReleaseFromController(applyGrabbingObjectVelocity);
			if (releasedObjectRigidBody != null && applyGrabbingObjectVelocity) {
				ThrowReleasedObject(releasedObjectRigidBody);
			}
    }
    #endregion

		#region Helper 
		protected void UpdateVelocities() {
			Vector3 targetItemPosition;
			Quaternion targetItemRotation;

			Vector3 targetHandPosition;
			Quaternion targetHandRotation;

			GetTargetValues(out targetHandPosition, out targetHandRotation, out targetItemPosition, out targetItemRotation);

			float velocityMagic = VelocityMagic / (Time.deltaTime / GameManager.ExpectedDeltaTime);
			float angularVelocityMagic = AngularVelocityMagic / (Time.deltaTime / GameManager.ExpectedDeltaTime);

			Vector3 positionDelta;
			Quaternion rotationDelta;

			float angle;
			Vector3 axis;

			positionDelta = (targetHandPosition - targetItemPosition);
			rotationDelta = targetHandRotation * Quaternion.Inverse(targetItemRotation);

			Vector3 velocityTarget = (positionDelta * velocityMagic) * Time.deltaTime;
			if (float.IsNaN(velocityTarget.x) == false) {
				rigidbody.velocity = Vector3.MoveTowards(rigidbody.velocity, velocityTarget, MaxVelocityChange);
			}

			rotationDelta.ToAngleAxis(out angle, out axis);
			if (angle > 180) { angle -= 360; }
			if (angle != 0)	{
				Vector3 angularTarget = angle * axis;
				if (float.IsNaN(angularTarget.x) == false) {
					angularTarget = (angularTarget * angularVelocityMagic) * Time.deltaTime;
					rigidbody.angularVelocity = Vector3.MoveTowards(rigidbody.angularVelocity, angularTarget, MaxAngularVelocityChange);
				}
			}


			if (VelocityHistory != null) {
				CurrentVelocityHistoryStep++;
				if (CurrentVelocityHistoryStep >= VelocityHistory.Length) {
					CurrentVelocityHistoryStep = 0;
				}

				VelocityHistory[CurrentVelocityHistoryStep] = rigidbody.velocity;
				AngularVelocityHistory[CurrentVelocityHistoryStep] = rigidbody.angularVelocity;
			}
		}
		protected void GetTargetValues(out Vector3 targetHandPosition, out Quaternion targetHandRotation, out Vector3 targetItemPosition, out Quaternion targetItemRotation) {
			targetItemPosition = attachPoint.position;
			targetItemRotation = attachPoint.rotation;

			targetHandPosition = grabbingObject.transform.position;
			targetHandRotation = grabbingObject.transform.rotation;
		}
		protected void AddExternalVelocities() {
			if (ExternalVelocity != Vector3.zero) {
				rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, ExternalVelocity, 0.5f);
				ExternalVelocity = Vector3.zero;
			}

			if (ExternalAngularVelocity != Vector3.zero) {
				rigidbody.angularVelocity = Vector3.Lerp(rigidbody.angularVelocity, ExternalAngularVelocity, 0.5f);
				ExternalAngularVelocity = Vector3.zero;
			}
		}

		public void AddExternalVelocity(Vector3 velocity) {
			if (ExternalVelocity == Vector3.zero) {
				ExternalVelocity = velocity;
			}
			else {
				ExternalVelocity = Vector3.Lerp(ExternalVelocity, velocity, 0.5f);
			}
		}

		public void AddExternalAngularVelocity(Vector3 angularVelocity) {
			if (ExternalAngularVelocity == Vector3.zero) {
				ExternalAngularVelocity = angularVelocity;
			}
			else {
				ExternalAngularVelocity = Vector3.Lerp(ExternalAngularVelocity, angularVelocity, 0.5f);
			}
		}
		#endregion
		*/
	}
}
