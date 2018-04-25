//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRTK;

namespace PlayByPierce.Masters
{
	/// <summary>
  /// class description
  /// </summary>
	public class ControllerGrab : MonoBehaviour 
	{
    #region Public Fields
    [Header("Grab Settings")]
    public bool invisibleOnGrab = false;
		public bool doHaptics = false;
    public bool holdToGrab = false;
    public float grabPrecognition = 0f;
    [Tooltip("An amount to multiply the velocity of any objects being thrown. This can be useful when scaling up the play area to simulate being able to throw items further.")]
    public float throwMultiplier = 1f;
    [Tooltip("If this is checked and the controller is not touching an Interactable Object when the grab button is pressed then a rigid body is added to the controller to allow the controller to push other rigid body objects around.")]
    public bool createRigidBodyWhenNotTouching = false;

    [Header("Custom Settings")]

    [Tooltip("The rigidbody point on the controller model to snap the grabbed object to. If blank it will be set to the SDK default.")]
    public Rigidbody controllerAttachPoint = null;
    #endregion
    //—————————
		#region Bookkeeping 
    protected ControllerEvents controllerEvents;
    protected ControllerTouch controllerTouch;
    protected ControllerEvents.ButtonTypes grabButton = ControllerEvents.ButtonTypes.Undefined;
    protected GameObject grabbedObject = null;
    protected bool grabPressed;
    protected float grabPrecognitionTimer = 0f;
    protected VRTK_ControllerReference controllerReference
    {
      get
      {
        return VRTK_ControllerReference.GetControllerReference((controllerTouch != null ? controllerTouch.gameObject : null));
      }
    }
		#endregion
    //—————————
    #region Delegates
    /// <summary>
    /// Emitted when the grab button is pressed.
    /// </summary>
    public event ControllerInteractionEventHandler GrabButtonPressed;
    /// <summary>
    /// Emitted when the grab button is released.
    /// </summary>
    public event ControllerInteractionEventHandler GrabButtonReleased;

    /// <summary>
    /// Emitted when a grab of a valid object is started.
    /// </summary>
    public event ObjectInteractEventHandler ControllerStartGrabInteractableObject;
    /// <summary>
    /// Emitted when a valid object is grabbed.
    /// </summary>
    public event ObjectInteractEventHandler ControllerGrabInteractableObject;
    /// <summary>
    /// Emitted when a ungrab of a valid object is started.
    /// </summary>
    public event ObjectInteractEventHandler ControllerStartUngrabInteractableObject;
    /// <summary>
    /// Emitted when a valid object is released from being grabbed.
    /// </summary>
    public event ObjectInteractEventHandler ControllerUngrabInteractableObject;
    public virtual void OnControllerStartGrabInteractableObject(ObjectInteractEventArgs e)
    {
      if (ControllerStartGrabInteractableObject != null)
      {
        ControllerStartGrabInteractableObject(this, e);
      }
    }
    public virtual void OnControllerGrabInteractableObject(ObjectInteractEventArgs e)
    {
      if (ControllerGrabInteractableObject != null)
      {
        ControllerGrabInteractableObject(this, e);
      }
    }
    public virtual void OnControllerStartUngrabInteractableObject(ObjectInteractEventArgs e)
    {
      if (ControllerStartUngrabInteractableObject != null)
      {
        ControllerStartUngrabInteractableObject(this, e);
      }
    }
    public virtual void OnControllerUngrabInteractableObject(ObjectInteractEventArgs e)
    {
      if (ControllerUngrabInteractableObject != null)
      {
        ControllerUngrabInteractableObject(this, e);
      }
    }
    public virtual void OnGrabButtonPressed(ControllerInteractionEventArgs e)
    {
      if (GrabButtonPressed != null)
      {
        GrabButtonPressed(this, e);
      }
    }
    public virtual void OnGrabButtonReleased(ControllerInteractionEventArgs e)
    {
      if (GrabButtonReleased != null)
      {
        GrabButtonReleased(this, e);
      }
    }
    protected virtual void ControllerIndexChanged(object sender, ControllerInteractionEventArgs e)
    {
      CheckControllerAttachPoint();
    }
    #endregion
    //—————————
    #region Public Methods
    /// <summary>
    /// The IsGrabButtonPressed method determines whether the current grab alias button is being pressed down.
    /// </summary>
    /// <returns>Returns true if the grab alias button is being held down.</returns>
    public virtual bool IsGrabButtonPressed()
    {
      return grabPressed;
    }

    /// <summary>
    /// The ForceRelease method will force the controller to stop grabbing the currently grabbed object.
    /// </summary>
    /// <param name="applyGrabbingObjectVelocity">If this is true then upon releasing the object any velocity on the grabbing object will be applied to the object to essentiall throw it. Defaults to `false`.</param>
    public virtual void ForceRelease(bool applyGrabbingObjectVelocity = false)
    {
      if (grabbedObject)
      {
        Debug.Log("ForceReleaseCalled");
        UnGrab(applyGrabbingObjectVelocity);
      }
    }

