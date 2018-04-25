//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Mostly VRTK
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	/// <summary>
  /// VRTK-Grab Mechanic that uses a fixed joint
  /// </summary>
	public class InteractableGrabJoint : InteractableGrabBase 
	{
		#region Public Fields
    [Header("Joint Options", order = 2)]

    [Tooltip("If this is checked then when the controller grabs the object, it will grab it with precision and pick it up at the particular point on the object the controller is touching.")]
    public bool precisionGrab;

    [Tooltip("A Transform provided as an empty game object which must be the child of the item being grabbed. Not required for `Precision Snap`.")]
    public Transform snapHandle;

    [Tooltip("Determines whether the joint should be destroyed immediately on release or whether to wait till the end of the frame before being destroyed.")]
    public bool destroyImmediatelyOnThrow = true;

    [Tooltip("Maximum force the joint can withstand before breaking. Infinity means unbreakable.")]
    public float breakForce = 1500f;
		#endregion
    //—————————
		#region Private Fields
		#endregion
    //—————————
		#region Bookkeeping 
    protected Joint controllerAttachJoint;
		#endregion
    //—————————
		#region Public Methods
    /// <summary>
    /// The ValidGrab method determines if the grab attempt is valid.
    /// </summary>
    /// <param name="checkAttachPoint"></param>
    /// <returns>Returns true if there is no current grab happening, or the grab is initiated by another grabbing object.</returns>
    public override bool ValidGrab(Rigidbody checkAttachPoint)
    {
      return (controllerAttachJoint == null || controllerAttachJoint.connectedBody != checkAttachPoint);
    }
		#endregion
    //—————————
		#region Initilization 
    protected override void Awake()
    {
      base.Awake();
      controllerAttachJoint = null;
    }
		#endregion
    //—————————
		#region Grab 
    /// <summary>
    /// The StartGrab method sets up the grab attach mechanic as soon as an object is grabbed. It is also responsible for creating the joint on the grabbed object.
    /// </summary>
    /// <param name="grabbingObject">The object that is doing the grabbing.</param>
    /// <param name="givenControllerAttachPoint">The point on the grabbing object that the grabbed object should be attached to after grab occurs.</param>
    /// <returns>Is true if the grab is successful, false if the grab is unsuccessful.</returns>
    public override bool StartGrab(GameObject grabbingObject, Rigidbody givenControllerAttachPoint)
    {
      base.StartGrab(grabbingObject, givenControllerAttachPoint);
      SnapObjectToGrabToController();
      return true;
    }
    protected virtual void SnapObjectToGrabToController()
    {
      if (!precisionGrab)
      {
        SetSnappedObjectPosition();
      }
      CreateJoint();
    }
    protected virtual void SetSnappedObjectPosition()
    {
      if (snapHandle == null)
      {
        gameObject.transform.position = transform.position;
      }
      else
      {
        gameObject.transform.rotation = snapHandle.transform.rotation * Quaternion.Euler(snapHandle.transform.localEulerAngles);
        gameObject.transform.position = snapHandle.transform.position - (snapHandle.transform.position - gameObject.transform.position);
      }
    }
    protected virtual void CreateJoint()
    {
      controllerAttachJoint = gameObject.AddComponent<FixedJoint>();
      controllerAttachJoint.breakForce = breakForce;
      if (precisionGrab)
      {
        controllerAttachJoint.anchor = gameObject.transform.InverseTransformPoint(grabbingAttach.transform.position);
      }
      controllerAttachJoint.connectedBody = grabbingAttach;
    }
    #endregion
    //—————————
    #region Stop Grab
    /// <summary>
    /// The StopGrab method ends the grab of the current object and cleans up the state. It is also responsible for removing the joint from the grabbed object.
    /// </summary>
    /// <param name="applyGrabbingObjectVelocity">If true will apply the current velocity of the grabbing object to the grabbed object on release.</param>
    public override void StopGrab(bool applyGrabbingObjectVelocity)
    {
      ReleaseObject(applyGrabbingObjectVelocity);
      base.StopGrab(applyGrabbingObjectVelocity);
    }
    protected override Rigidbody ReleaseFromController(bool applyGrabbingObjectVelocity)
    {
      if (controllerAttachJoint)
      {
        var jointRigidbody = controllerAttachJoint.GetComponent<Rigidbody>();
        DestroyJoint(destroyImmediatelyOnThrow, applyGrabbingObjectVelocity);
        controllerAttachJoint = null;

        return jointRigidbody;
      }
      return null;
    }
    protected virtual void DestroyJoint(bool withDestroyImmediate, bool applyGrabbingObjectVelocity)
    {
      controllerAttachJoint.connectedBody = null;
      if (withDestroyImmediate && applyGrabbingObjectVelocity)
      {
        DestroyImmediate(controllerAttachJoint);
      }
      else
      {
        Destroy(controllerAttachJoint);
      }
    }
		#endregion
    //—————————
		#region Helper 
    protected virtual void OnJointBreak(float force)
    {
      ForceReleaseGrab();
    }
		#endregion
	}
}
