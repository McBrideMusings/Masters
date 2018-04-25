//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Mostly VRTK
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRTK;

namespace PlayByPierce.Masters
{
	/// <summary>
  /// Abstract base class for interactable grab mechanics
  /// </summary>
	public abstract class InteractableGrabBase : MonoBehaviour 
	{
		#region Public Fields
    [Header("Base Options", order = 1)]

    [Tooltip("If checked then when the object is thrown, the distance between the object's attach point and the controller's attach point will be used to calculate a faster throwing velocity.")]
    public bool throwVelocityWithAttachDistance = false;
    [Tooltip("An amount to multiply the velocity of the given object when it is thrown. This can also be used in conjunction with the Interact Grab Throw Multiplier to have certain objects be thrown even further than normal (or thrown a shorter distance if a number below 1 is entered).")]
    public float throwMultiplier = 1f;
    [Tooltip("The amount of time to delay collisions affecting the object when it is first grabbed. This is useful if a game object may get stuck inside another object when it is being grabbed.")]
    public float onGrabCollisionDelay = 0f;
		#endregion

		#region Private Fields
    [ReadOnly] [SerializeField] protected GameObject grabbingObject;
    [ReadOnly] [SerializeField] protected Rigidbody grabbingAttach;
		#endregion

		#region Bookkeeping 
    /// <returns>Is true if the grab is successful, false if the grab is unsuccessful.</returns>
    public bool isKinematic { get; private set; }

    /// <returns>Is true if the grab is successful, false if the grab is unsuccessful.</returns>
    public bool isSwappable { get; private set; }

    /// <returns>Is true if the grab is successful, false if the grab is unsuccessful.</returns>
    public bool isActionable { get; private set; }
		protected new Transform transform;
		protected new Rigidbody rigidbody;
    protected InteractableObject interactable;
		#endregion


    #region Public Methods
    /// <returns>Is true if the grab is successful, false if the grab is unsuccessful.</returns>
    public bool IsGrabbed() {
      if (grabbingAttach) {
        return true;
      }
      return false;
    }
    /// <summary>
    /// The ValidGrab method determines if the grab attempt is valid.
    /// </summary>
    /// <param name="checkAttachPoint"></param>
    /// <returns>Always returns true for the base check.</returns>
    public virtual bool ValidGrab(Rigidbody checkAttachPoint) {
      return true;
    }
		#endregion


    #region Initilization
    protected virtual void Awake() {
      transform = gameObject.GetComponent<Transform>();
			rigidbody = gameObject.GetComponent<Rigidbody>();
      interactable = gameObject.GetComponent<InteractableObject>();
    }
    #endregion

    #region Core
    /// <summary>
    /// The ProcessUpdate method is run in every Update method on the interactable object.
    /// </summary>
    public virtual void ProcessUpdate() {}

    /// <summary>
    /// The ProcessFixedUpdate method is run in every FixedUpdate method on the interactable object.
    /// </summary>
    public virtual void ProcessFixedUpdate() {}
    #endregion


    #region Grab Start
    /// <summary>
    /// The StartGrab method sets up the grab attach mechanic as soon as an object is grabbed.
    /// </summary>
    /// <param name="grabbingController">The object that is doing the grabbing.</param>
    /// <param name="grabbingAttachPoint">The point on the grabbing object that the grabbed object should be attached to after grab occurs.</param>
    /// <returns>Is true if the grab is successful, false if the grab is unsuccessful.</returns>
    public virtual bool StartGrab(GameObject grabbingController, Rigidbody grabbingAttachPoint) {
      grabbingObject = grabbingController;
      grabbingAttach = grabbingAttachPoint;
      interactable.PauseCollisions(onGrabCollisionDelay);
      return true;
    }
    #endregion

    #region Grab Stop
    /// <summary>
    /// The StopGrab method ends the grab of the current object and cleans up the state.
    /// </summary>
    /// <param name="applyGrabbingObjectVelocity">If true will apply the current velocity of the grabbing object to the grabbed object on release.</param>
    public virtual void StopGrab(bool applyGrabbingObjectVelocity) {
      grabbingObject = null;
      //interactableObject = null;
      grabbingAttach = null;
    }
    protected virtual Rigidbody ReleaseFromController(bool applyGrabbingObjectVelocity) {
      return rigidbody;
    }
    protected virtual void ForceReleaseGrab() {
      if (interactable) {
        GameObject grabbingObject = interactable.GetGrabbingObject();
        if (grabbingObject != null) {
          ControllerGrab grabbingObjectScript = grabbingObject.GetComponent<ControllerGrab>();
          if (grabbingObjectScript != null) {
            Debug.Log("this");
            grabbingObjectScript.ForceRelease();
          }
        }
      }
    }

    protected virtual void ReleaseObject(bool applyGrabbingObjectVelocity) {
      Rigidbody releasedObjectRigidBody = ReleaseFromController(applyGrabbingObjectVelocity);
      if (releasedObjectRigidBody != null && applyGrabbingObjectVelocity){
          ThrowReleasedObject(releasedObjectRigidBody);
      }
    }
    #endregion


		#region Helper 
    protected virtual void ThrowReleasedObject(Rigidbody objectRigidbody) {
      if (interactable != null)
      {
        VRTK_ControllerReference controllerReference = VRTK_ControllerReference.GetControllerReference(interactable.GetGrabbingObject());
        if (VRTK_ControllerReference.IsValid(controllerReference) && controllerReference.scriptAlias != null)
        {
          ControllerGrab grabbingObjectScript = controllerReference.scriptAlias.GetComponent<ControllerGrab>();
          if (grabbingObjectScript != null)
          {
            Transform origin = VRTK_DeviceFinder.GetControllerOrigin(controllerReference);

            Vector3 velocity = VRTK_DeviceFinder.GetControllerVelocity(controllerReference);
            Vector3 angularVelocity = VRTK_DeviceFinder.GetControllerAngularVelocity(controllerReference);
            float grabbingObjectThrowMultiplier = grabbingObjectScript.throwMultiplier;

            if (origin != null)
            {
              objectRigidbody.velocity = origin.TransformVector(velocity) * (grabbingObjectThrowMultiplier * throwMultiplier);
              objectRigidbody.angularVelocity = origin.TransformDirection(angularVelocity);
            }
            else
            {
              objectRigidbody.velocity = velocity * (grabbingObjectThrowMultiplier * throwMultiplier);
              objectRigidbody.angularVelocity = angularVelocity;
            }

            if (throwVelocityWithAttachDistance)
            {
              Collider rigidbodyCollider = objectRigidbody.GetComponentInChildren<Collider>();
              if (rigidbodyCollider != null)
              {
                Vector3 collisionCenter = rigidbodyCollider.bounds.center;
                objectRigidbody.velocity = objectRigidbody.GetPointVelocity(collisionCenter + (collisionCenter - transform.position));
              }
              else
              {
                objectRigidbody.velocity = objectRigidbody.GetPointVelocity(objectRigidbody.position + (objectRigidbody.position - transform.position));
              }
            }
          }
        }
      }
    }
		#endregion
	}
}