    /// <summary>
    /// The AttemptGrab method will attempt to grab the currently touched object without needing to press the grab button on the controller.
    /// </summary>
    public virtual void AttemptGrab()
    {
      AttemptGrabObject(true);
    }

    /// <summary>
    /// The GetGrabbedObject method returns the current object being grabbed by the controller.
    /// </summary>
    /// <returns>The game object of what is currently being grabbed by this controller.</returns>
    public virtual GameObject GetGrabbedObject()
    {
      return grabbedObject;
    }
    #endregion
    //—————————
		#region Initilization 
    protected virtual void Awake()
    {
      CheckControllerAttachPoint();
      controllerEvents = (controllerEvents != null ? controllerEvents : GetComponentInParent<ControllerEvents>());
      controllerTouch = (controllerTouch != null ? controllerTouch : GetComponentInParent<ControllerTouch>());
      if (controllerTouch == null)
      {
        //VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_NOT_INJECTED, "VRTK_InteractGrab", "VRTK_InteractTouch", "interactTouch", "the same or parent"));
      }

      VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
    }
    protected virtual void OnEnable()
    {
      ManageGrabListener(true);
      ManageInteractTouchListener(true);
      if (controllerEvents != null)
      {
        controllerEvents.ControllerIndexChanged += ControllerIndexChanged;
      }
      CheckControllerAttachPoint();
    }
		#endregion
    //—————————
		#region Core 
    protected virtual void Update()
    {
      ManageGrabListener(true);
      CheckControllerAttachPoint();
      CreateNonTouchingRigidbody();
      CheckPrecognitionGrab();
    }
    protected virtual void CheckPrecognitionGrab()
    {
      if (grabPrecognitionTimer >= Time.time)
      {
        GameObject grabbedObject = (controllerTouch != null ? controllerTouch.GetTouchedObject() : null);
        if (grabbedObject != null)
        {
          AttemptGrabObject(false);
          if (GetGrabbedObject() != null)
          {
              grabPrecognitionTimer = 0f;
          }
        }
      }
    }
    #endregion
    //—————————
    #region Button
    protected virtual void GrabPressed(object sender, ControllerInteractionEventArgs e)
    {
      OnGrabButtonPressed(controllerEvents.SetControllerEvent(ref grabPressed, true));
      if (holdToGrab)
      {
        AttemptGrabObject(true);
      }
      else
      {
        if (grabbedObject)
        {
          AttemptUnGrabObject();
        }
        else
        {
          AttemptGrabObject(true);
        }
      }
      
    }
    protected virtual void GrabReleased(object sender, ControllerInteractionEventArgs e)
    {
      if (holdToGrab)
      {
        AttemptUnGrabObject();
      }
      OnGrabButtonReleased(controllerEvents.SetControllerEvent(ref grabPressed, false));
    }
    #endregion
    //—————————
    #region Grab
    protected virtual void AttemptGrabObject(bool startPrecognition)
    {
      GameObject grabbed = (controllerTouch != null ? controllerTouch.GetTouchedObject() : null);
      InteractableObject grabbedScript = (grabbed != null ? grabbed.GetComponent<InteractableObject>() : null);
      if (grabbed != null && grabbedScript != null && grabbedScript.isGrabbable && grabbedScript.grabAttachMechanicScript.ValidGrab(controllerAttachPoint))
      {
        if (!grabbedScript.IsGrabbed())
        {
          //AttemptHaptics(initialGrabAttempt);
          Grab(grabbed, grabbedScript);
        }
      }
      else if (startPrecognition && grabPrecognitionTimer == 0)
      {
        grabPrecognitionTimer = Time.time + grabPrecognition;
      }
    }
    protected virtual void Grab(GameObject grabbed, InteractableObject grabbedScript)
    {
      grabbedObject = grabbed;

      OnControllerStartGrabInteractableObject(controllerTouch.SetControllerInteractEvent(grabbedObject));

      grabbedScript.SaveCurrentState();
      grabbedScript.Grabbed(this, controllerAttachPoint);
      grabbedScript.ZeroVelocity();
      grabbedScript.isKinematic = false;
      grabbedScript.ToggleHighlight(false);
      //ToggleControllerVisibility(false);

      OnControllerGrabInteractableObject(controllerTouch.SetControllerInteractEvent(grabbedObject));
    }
    #endregion
    //—————————
    #region UnGrab

    protected virtual void AttemptUnGrabObject()
    {
      if (grabbedObject != null && controllerTouch.GetInteractableScript(grabbedObject) != null)
      {
        UnGrab(true);
      }
    }
    protected virtual void UnGrab(bool applyGrabbingObjectVelocity)
    {
      //Debug.Log("Ungrabbing Object");
      InteractableObject unGrabbedScript = controllerTouch.GetInteractableScript(grabbedObject);
      OnControllerStartUngrabInteractableObject(controllerTouch.SetControllerInteractEvent(grabbedObject));

      unGrabbedScript.Ungrabbed(this);
      unGrabbedScript.ToggleHighlight(false);
      //ToggleControllerVisibility(true);

      OnControllerUngrabInteractableObject(controllerTouch.SetControllerInteractEvent(grabbedObject));

      grabbedObject = null;
    }

