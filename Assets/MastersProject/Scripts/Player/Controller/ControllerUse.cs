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
	public class ControllerUse : MonoBehaviour 
	{

		#region Public Fields
		#endregion

		#region Private Fields
    protected ControllerEvents controllerEvents;
    protected ControllerTouch controllerTouch;
    protected ControllerGrab controllerGrab;
		#endregion

		#region Bookkeeping 
    protected GameObject usingObject = null;
    protected ControllerEvents.ButtonTypes useButton = ControllerEvents.ButtonTypes.Undefined;
    protected bool usePressed;
    protected int usingState = 0;
    protected VRTK_ControllerReference controllerReference
    {
      get
      {
        return VRTK_ControllerReference.GetControllerReference((controllerTouch != null ? controllerTouch.gameObject : null));
      }
    }
		#endregion


    #region Delegates
    /// <summary>
    /// Emitted when the use toggle alias button is pressed.
    /// </summary>
    public event ControllerInteractionEventHandler UseButtonPressed;
    /// <summary>
    /// Emitted when the use toggle alias button is released.
    /// </summary>
    public event ControllerInteractionEventHandler UseButtonReleased;

    /// <summary>
    /// Emitted when a use of a valid object is started.
    /// </summary>
    public event ObjectInteractEventHandler ControllerStartUseInteractableObject;
    /// <summary>
    /// Emitted when a valid object starts being used.
    /// </summary>
    public event ObjectInteractEventHandler ControllerUseInteractableObject;
    /// <summary>
    /// Emitted when a unuse of a valid object is started.
    /// </summary>
    public event ObjectInteractEventHandler ControllerStartUnuseInteractableObject;
    /// <summary>
    /// Emitted when a valid object stops being used.
    /// </summary>
    public event ObjectInteractEventHandler ControllerUnuseInteractableObject;
    public virtual void OnControllerStartUseInteractableObject(ObjectInteractEventArgs e)
    {
      if (ControllerStartUseInteractableObject != null)
      {
        ControllerStartUseInteractableObject(this, e);
      }
    }

    public virtual void OnControllerUseInteractableObject(ObjectInteractEventArgs e)
    {
      if (ControllerUseInteractableObject != null)
      {
        ControllerUseInteractableObject(this, e);
      }
    }

    public virtual void OnControllerStartUnuseInteractableObject(ObjectInteractEventArgs e)
    {
      if (ControllerStartUnuseInteractableObject != null)
      {
        ControllerStartUnuseInteractableObject(this, e);
      }
    }

    public virtual void OnControllerUnuseInteractableObject(ObjectInteractEventArgs e)
    {
      if (ControllerUnuseInteractableObject != null)
      {
        ControllerUnuseInteractableObject(this, e);
      }
    }

    public virtual void OnUseButtonPressed(ControllerInteractionEventArgs e)
    {
      if (UseButtonPressed != null)
      {
        UseButtonPressed(this, e);
      }
    }

    public virtual void OnUseButtonReleased(ControllerInteractionEventArgs e)
    {
      if (UseButtonReleased != null)
      {
        UseButtonReleased(this, e);
      }
    }
    #endregion


    #region Public Methods
    /// <summary>
    /// The IsUsebuttonPressed method determines whether the current use alias button is being pressed down.
    /// </summary>
    /// <returns>Returns true if the use alias button is being held down.</returns>
    public virtual bool IsUseButtonPressed()
    {
      return usePressed;
    }

    /// <summary>
    /// The GetUsingObject method returns the current object being used by the controller.
    /// </summary>
    /// <returns>The game object of what is currently being used by this controller.</returns>
    public virtual GameObject GetUsingObject()
    {
      return usingObject;
    }

    /// <summary>
    /// The ForceStopUsing method will force the controller to stop using the currently touched object and will also stop the object's using action.
    /// </summary>
    public virtual void ForceStopUsing()
    {
      if (usingObject != null)
      {
        SetObjectUsingState(usingObject, 0);
        UnuseInteractedObject(true);
      }
    }

    /// <summary>
    /// The ForceResetUsing will force the controller to stop using the currently touched object but the object will continue with it's existing using action.
    /// </summary>
    public virtual void ForceResetUsing()
    {
      if (usingObject != null)
      {
        UnuseInteractedObject(false);
      }
    }

    /// <summary>
    /// The AttemptUse method will attempt to use the currently touched object without needing to press the use button on the controller.
    /// </summary>
    public virtual void AttemptUse()
    {
      AttemptUseObject();
    }
    #endregion


		#region Initilization 
    protected virtual void OnEnable()
    {
      controllerEvents = GetComponent<ControllerEvents>();
      controllerTouch = GetComponent<ControllerTouch>();
      controllerGrab = GetComponent<ControllerGrab>();

      if (controllerTouch == null || controllerGrab)
      {
        //VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_NOT_INJECTED, "VRTK_InteractUse", "VRTK_InteractTouch", "interactTouch", "the same or parent"));
      }

      ManageUseListener(true, useButton);
      ManageTouchListener(true);
    }
		#endregion

    #region Deinitilization 
    protected virtual void OnDisable()
    {
      ForceResetUsing();
      ManageUseListener(false, useButton);
      ManageTouchListener(false);
    }
    #endregion


    #region Core
    protected virtual void Update() {}
    #endregion


    #region Listeners
    protected virtual void ManageTouchListener(bool state)
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
    protected virtual void ManageUseListener(bool state, ControllerEvents.ButtonTypes newUseButton = ControllerEvents.ButtonTypes.Undefined)
    {
      if (controllerEvents != null)
      {
        if (!state || !useButton.Equals(newUseButton))
        {
          controllerEvents.UnsubscribeToButtonAliasEvent(useButton, true, DoStartUseObject);
          controllerEvents.UnsubscribeToButtonAliasEvent(useButton, false, DoStopUseObject);
          useButton = ControllerEvents.ButtonTypes.Undefined;
        }

        if (state && !useButton.Equals(newUseButton))
        {
          useButton = newUseButton;
          controllerEvents.SubscribeToButtonAliasEvent(useButton, true, DoStartUseObject);
          controllerEvents.SubscribeToButtonAliasEvent(useButton, false, DoStopUseObject);
        }
      }
    }
    #endregion

    #region Use Flow
    protected virtual void DoStartUseObject(object sender, ControllerInteractionEventArgs e)
    {
      OnUseButtonPressed(controllerEvents.SetControllerEvent(ref usePressed, true));
      AttemptUseObject();
    }

    protected virtual void DoStopUseObject(object sender, ControllerInteractionEventArgs e)
    {
      Debug.Log(GetObjectUsingState(usingObject));
      if (IsObjectHoldOnUse(usingObject) || GetObjectUsingState(usingObject) >= 2)
      {
        SetObjectUsingState(usingObject, 0);
        UnuseInteractedObject(true);
      }
      OnUseButtonReleased(controllerEvents.SetControllerEvent(ref usePressed, false));
    }
    protected virtual void AttemptUseObject()
    {
      GameObject touchedObject = GetFromGrab();
      if (touchedObject == null)
      {
        touchedObject = (controllerTouch != null ? controllerTouch.GetTouchedObject() : null);
      }

      if (touchedObject != null && controllerTouch != null && controllerTouch.IsObjectInteractable(touchedObject) && GetObjectUsingState(touchedObject) != 1)
      {
        InteractableObject interactableObjectScript = touchedObject.GetComponent<InteractableObject>();

        if (interactableObjectScript != null && !interactableObjectScript.IsGrabbed())
        {
          return;
        }

        UseInteractedObject(touchedObject);
        if (usingObject != null && !IsObjectHoldOnUse(usingObject))
        {
          SetObjectUsingState(usingObject, 1);
        }
      }
    }
    protected virtual void UseInteractedObject(GameObject touchedObject)
    {
      if ((usingObject == null || usingObject != touchedObject) && IsObjectUsable(touchedObject) && controllerTouch != null)
      {
        usingObject = touchedObject;
        OnControllerStartUseInteractableObject(controllerTouch.SetControllerInteractEvent(usingObject));
        InteractableObject usingObjectScript = (usingObject != null ? usingObject.GetComponent<InteractableObject>() : null);

        if (usingObjectScript != null)
        {
          usingObject = null;

          usingObjectScript.StartUsing(this);
          ToggleControllerVisibility(false);
          AttemptHaptics();
          OnControllerUseInteractableObject(controllerTouch.SetControllerInteractEvent(usingObject));
        }
      }
    }

    protected virtual void UnuseInteractedObject(bool completeStop)
    {
      if (usingObject != null && controllerTouch != null)
      {
        OnControllerStartUnuseInteractableObject(controllerTouch.SetControllerInteractEvent(usingObject));
        InteractableObject usingObjectCheck = usingObject.GetComponent<InteractableObject>();
        if (usingObjectCheck != null && completeStop)
        {
          usingObjectCheck.StopUsing(this);
        }
        ToggleControllerVisibility(true);
        OnControllerUnuseInteractableObject(controllerTouch.SetControllerInteractEvent(usingObject));
        usingObject = null;
      }
    }
    #endregion


    protected virtual bool IsObjectUsable(GameObject obj)
    {
      InteractableObject objScript = (obj != null ? obj.GetComponent<InteractableObject>() : null);
      return (obj != null && controllerTouch != null && controllerTouch.IsObjectInteractable(obj) && objScript != null && objScript.isUsable);
    }

    protected virtual bool IsObjectHoldOnUse(GameObject obj)
    {
      if (obj != null)
      {
        InteractableObject objScript = obj.GetComponent<InteractableObject>();
        return (objScript != null && objScript.holdButtonToUse);
      }
      return false;
    }

    protected virtual int GetObjectUsingState(GameObject obj)
    {
      if (obj != null)
      {
        InteractableObject objScript = obj.GetComponent<InteractableObject>();
        if (objScript != null)
        {
          return objScript.usingState;
        }
      }
      return 0;
    }

    protected virtual void SetObjectUsingState(GameObject obj, int value)
    {
      if (obj != null)
      {
        InteractableObject objScript = obj.GetComponent<InteractableObject>();
        if (objScript != null)
        {
          objScript.usingState = value;
        }
      }
    }

		#region Helper
    protected virtual void ControllerTouchInteractableObject(object sender, ObjectInteractEventArgs e) 
    {
      if (e.target != null)
      {
        InteractableObject touchedObjectScript = e.target.GetComponent<InteractableObject>();
        if (touchedObjectScript != null && touchedObjectScript.isUsable && touchedObjectScript.useButton != ControllerEvents.ButtonTypes.Undefined)
        {
          ManageUseListener(true, touchedObjectScript.useButton);
        }
      }
    }
    protected virtual void ControllerUntouchInteractableObject(object sender, ObjectInteractEventArgs e) 
    {
      if (e.target != null)
      {
        InteractableObject unTouchedObjectScript = e.target.GetComponent<InteractableObject>();
        if (unTouchedObjectScript != null && !unTouchedObjectScript.IsUsing() && useButton != ControllerEvents.ButtonTypes.Undefined)
        {
          ManageUseListener(false);
        }
      }
    }
    protected virtual GameObject GetFromGrab()
    {
      if (controllerGrab != null)
      {
        return controllerGrab.GetGrabbedObject();
      }
      return null;
    }
    protected virtual void AttemptHaptics()
    { 
      /* 
      if (usingObject != null)
      {
        VRTK_InteractHaptics doHaptics = usingObject.GetComponentInParent<VRTK_InteractHaptics>();
        if (doHaptics != null)
        {
          doHaptics.HapticsOnUse(controllerReference);
        }
      }
      */
    }

    protected virtual void ToggleControllerVisibility(bool visible)
    {
      /* 
      if (usingObject != null)
      {
        VRTK_InteractControllerAppearance[] controllerAppearanceScript = usingObject.GetComponentsInParent<VRTK_InteractControllerAppearance>(true);
        if (controllerAppearanceScript.Length > 0)
        {
          controllerAppearanceScript[0].ToggleControllerOnUse(visible, controllerReference.model, usingObject);
        }
      }
      */
    } 
		#endregion
	}
}