    #endregion
    //—————————
    #region Listeners
    protected virtual void ManageInteractTouchListener(bool state)
    {
      if (controllerTouch != null && !state)
      {
        controllerTouch.ControllerTouchInteractableObject -= ControllerTouchInteractableObject;
        controllerTouch.ControllerUntouchInteractableObject -= ControllerUntouchInteractableObject;
      }

      if (controllerTouch != null && state)
      {
        controllerTouch.ControllerTouchInteractableObject += ControllerTouchInteractableObject;
        controllerTouch.ControllerUntouchInteractableObject += ControllerUntouchInteractableObject;
      }
    }
    protected virtual void ManageGrabListener(bool state)
    {
      if (controllerEvents != null && controllerEvents.grabButton != ControllerEvents.ButtonTypes.Undefined)
      {
        if (!state || !grabButton.Equals(controllerEvents.grabButton))
        {
          controllerEvents.UnsubscribeToButtonAliasEvent(grabButton, true, GrabPressed);
          controllerEvents.UnsubscribeToButtonAliasEvent(grabButton, false, GrabReleased);
          grabButton = ControllerEvents.ButtonTypes.Undefined;
        }

        if (state && !grabButton.Equals(controllerEvents.grabButton))
        {
          grabButton = controllerEvents.grabButton;
          controllerEvents.SubscribeToButtonAliasEvent(grabButton, true, GrabPressed);
          controllerEvents.SubscribeToButtonAliasEvent(grabButton, false, GrabReleased);
        }
      }
    }
    #endregion
    //—————————
		#region Helper 
    protected virtual void CheckControllerAttachPoint()
    {
      if (controllerAttachPoint == null)
      {
        //If no attach point has been specified then just use the tip of the controller
        if (controllerReference.model != null && controllerAttachPoint == null)
        {
          //attempt to find the attach point on the controller
          SDK_BaseController.ControllerHand handType = VRTK_DeviceFinder.GetControllerHand(controllerTouch.gameObject);
          string elementPath = VRTK_SDK_Bridge.GetControllerElementPath(SDK_BaseController.ControllerElements.AttachPoint, handType);
          controllerAttachPoint = controllerReference.model.transform.Find(elementPath).GetComponent<Rigidbody>();
          if (controllerAttachPoint == null)
          {
            Rigidbody autoGenRB = controllerReference.model.transform.Find(elementPath).gameObject.AddComponent<Rigidbody>();
            autoGenRB.isKinematic = true;
            controllerAttachPoint = autoGenRB;
          }
        }
      }
    }
    protected virtual bool IsObjectHoldOnGrab(GameObject obj)
    {
      if (obj != null)
      {
        InteractableObject objScript = obj.GetComponent<InteractableObject>();
        return (objScript != null);
      }
      return false;
    }
    protected virtual GameObject GetGrabbableObject()
    {
      GameObject obj = (controllerTouch != null ? controllerTouch.GetTouchedObject() : null);
      return obj;
    }
    protected virtual void CreateNonTouchingRigidbody()
    {
      /* 
        if (createRigidBodyWhenNotTouching && grabbedObject == null && controllerTouch != null)
        {
            if (controllerTouch.IsRigidBodyActive() != grabPressed)
            {
                controllerTouch.ToggleControllerRigidBody(grabPressed);
            }
        }
       */ 
    }
    protected virtual void AttemptHaptics(bool initialGrabAttempt)
    {
      /* 
      if (grabbedObject != null && initialGrabAttempt)
      {
        VRTK_InteractHaptics doHaptics = grabbedObject.GetComponentInParent<VRTK_InteractHaptics>();
        if (doHaptics != null)
        {
          doHaptics.HapticsOnGrab(controllerReference);
        }
      }
      */
    }
    protected virtual void ControllerTouchInteractableObject(object sender, ObjectInteractEventArgs e) {}
    protected virtual void ControllerUntouchInteractableObject(object sender, ObjectInteractEventArgs e) {}
    protected virtual void ToggleControllerVisibility(bool visible)
    {
      if (grabbedObject != null)
      {
          VRTK_InteractControllerAppearance[] controllerAppearanceScript = grabbedObject.GetComponentsInParent<VRTK_InteractControllerAppearance>(true);
          if (controllerAppearanceScript.Length > 0)
          {
              controllerAppearanceScript[0].ToggleControllerOnGrab(visible, controllerReference.model, grabbedObject);
          }
      }
      else if (visible)
      {
          VRTK_ObjectAppearance.SetRendererVisible(controllerReference.model, grabbedObject);
      }
    }
		#endregion
    //—————————
    #region Deinitilization
    protected virtual void OnDisable()
    {
      ForceRelease();
      ManageGrabListener(false);
      ManageInteractTouchListener(false);
      if (controllerEvents != null)
      {
        controllerEvents.ControllerIndexChanged -= ControllerIndexChanged;
      }
    }
    protected virtual void OnDestroy()
    {
      VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
    }
    #endregion
	}
}
