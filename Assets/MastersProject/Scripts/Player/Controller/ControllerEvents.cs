//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     VRTK
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

using VRTK;

namespace PlayByPierce.Masters 
{
	/// <summary>
	/// Event Payload
	/// </summary>
	/// <param name="controllerIndex">**OBSOLETE** The index of the controller that was used.</param>
	/// <param name="controllerReference">The reference for the controller that was used.</param>
	/// <param name="buttonPressure">The amount of pressure being applied to the button pressed. `0f` to `1f`.</param>
	/// <param name="touchpadAxis">The position the touchpad is touched at. `(0,0)` to `(1,1)`.</param>
	/// <param name="touchpadAngle">The rotational position the touchpad is being touched at, 0 being top, 180 being bottom and all other angles accordingly. `0f` to `360f`.</param>
	public struct ControllerInteractionEventArgs
	{
		public VRTK_ControllerReference controllerReference;
		public float buttonPressure;
		public Vector2 touchpadAxis;
		public float touchpadAngle;
	}

	/// <summary>
	/// Event Payload
	/// </summary>
	/// <param name="sender">this object</param>
	/// <param name="e"><see cref="ControllerInteractionEventArgs"/></param>
	public delegate void ControllerInteractionEventHandler(object sender, ControllerInteractionEventArgs e);

	/// <summary>
  /// VRTK Controller Events
  /// </summary>
	public class ControllerEvents : MonoBehaviour 
	{
		#region Public Fields 
    /// <summary>
    /// Button types
    /// </summary>
    /// <param name="Undefined">No button specified</param>
    /// <param name="TriggerTouch">The trigger is squeezed a small amount.</param>
    /// <param name="TriggerPress">The trigger is squeezed about half way in.</param>
    /// <param name="GripTouch">The grip button is touched.</param>
    /// <param name="GripPress">The grip button is pressed.</param>
    /// <param name="TouchpadTouch">The touchpad is touched (without pressing down to click).</param>
    /// <param name="TouchpadPress">The touchpad is pressed (to the point of hearing a click).</param>
    /// <param name="ButtonOneTouch">The button one is touched.</param>
    /// <param name="ButtonOnePress">The button one is pressed.</param>
    /// <param name="ButtonTwoTouch">The button one is touched.</param>
    /// <param name="ButtonTwoPress">The button one is pressed.</param>
    /// <param name="StartMenuPress">The button one is pressed.</param>
    public enum ButtonTypes
    {
      Undefined,
      TriggerTouch,
      TriggerPress,
      GripTouch,
      GripPress,
      TouchpadTouch,
      TouchpadPress,
      ButtonOneTouch,
      ButtonOnePress,
      ButtonTwoTouch,
      ButtonTwoPress,
      StartMenuPress
    }			
    [Header("Oculus Controls")]
    [Tooltip("The button to use for the action of grabbing game objects.")]
    public ButtonTypes oculusGrabButton = ButtonTypes.TriggerPress;
    [Tooltip("The button to use for the action of clicking a UI element.")]
    public ButtonTypes oculusUIButton = ButtonTypes.TriggerPress;
    [Tooltip("The button to use for the action of bringing up an in-game menu.")]
    public ButtonTypes oculusMenuButton = ButtonTypes.ButtonTwoPress;

    [Header("Vive Controls")]
    [Tooltip("The button to use for the action of grabbing game objects.")]
    public ButtonTypes viveGrabButton = ButtonTypes.TriggerPress;
    [Tooltip("The button to use for the action of clicking a UI element.")]
    public ButtonTypes viveUIButton = ButtonTypes.TriggerPress;
    [Tooltip("The button to use for the action of bringing up an in-game menu.")]
    public ButtonTypes viveMenuButton = ButtonTypes.ButtonTwoPress;

    [Header("Refinement")]

    [Tooltip("The amount of fidelity in the changes on the axis, which is defaulted to 1. Any number higher than 2 will probably give too sensitive results.")]
    public int axisFidelity = 1;
    [Tooltip("The level on the trigger axis to reach before a click is registered.")]
    public float triggerClickThreshold = 1f;
    [Tooltip("The level on the trigger axis to reach before the axis is forced to 0f.")]
    public float triggerForceZeroThreshold = 0.01f;
    [Tooltip("If this is checked then the trigger axis will be forced to 0f when the trigger button reports an untouch event.")]
    public bool triggerAxisZeroOnUntouch = false;
    [Tooltip("The level on the grip axis to reach before a click is registered.")]
    public float gripClickThreshold = 1f;
    [Tooltip("The level on the grip axis to reach before the axis is forced to 0f.")]
    public float gripForceZeroThreshold = 0.01f;
    [Tooltip("If this is checked then the grip axis will be forced to 0f when the grip button reports an untouch event.")]
    public bool gripAxisZeroOnUntouch = false;
		#endregion

    #region Private Fields
    public ButtonTypes grabButton
    {
      get
      {
        if (currentPlatform == PlatformMode.Oculus) 
        {
          return oculusGrabButton;
        }  
        else if (currentPlatform == PlatformMode.Vive) 
        {
          return viveGrabButton;
        }
        else 
        {
          //TODO - Figure out a way to make a proper load order and have this script wait for VRTK_SDKManager
          //Debug.LogError("No VR Platform Detected, Undefined Grab Returned");
          return ButtonTypes.Undefined;
        }
      }
    }
    public ButtonTypes uiButton
    {
      get
      {
        if (currentPlatform == PlatformMode.Oculus) 
        {
          return oculusUIButton;
        }  
        else if (currentPlatform == PlatformMode.Vive) 
        {
          return viveUIButton;
        }
        else 
        {
          Debug.LogError("No VR Platform Detected, Undefined Grab Returned");
          return ButtonTypes.Undefined;
        }
      }
    }
    public ButtonTypes menuButton
    {
      get
      {
        if (currentPlatform == PlatformMode.Oculus) 
        {
          return oculusMenuButton;
        }  
        else if (currentPlatform == PlatformMode.Vive) 
        {
          return viveMenuButton;
        }
        else 
        {
          Debug.LogError("No VR Platform Detected, Undefined Grab Returned");
          return ButtonTypes.Undefined;
        }
      }
    }

    /// <summary>
    /// This will be true if the trigger is squeezed about half way in.
    /// </summary>
    [HideInInspector] public bool triggerPressed = false;
    /// <summary>
    /// This will be true if the trigger is squeezed a small amount.
    /// </summary>
    [HideInInspector] public bool triggerTouched = false;
    /// <summary>
    /// This will be true if the trigger is squeezed a small amount more from any previous squeeze on the trigger.
    /// </summary>
    [HideInInspector] public bool triggerHairlinePressed = false;
    /// <summary>
    /// This will be true if the trigger is squeezed all the way down.
    /// </summary>
    [HideInInspector] public bool triggerClicked = false;
    /// <summary>
    /// This will be true if the trigger has been squeezed more or less.
    /// </summary>
    [HideInInspector] public bool triggerAxisChanged = false;

    /// <summary>
    /// This will be true if the grip is squeezed about half way in.
    /// </summary>
    [HideInInspector] public bool gripPressed = false;
    /// <summary>
    /// This will be true if the grip is touched.
    /// </summary>
    [HideInInspector] public bool gripTouched = false;
    /// <summary>
    /// This will be true if the grip is squeezed a small amount more from any previous squeeze on the grip.
    /// </summary>
    [HideInInspector] public bool gripHairlinePressed = false;
    /// <summary>
    /// This will be true if the grip is squeezed all the way down.
    /// </summary>
    [HideInInspector] public bool gripClicked = false;
    /// <summary>
    /// This will be true if the grip has been squeezed more or less.
    /// </summary>
    [HideInInspector] public bool gripAxisChanged = false;
    /// <summary>
    /// This will be true if the touchpad is held down.
    /// </summary>
    [HideInInspector] public bool touchpadPressed = false;
    /// <summary>
    /// This will be true if the touchpad is being touched.
    /// </summary>
    [HideInInspector] public bool touchpadTouched = false;
    /// <summary>
    /// This will be true if the touchpad touch position has changed.
    /// </summary>
    [HideInInspector] public bool touchpadAxisChanged = false;
    /// <summary>
    /// This will be true if button one is held down.
    /// </summary>
    [HideInInspector] public bool buttonOnePressed = false;
    /// <summary>
    /// This will be true if button one is being touched.
    /// </summary>
    [HideInInspector] public bool buttonOneTouched = false;
    /// <summary>This will be true if button two is held down.</summary>
    [HideInInspector] public bool buttonTwoPressed = false;
    /// <summary>
    /// This will be true if button two is being touched.
    /// </summary>
    [HideInInspector] public bool buttonTwoTouched = false;
    /// <summary>
    /// This will be true if start menu is held down.
    /// </summary>
    [HideInInspector] public bool startMenuPressed = false;			
    /// <summary>
    /// This will be true if the controller model alias renderers are visible.
    /// </summary>
    [HideInInspector] public bool controllerVisible = true;
    #endregion

    #region Bookkeeping
    protected enum PlatformMode { Oculus, Vive, Undefined };
    protected PlatformMode currentPlatform = PlatformMode.Undefined;	
    protected Vector2 touchpadAxis = Vector2.zero;
    protected Vector2 triggerAxis = Vector2.zero;
    protected Vector2 gripAxis = Vector2.zero;
    protected float hairTriggerDelta;
    protected float hairGripDelta;
		#endregion

    #region Delegates
    /// <summary>
    /// Emitted when the trigger is squeezed about half way in.
    /// </summary>
    public event ControllerInteractionEventHandler TriggerPressed;
    /// <summary>
    /// Emitted when the trigger is released under half way.
    /// </summary>
    public event ControllerInteractionEventHandler TriggerReleased;

    /// <summary>
    /// Emitted when the trigger is squeezed a small amount.
    /// </summary>
    public event ControllerInteractionEventHandler TriggerTouchStart;
    /// <summary>
    /// Emitted when the trigger is no longer being squeezed at all.
    /// </summary>
    public event ControllerInteractionEventHandler TriggerTouchEnd;

    /// <summary>
    /// Emitted when the trigger is squeezed past the current hairline threshold.
    /// </summary>
    public event ControllerInteractionEventHandler TriggerHairlineStart;
    /// <summary>
    /// Emitted when the trigger is released past the current hairline threshold.
    /// </summary>
    public event ControllerInteractionEventHandler TriggerHairlineEnd;

    /// <summary>
    /// Emitted when the trigger is squeezed all the way down.
    /// </summary>
    public event ControllerInteractionEventHandler TriggerClicked;
    /// <summary>
    /// Emitted when the trigger is no longer being held all the way down.
    /// </summary>
    public event ControllerInteractionEventHandler TriggerUnclicked;

    /// <summary>
    /// Emitted when the amount of squeeze on the trigger changes.
    /// </summary>
    public event ControllerInteractionEventHandler TriggerAxisChanged;

    /// <summary>
    /// Emitted when the grip is squeezed about half way in.
    /// </summary>
    public event ControllerInteractionEventHandler GripPressed;
    /// <summary>
    /// Emitted when the grip is released under half way.
    /// </summary>
    public event ControllerInteractionEventHandler GripReleased;

    /// <summary>
    /// Emitted when the grip is squeezed a small amount.
    /// </summary>
    public event ControllerInteractionEventHandler GripTouchStart;
    /// <summary>
    /// Emitted when the grip is no longer being squeezed at all.
    /// </summary>
    public event ControllerInteractionEventHandler GripTouchEnd;

    /// <summary>
    /// Emitted when the grip is squeezed past the current hairline threshold.
    /// </summary>
    public event ControllerInteractionEventHandler GripHairlineStart;
    /// <summary>
    /// Emitted when the grip is released past the current hairline threshold.
    /// </summary>
    public event ControllerInteractionEventHandler GripHairlineEnd;

    /// <summary>
    /// Emitted when the grip is squeezed all the way down.
    /// </summary>
    public event ControllerInteractionEventHandler GripClicked;
    /// <summary>
    /// Emitted when the grip is no longer being held all the way down.
    /// </summary>
    public event ControllerInteractionEventHandler GripUnclicked;

    /// <summary>
    /// Emitted when the amount of squeeze on the grip changes.
    /// </summary>
    public event ControllerInteractionEventHandler GripAxisChanged;

    /// <summary>
    /// Emitted when the touchpad is pressed (to the point of hearing a click).
    /// </summary>
    public event ControllerInteractionEventHandler TouchpadPressed;
    /// <summary>
    /// Emitted when the touchpad has been released after a pressed state.
    /// </summary>
    public event ControllerInteractionEventHandler TouchpadReleased;

    /// <summary>
    /// Emitted when the touchpad is touched (without pressing down to click).
    /// </summary>
    public event ControllerInteractionEventHandler TouchpadTouchStart;
    /// <summary>
    /// Emitted when the touchpad is no longer being touched.
    /// </summary>
    public event ControllerInteractionEventHandler TouchpadTouchEnd;

    /// <summary>
    /// Emitted when the touchpad is being touched in a different location.
    /// </summary>
    public event ControllerInteractionEventHandler TouchpadAxisChanged;

    /// <summary>
    /// Emitted when button one is touched.
    /// </summary>
    public event ControllerInteractionEventHandler ButtonOneTouchStart;
    /// <summary>
    /// Emitted when button one is no longer being touched.
    /// </summary>
    public event ControllerInteractionEventHandler ButtonOneTouchEnd;
    /// <summary>
    /// Emitted when button one is pressed.
    /// </summary>
    public event ControllerInteractionEventHandler ButtonOnePressed;
    /// <summary>
    /// Emitted when button one is released.
    /// </summary>
    public event ControllerInteractionEventHandler ButtonOneReleased;

    /// <summary>
    /// Emitted when button two is touched.
    /// </summary>
    public event ControllerInteractionEventHandler ButtonTwoTouchStart;
    /// <summary>
    /// Emitted when button two is no longer being touched.
    /// </summary>
    public event ControllerInteractionEventHandler ButtonTwoTouchEnd;
    /// <summary>
    /// Emitted when button two is pressed.
    /// </summary>
    public event ControllerInteractionEventHandler ButtonTwoPressed;
    /// <summary>
    /// Emitted when button two is released.
    /// </summary>
    public event ControllerInteractionEventHandler ButtonTwoReleased;

    /// <summary>
    /// Emitted when start menu is pressed.
    /// </summary>
    public event ControllerInteractionEventHandler StartMenuPressed;
    /// <summary>
    /// Emitted when start menu is released.
    /// </summary>
    public event ControllerInteractionEventHandler StartMenuReleased;

    /// <summary>
    /// Emitted when the grab toggle alias button is pressed.
    /// </summary>
    public event ControllerInteractionEventHandler ControlsGrabOn;
    /// <summary>
    /// Emitted when the grab toggle alias button is released.
    /// </summary>
    public event ControllerInteractionEventHandler ControlsGrabOff;

    /// <summary>
    /// Emitted when the use toggle alias button is pressed.
    /// </summary>
    public event ControllerInteractionEventHandler ControlsUseOn;
    /// <summary>
    /// Emitted when the use toggle alias button is released.
    /// </summary>
    public event ControllerInteractionEventHandler ControlsUseOff;

    /// <summary>
    /// Emitted when the menu toggle alias button is pressed.
    /// </summary>
    public event ControllerInteractionEventHandler ControlsMenuOn;
    /// <summary>
    /// Emitted when the menu toggle alias button is released.
    /// </summary>
    public event ControllerInteractionEventHandler ControlsMenuOff;

    /// <summary>
    /// Emitted when the UI click alias button is pressed.
    /// </summary>
    public event ControllerInteractionEventHandler ControlsUIOn;
    /// <summary>
    /// Emitted when the UI click alias button is released.
    /// </summary>
    public event ControllerInteractionEventHandler ControlsUIOff;

    /// <summary>
    /// Emitted when the controller is enabled.
    /// </summary>
    public event ControllerInteractionEventHandler ControllerEnabled;
    /// <summary>
    /// Emitted when the controller is disabled.
    /// </summary>
    public event ControllerInteractionEventHandler ControllerDisabled;
    /// <summary>
    /// Emitted when the controller index changed.
    /// </summary>
    public event ControllerInteractionEventHandler ControllerIndexChanged;

    /// <summary>
    /// Emitted when the controller is set to visible.
    /// </summary>
    public event ControllerInteractionEventHandler ControllerVisible;
    /// <summary>
    /// Emitted when the controller is set to hidden.
    /// </summary>
    public event ControllerInteractionEventHandler ControllerHidden;
    public virtual void OnTriggerPressed(ControllerInteractionEventArgs e)
    {
      if (TriggerPressed != null)
      {
        TriggerPressed(this, e);
      }
    }

    public virtual void OnTriggerReleased(ControllerInteractionEventArgs e)
    {
      if (TriggerReleased != null)
      {
        TriggerReleased(this, e);
      }
    }

    public virtual void OnTriggerTouchStart(ControllerInteractionEventArgs e)
    {
      if (TriggerTouchStart != null)
      {
        TriggerTouchStart(this, e);
      }
    }

    public virtual void OnTriggerTouchEnd(ControllerInteractionEventArgs e)
    {
      if (TriggerTouchEnd != null)
      {
        TriggerTouchEnd(this, e);
      }
    }

    public virtual void OnTriggerHairlineStart(ControllerInteractionEventArgs e)
    {
      if (TriggerHairlineStart != null)
      {
        TriggerHairlineStart(this, e);
      }
    }

    public virtual void OnTriggerHairlineEnd(ControllerInteractionEventArgs e)
    {
      if (TriggerHairlineEnd != null)
      {
        TriggerHairlineEnd(this, e);
      }
    }

    public virtual void OnTriggerClicked(ControllerInteractionEventArgs e)
    {
      if (TriggerClicked != null)
      {
        TriggerClicked(this, e);
      }
    }

    public virtual void OnTriggerUnclicked(ControllerInteractionEventArgs e)
    {
      if (TriggerUnclicked != null)
      {
        TriggerUnclicked(this, e);
      }
    }

    public virtual void OnTriggerAxisChanged(ControllerInteractionEventArgs e)
    {
      if (TriggerAxisChanged != null)
      {
        TriggerAxisChanged(this, e);
      }
    }

    public virtual void OnGripPressed(ControllerInteractionEventArgs e)
    {
      if (GripPressed != null)
      {
        GripPressed(this, e);
      }
    }

    public virtual void OnGripReleased(ControllerInteractionEventArgs e)
    {
      if (GripReleased != null)
      {
        GripReleased(this, e);
      }
    }

    public virtual void OnGripTouchStart(ControllerInteractionEventArgs e)
    {
      if (GripTouchStart != null)
      {
        GripTouchStart(this, e);
      }
    }

    public virtual void OnGripTouchEnd(ControllerInteractionEventArgs e)
    {
      if (GripTouchEnd != null)
      {
        GripTouchEnd(this, e);
      }
    }

    public virtual void OnGripHairlineStart(ControllerInteractionEventArgs e)
    {
      if (GripHairlineStart != null)
      {
        GripHairlineStart(this, e);
      }
    }

    public virtual void OnGripHairlineEnd(ControllerInteractionEventArgs e)
    {
      if (GripHairlineEnd != null)
      {
        GripHairlineEnd(this, e);
      }
    }

    public virtual void OnGripClicked(ControllerInteractionEventArgs e)
    {
      if (GripClicked != null)
      {
        GripClicked(this, e);
      }
    }

    public virtual void OnGripUnclicked(ControllerInteractionEventArgs e)
    {
      if (GripUnclicked != null)
      {
        GripUnclicked(this, e);
      }
    }

    public virtual void OnGripAxisChanged(ControllerInteractionEventArgs e)
    {
      if (GripAxisChanged != null)
      {
        GripAxisChanged(this, e);
      }
    }

    public virtual void OnTouchpadPressed(ControllerInteractionEventArgs e)
    {
      if (TouchpadPressed != null)
      {
        TouchpadPressed(this, e);
      }
    }

    public virtual void OnTouchpadReleased(ControllerInteractionEventArgs e)
    {
      if (TouchpadReleased != null)
      {
        TouchpadReleased(this, e);
      }
    }

    public virtual void OnTouchpadTouchStart(ControllerInteractionEventArgs e)
    {
      if (TouchpadTouchStart != null)
      {
        TouchpadTouchStart(this, e);
      }
    }

    public virtual void OnTouchpadTouchEnd(ControllerInteractionEventArgs e)
    {
      if (TouchpadTouchEnd != null)
      {
        TouchpadTouchEnd(this, e);
      }
    }

    public virtual void OnTouchpadAxisChanged(ControllerInteractionEventArgs e)
    {
      if (TouchpadAxisChanged != null)
      {
        TouchpadAxisChanged(this, e);
      }
    }

    public virtual void OnButtonOneTouchStart(ControllerInteractionEventArgs e)
    {
      if (ButtonOneTouchStart != null)
      {
        ButtonOneTouchStart(this, e);
      }
    }

    public virtual void OnButtonOneTouchEnd(ControllerInteractionEventArgs e)
    {
      if (ButtonOneTouchEnd != null)
      {
        ButtonOneTouchEnd(this, e);
      }
    }

    public virtual void OnButtonOnePressed(ControllerInteractionEventArgs e)
    {
      if (ButtonOnePressed != null)
      {
        ButtonOnePressed(this, e);
      }
    }

    public virtual void OnButtonOneReleased(ControllerInteractionEventArgs e)
    {
      if (ButtonOneReleased != null)
      {
        ButtonOneReleased(this, e);
      }
    }

    public virtual void OnButtonTwoTouchStart(ControllerInteractionEventArgs e)
    {
      if (ButtonTwoTouchStart != null)
      {
        ButtonTwoTouchStart(this, e);
      }
    }

    public virtual void OnButtonTwoTouchEnd(ControllerInteractionEventArgs e)
    {
      if (ButtonTwoTouchEnd != null)
      {
        ButtonTwoTouchEnd(this, e);
      }
    }

    public virtual void OnButtonTwoPressed(ControllerInteractionEventArgs e)
    {
      if (ButtonTwoPressed != null)
      {
        ButtonTwoPressed(this, e);
      }
    }

    public virtual void OnButtonTwoReleased(ControllerInteractionEventArgs e)
    {
      if (ButtonTwoReleased != null)
      {
        ButtonTwoReleased(this, e);
      }
    }

    public virtual void OnStartMenuPressed(ControllerInteractionEventArgs e)
    {
      if (StartMenuPressed != null)
      {
        StartMenuPressed(this, e);
      }
    }

    public virtual void OnStartMenuReleased(ControllerInteractionEventArgs e)
    {
      if (StartMenuReleased != null)
      {
        StartMenuReleased(this, e);
      }
    }

    public virtual void OnControlsGrabOn(ControllerInteractionEventArgs e)
    {
      if (ControlsGrabOn != null)
      {
        ControlsGrabOn(this, e);
      }
    }

    public virtual void OnControlsGrabOff(ControllerInteractionEventArgs e)
    {
      if (ControlsGrabOff != null)
      {
        ControlsGrabOff(this, e);
      }
    }

    public virtual void OnControlsUseOn(ControllerInteractionEventArgs e)
    {
      if (ControlsUseOn != null)
      {
        ControlsUseOn(this, e);
      }
    }

    public virtual void OnControlsUseOff(ControllerInteractionEventArgs e)
    {
      if (ControlsUseOff != null)
      {
        ControlsUseOff(this, e);
      }
    }

    public virtual void OnControlsUIOn(ControllerInteractionEventArgs e)
    {
      if (ControlsUIOn != null)
      {
        ControlsUIOn(this, e);
      }
    }

    public virtual void OnControlsUIOff(ControllerInteractionEventArgs e)
    {
      if (ControlsUIOff != null)
      {
        ControlsUIOff(this, e);
      }
    }

    public virtual void OnControlsMenuOn(ControllerInteractionEventArgs e)
    {
      if (ControlsMenuOn != null)
      {
        ControlsMenuOn(this, e);
      }
    }

    public virtual void OnControlsMenuOff(ControllerInteractionEventArgs e)
    {
      if (ControlsMenuOff != null)
      {
        ControlsMenuOff(this, e);
      }
    }

    public virtual void OnControllerEnabled(ControllerInteractionEventArgs e)
    {
      if (ControllerEnabled != null)
      {
        ControllerEnabled(this, e);
      }
    }

    public virtual void OnControllerDisabled(ControllerInteractionEventArgs e)
    {
      if (ControllerDisabled != null)
      {
        ControllerDisabled(this, e);
      }
    }

    public virtual void OnControllerIndexChanged(ControllerInteractionEventArgs e)
    {
      if (ControllerIndexChanged != null)
      {
        ControllerIndexChanged(this, e);
      }
    }

    public virtual void OnControllerVisible(ControllerInteractionEventArgs e)
    {
      controllerVisible = true;
      if (ControllerVisible != null)
      {
        ControllerVisible(this, e);
      }
    }

    public virtual void OnControllerHidden(ControllerInteractionEventArgs e)
    {
      controllerVisible = false;
      if (ControllerHidden != null)
      {
        ControllerHidden(this, e);
      }
    } 
    #endregion


    #region Public Methods
    /// <summary>
		/// The SetControllerEvent/0 method is used to set the Controller Event payload.
		/// </summary>
		/// <returns>The payload for a Controller Event.</returns>
		public virtual ControllerInteractionEventArgs SetControllerEvent()
		{
				var nullBool = false;
				return SetControllerEvent(ref nullBool);
		}
    /// <summary>
		/// The SetControllerEvent/3 method is used to set the Controller Event payload.
		/// </summary>
		/// <param name="buttonBool">The state of the pressed button if required.</param>
		/// <param name="value">The value to set the buttonBool reference to.</param>
		/// <param name="buttonPressure">The pressure of the button pressed if required.</param>
		/// <returns>The payload for a Controller Event.</returns>
		public virtual ControllerInteractionEventArgs SetControllerEvent(ref bool buttonBool, bool value = false, float buttonPressure = 0f)
		{
      VRTK_ControllerReference controllerReference = VRTK_ControllerReference.GetControllerReference(gameObject);
      buttonBool = value;
      ControllerInteractionEventArgs e;
      e.controllerReference = controllerReference;
      e.buttonPressure = buttonPressure;
      e.touchpadAxis = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.Touchpad, controllerReference);
      e.touchpadAngle = CalculateTouchpadAxisAngle(e.touchpadAxis);
      return e;
		}
    /// <summary>
    /// The GetTouchpadAxis method returns the coordinates of where the touchpad is being touched and can be used for directional input via the touchpad. The `x` value is the horizontal touch plane and the `y` value is the vertical touch plane.
    /// </summary>
    /// <returns>A 2 dimensional vector containing the x and y position of where the touchpad is being touched. `(0,0)` to `(1,1)`.</returns>
    public virtual Vector2 GetTouchpadAxis()
    {
      return touchpadAxis;
    }

    /// <summary>
    /// The GetTouchpadAxisAngle method returns the angle of where the touchpad is currently being touched with the top of the touchpad being 0 degrees and the bottom of the touchpad being 180 degrees.
    /// </summary>
    /// <returns>A float representing the angle of where the touchpad is being touched. `0f` to `360f`.</returns>
    public virtual float GetTouchpadAxisAngle()
    {
      return CalculateTouchpadAxisAngle(touchpadAxis);
    }

    /// <summary>
    /// The GetTriggerAxis method returns a float that represents how much the trigger is being squeezed. This can be useful for using the trigger axis to perform high fidelity tasks or only activating the trigger press once it has exceeded a given press threshold.
    /// </summary>
    /// <returns>A float representing the amount of squeeze that is being applied to the trigger. `0f` to `1f`.</returns>
    public virtual float GetTriggerAxis()
    {
      return triggerAxis.x;
    }

    /// <summary>
    /// The GetGripAxis method returns a float that represents how much the grip is being squeezed. This can be useful for using the grip axis to perform high fidelity tasks or only activating the grip press once it has exceeded a given press threshold.
    /// </summary>
    /// <returns>A float representing the amount of squeeze that is being applied to the grip. `0f` to `1f`.</returns>
    public virtual float GetGripAxis()
    {
      return gripAxis.x;
    }

    /// <summary>
    /// The GetHairTriggerDelta method returns a float representing the difference in how much the trigger is being pressed in relation to the hairline threshold start.
    /// </summary>
    /// <returns>A float representing the difference in the trigger pressure from the hairline threshold start to current position.</returns>
    public virtual float GetHairTriggerDelta()
    {
      return hairTriggerDelta;
    }

    /// <summary>
    /// The GetHairTriggerDelta method returns a float representing the difference in how much the trigger is being pressed in relation to the hairline threshold start.
    /// </summary>
    /// <returns>A float representing the difference in the trigger pressure from the hairline threshold start to current position.</returns>
    public virtual float GetHairGripDelta()
    {
      return hairGripDelta;
    }

    /// <summary>
    /// The AnyButtonPressed method returns true if any of the controller buttons are being pressed and this can be useful to determine if an action can be taken whilst the user is using the controller.
    /// </summary>
    /// <returns>Is true if any of the controller buttons are currently being pressed.</returns>
    public virtual bool AnyButtonPressed()
    {
      return (triggerPressed || gripPressed || touchpadPressed || buttonOnePressed || buttonTwoPressed || startMenuPressed);
    }

    /// <summary>
    /// The IsButtonPressed method takes a given button alias and returns a boolean whether that given button is currently being pressed or not.
    /// </summary>
    /// <param name="button">The button to check if it's being pressed.</param>
    /// <returns>Is true if the button is being pressed.</returns>
    public virtual bool IsButtonPressed(ButtonTypes button)
    {
      switch (button)
      {
        case ButtonTypes.TriggerTouch:
          return triggerTouched;
        case ButtonTypes.TriggerPress:
          return triggerPressed;
        case ButtonTypes.GripTouch:
          return gripTouched;
        case ButtonTypes.GripPress:
          return gripPressed;
        case ButtonTypes.TouchpadTouch:
          return touchpadTouched;
        case ButtonTypes.TouchpadPress:
          return touchpadPressed;
        case ButtonTypes.ButtonOnePress:
          return buttonOnePressed;
        case ButtonTypes.ButtonOneTouch:
          return buttonOneTouched;
        case ButtonTypes.ButtonTwoPress:
          return buttonTwoPressed;
        case ButtonTypes.ButtonTwoTouch:
          return buttonTwoTouched;
        case ButtonTypes.StartMenuPress:
          return startMenuPressed;
      }
      return false;
    }

    /// <summary>
    /// The SubscribeToButtonAliasEvent method makes it easier to subscribe to a button event on either the start or end action. Upon the event firing, the given callback method is executed.
    /// </summary>
    /// <param name="givenButton">The ButtonAlias to register the event on.</param>
    /// <param name="startEvent">If this is `true` then the start event related to the button is used (e.g. OnPress). If this is `false` then the end event related to the button is used (e.g. OnRelease). </param>
    /// <param name="callbackMethod">The method to subscribe to the event.</param>
    public virtual void SubscribeToButtonAliasEvent(ButtonTypes givenButton, bool startEvent, ControllerInteractionEventHandler callbackMethod)
    {
        ButtonAliasEventSubscription(true, givenButton, startEvent, callbackMethod);
    }

    /// <summary>
    /// The UnsubscribeToButtonAliasEvent method makes it easier to unsubscribe to from button event on either the start or end action.
    /// </summary>
    /// <param name="givenButton">The ButtonAlias to unregister the event on.</param>
    /// <param name="startEvent">If this is `true` then the start event related to the button is used (e.g. OnPress). If this is `false` then the end event related to the button is used (e.g. OnRelease). </param>
    /// <param name="callbackMethod">The method to unsubscribe from the event.</param>
    public virtual void UnsubscribeToButtonAliasEvent(ButtonTypes givenButton, bool startEvent, ControllerInteractionEventHandler callbackMethod)
    {
        ButtonAliasEventSubscription(false, givenButton, startEvent, callbackMethod);
    }
    #endregion


		#region Initilization 
		protected virtual void Awake()
		{
			VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
		}
    protected virtual void Start()
    {
      switch (VRTK_SDKManager.instance.loadedSetup.name)
      {
        case "OculusVR":
          currentPlatform = PlatformMode.Oculus;
          break;
        case "SteamVR":
          currentPlatform = PlatformMode.Vive;
          break;
        default:
          currentPlatform = PlatformMode.Undefined;
          break;
      }
    }
		protected virtual void OnEnable()
		{
			var actualController = VRTK_DeviceFinder.GetActualController(gameObject);
			if (actualController)
			{
				var controllerTracker = actualController.GetComponent<VRTK_TrackedController>();
				if (controllerTracker)
				{
					controllerTracker.ControllerEnabled += TrackedControllerEnabled;
					controllerTracker.ControllerDisabled += TrackedControllerDisabled;
					controllerTracker.ControllerIndexChanged += TrackedControllerIndexChanged;
				}
			}
		}
		#endregion

		#region DeInitilization		
    protected virtual void OnDisable()
		{
			Invoke("DisableEvents", 0f);
			var actualController = VRTK_DeviceFinder.GetActualController(gameObject);
			if (actualController)
			{
				var controllerTracker = actualController.GetComponent<VRTK_TrackedController>();
				if (controllerTracker)
				{
					controllerTracker.ControllerEnabled -= TrackedControllerEnabled;
					controllerTracker.ControllerDisabled -= TrackedControllerDisabled;
				}
			}
		}

		protected virtual void OnDestroy()
		{
			VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
		}
		protected virtual void DisableEvents()
		{
			if (triggerPressed)
			{
				OnTriggerReleased(SetControllerEvent(ref triggerPressed, false, 0f));
			}

			if (triggerTouched)
			{
				OnTriggerTouchEnd(SetControllerEvent(ref triggerTouched, false, 0f));
			}

			if (triggerHairlinePressed)
			{
				OnTriggerHairlineEnd(SetControllerEvent(ref triggerHairlinePressed, false, 0f));
			}

			if (triggerClicked)
			{
				OnTriggerUnclicked(SetControllerEvent(ref triggerClicked, false, 0f));
			}

			if (gripPressed)
			{
				OnGripReleased(SetControllerEvent(ref gripPressed, false, 0f));
			}

			if (gripTouched)
			{ 
				OnGripTouchEnd(SetControllerEvent(ref gripTouched, false, 0f));
			}

			if (gripHairlinePressed)
			{
				OnGripHairlineEnd(SetControllerEvent(ref gripHairlinePressed, false, 0f));
			}

			if (gripClicked)
			{
				OnGripUnclicked(SetControllerEvent(ref gripClicked, false, 0f));
			}

			if (touchpadPressed)
			{
				OnTouchpadReleased(SetControllerEvent(ref touchpadPressed, false, 0f));
			}

			if (touchpadTouched)
			{
				OnTouchpadTouchEnd(SetControllerEvent(ref touchpadTouched, false, 0f));
			}

			if (buttonOnePressed)
			{
				OnButtonOneReleased(SetControllerEvent(ref buttonOnePressed, false, 0f));
			}

			if (buttonOneTouched)
			{
				OnButtonOneTouchEnd(SetControllerEvent(ref buttonOneTouched, false, 0f));
			}

			if (buttonTwoPressed)
			{
				OnButtonTwoReleased(SetControllerEvent(ref buttonTwoPressed, false, 0f));
			}

			if (buttonTwoTouched)
			{
				OnButtonTwoTouchEnd(SetControllerEvent(ref buttonTwoTouched, false, 0f));
			}

			if (startMenuPressed)
			{
				OnStartMenuReleased(SetControllerEvent(ref startMenuPressed, false, 0f));
			}

			triggerAxisChanged = false;
			gripAxisChanged = false;
			touchpadAxisChanged = false;

			VRTK_ControllerReference controllerReference = VRTK_ControllerReference.GetControllerReference(gameObject);

			if (VRTK_ControllerReference.IsValid(controllerReference))
			{
        Vector2 currentTriggerAxis = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.Trigger, controllerReference);
        Vector2 currentGripAxis = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.Grip, controllerReference);
        Vector2 currentTouchpadAxis = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.Touchpad, controllerReference);

        // Save current touch and trigger settings to detect next change.
        touchpadAxis = new Vector2(currentTouchpadAxis.x, currentTouchpadAxis.y);
        triggerAxis = new Vector2(currentTriggerAxis.x, currentTriggerAxis.y);
        gripAxis = new Vector2(currentGripAxis.x, currentGripAxis.y);
        hairTriggerDelta = VRTK_SDK_Bridge.GetControllerHairlineDelta(SDK_BaseController.ButtonTypes.TriggerHairline, controllerReference);
        hairGripDelta = VRTK_SDK_Bridge.GetControllerHairlineDelta(SDK_BaseController.ButtonTypes.GripHairline, controllerReference);
			}
		}
		#endregion
		
		#region Core 

    protected virtual void Update()
    {
      VRTK_ControllerReference controllerReference = VRTK_ControllerReference.GetControllerReference(gameObject);

      //Only continue if the controller reference is valid
      if (!VRTK_ControllerReference.IsValid(controllerReference))
      {
        return;
      }

      Vector2 currentTriggerAxis = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.Trigger, controllerReference);
      Vector2 currentGripAxis = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.Grip, controllerReference);
      Vector2 currentTouchpadAxis = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.Touchpad, controllerReference);

      //Trigger Touched
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Trigger, SDK_BaseController.ButtonPressTypes.TouchDown, controllerReference))
      {
        OnTriggerTouchStart(SetControllerEvent(ref triggerTouched, true, currentTriggerAxis.x));
      }

      //Trigger Hairline
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.TriggerHairline, SDK_BaseController.ButtonPressTypes.PressDown, controllerReference))
      {
        OnTriggerHairlineStart(SetControllerEvent(ref triggerHairlinePressed, true, currentTriggerAxis.x));
      }

      //Trigger Pressed
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Trigger, SDK_BaseController.ButtonPressTypes.PressDown, controllerReference))
      {
        OnTriggerPressed(SetControllerEvent(ref triggerPressed, true, currentTriggerAxis.x));
      }

      //Trigger Clicked
      if (!triggerClicked && currentTriggerAxis.x >= triggerClickThreshold)
      {
        OnTriggerClicked(SetControllerEvent(ref triggerClicked, true, currentTriggerAxis.x));
      }
      else if (triggerClicked && currentTriggerAxis.x < triggerClickThreshold)
      {
        OnTriggerUnclicked(SetControllerEvent(ref triggerClicked, false, 0f));
      }

      // Trigger Pressed end
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Trigger, SDK_BaseController.ButtonPressTypes.PressUp, controllerReference))
      {
        OnTriggerReleased(SetControllerEvent(ref triggerPressed, false, 0f));
      }

      //Trigger Hairline End
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.TriggerHairline, SDK_BaseController.ButtonPressTypes.PressUp, controllerReference))
      {
        OnTriggerHairlineEnd(SetControllerEvent(ref triggerHairlinePressed, false, 0f));
      }

      //Trigger Touch End
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Trigger, SDK_BaseController.ButtonPressTypes.TouchUp, controllerReference))
      {
        OnTriggerTouchEnd(SetControllerEvent(ref triggerTouched, false, 0f));
      }

      //Trigger Axis
      currentTriggerAxis.x = ((!triggerTouched && triggerAxisZeroOnUntouch) || currentTriggerAxis.x < triggerForceZeroThreshold ? 0f : currentTriggerAxis.x);
      if (VRTK_SharedMethods.Vector2ShallowCompare(triggerAxis, currentTriggerAxis, axisFidelity))
      {
        triggerAxisChanged = false;
      }
      else
      {
        OnTriggerAxisChanged(SetControllerEvent(ref triggerAxisChanged, true, currentTriggerAxis.x));
      }

      //Grip Touched
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Grip, SDK_BaseController.ButtonPressTypes.TouchDown, controllerReference))
      {
        OnGripTouchStart(SetControllerEvent(ref gripTouched, true, currentGripAxis.x));
      }

      //Grip Hairline
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.GripHairline, SDK_BaseController.ButtonPressTypes.PressDown, controllerReference))
      {
        OnGripHairlineStart(SetControllerEvent(ref gripHairlinePressed, true, currentGripAxis.x));
      }

      //Grip Pressed
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Grip, SDK_BaseController.ButtonPressTypes.PressDown, controllerReference))
      {
        OnGripPressed(SetControllerEvent(ref gripPressed, true, currentGripAxis.x));
      }

      //Grip Clicked
      if (!gripClicked && currentGripAxis.x >= gripClickThreshold)
      {
        OnGripClicked(SetControllerEvent(ref gripClicked, true, currentGripAxis.x));
      }
      else if (gripClicked && currentGripAxis.x < gripClickThreshold)
      {
        OnGripUnclicked(SetControllerEvent(ref gripClicked, false, 0f));
      }

      // Grip Pressed End
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Grip, SDK_BaseController.ButtonPressTypes.PressUp, controllerReference))
      {
        OnGripReleased(SetControllerEvent(ref gripPressed, false, 0f));
      }

      //Grip Hairline End
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.GripHairline, SDK_BaseController.ButtonPressTypes.PressUp, controllerReference))
      {
        OnGripHairlineEnd(SetControllerEvent(ref gripHairlinePressed, false, 0f));
      }

      // Grip Touch End
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Grip, SDK_BaseController.ButtonPressTypes.TouchUp, controllerReference))
      {
        OnGripTouchEnd(SetControllerEvent(ref gripTouched, false, 0f));
      }

      //Grip Axis
      currentGripAxis.x = ((!gripTouched && gripAxisZeroOnUntouch) || currentGripAxis.x < gripForceZeroThreshold ? 0f : currentGripAxis.x);
      if (VRTK_SharedMethods.Vector2ShallowCompare(gripAxis, currentGripAxis, axisFidelity))
      {
        gripAxisChanged = false;
      }
      else
      {
        OnGripAxisChanged(SetControllerEvent(ref gripAxisChanged, true, currentGripAxis.x));
      }

      //Touchpad Touched
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Touchpad, SDK_BaseController.ButtonPressTypes.TouchDown, controllerReference))
      {
        OnTouchpadTouchStart(SetControllerEvent(ref touchpadTouched, true, 1f));
      }

      //Touchpad Pressed
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Touchpad, SDK_BaseController.ButtonPressTypes.PressDown, controllerReference))
      {
        OnTouchpadPressed(SetControllerEvent(ref touchpadPressed, true, 1f));
      }
      else if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Touchpad, SDK_BaseController.ButtonPressTypes.PressUp, controllerReference))
      {
        OnTouchpadReleased(SetControllerEvent(ref touchpadPressed, false, 0f));
      }

      //Touchpad Untouched
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.Touchpad, SDK_BaseController.ButtonPressTypes.TouchUp, controllerReference))
      {
        OnTouchpadTouchEnd(SetControllerEvent(ref touchpadTouched, false, 0f));
        touchpadAxis = Vector2.zero;
      }

      //Touchpad Axis
      if (VRTK_SDK_Bridge.IsTouchpadStatic(touchpadTouched, touchpadAxis, currentTouchpadAxis, axisFidelity))
      {
        touchpadAxisChanged = false;
      }
      else
      {
        OnTouchpadAxisChanged(SetControllerEvent(ref touchpadAxisChanged, true, 1f));
      }

      //ButtonOne Touched
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.ButtonOne, SDK_BaseController.ButtonPressTypes.TouchDown, controllerReference))
      {
        OnButtonOneTouchStart(SetControllerEvent(ref buttonOneTouched, true, 1f));
      }

      //ButtonOne Pressed
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.ButtonOne, SDK_BaseController.ButtonPressTypes.PressDown, controllerReference))
      {
        OnButtonOnePressed(SetControllerEvent(ref buttonOnePressed, true, 1f));
      }
      else if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.ButtonOne, SDK_BaseController.ButtonPressTypes.PressUp, controllerReference))
      {
        OnButtonOneReleased(SetControllerEvent(ref buttonOnePressed, false, 0f));
      }

      //ButtonOne Touched End
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.ButtonOne, SDK_BaseController.ButtonPressTypes.TouchUp, controllerReference))
      {
        OnButtonOneTouchEnd(SetControllerEvent(ref buttonOneTouched, false, 0f));
      }

      //ButtonTwo Touched
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.ButtonTwo, SDK_BaseController.ButtonPressTypes.TouchDown, controllerReference))
      {
        OnButtonTwoTouchStart(SetControllerEvent(ref buttonTwoTouched, true, 1f));
      }

      //ButtonTwo Pressed
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.ButtonTwo, SDK_BaseController.ButtonPressTypes.PressDown, controllerReference))
      {
        OnButtonTwoPressed(SetControllerEvent(ref buttonTwoPressed, true, 1f));
      }
      else if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.ButtonTwo, SDK_BaseController.ButtonPressTypes.PressUp, controllerReference))
      {
        OnButtonTwoReleased(SetControllerEvent(ref buttonTwoPressed, false, 0f));
      }

      //ButtonTwo Touched End
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.ButtonTwo, SDK_BaseController.ButtonPressTypes.TouchUp, controllerReference))
      {
        OnButtonTwoTouchEnd(SetControllerEvent(ref buttonTwoTouched, false, 0f));
      }

      //StartMenu Pressed
      if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.StartMenu, SDK_BaseController.ButtonPressTypes.PressDown, controllerReference))
      {
        OnStartMenuPressed(SetControllerEvent(ref startMenuPressed, true, 1f));
      }
      else if (VRTK_SDK_Bridge.GetControllerButtonState(SDK_BaseController.ButtonTypes.StartMenu, SDK_BaseController.ButtonPressTypes.PressUp, controllerReference))
      {
        OnStartMenuReleased(SetControllerEvent(ref startMenuPressed, false, 0f));
      }

      // Save current touch and trigger settings to detect next change.
      touchpadAxis = (touchpadAxisChanged ? new Vector2(currentTouchpadAxis.x, currentTouchpadAxis.y) : touchpadAxis);
      triggerAxis = (triggerAxisChanged ? new Vector2(currentTriggerAxis.x, currentTriggerAxis.y) : triggerAxis);
      gripAxis = (gripAxisChanged ? new Vector2(currentGripAxis.x, currentGripAxis.y) : gripAxis);

      hairTriggerDelta = VRTK_SDK_Bridge.GetControllerHairlineDelta(SDK_BaseController.ButtonTypes.TriggerHairline, controllerReference);
      hairGripDelta = VRTK_SDK_Bridge.GetControllerHairlineDelta(SDK_BaseController.ButtonTypes.GripHairline, controllerReference);
    }
		#endregion

    #region Controller Methods 
    protected virtual void ButtonAliasEventSubscription(bool subscribe, ButtonTypes givenButton, bool startEvent, ControllerInteractionEventHandler callbackMethod)
    {
      switch (givenButton)
      {
        case ButtonTypes.TriggerPress:
          if (subscribe)
          {
            if (startEvent)
            {
              TriggerPressed += callbackMethod;
            }
            else
            {
              TriggerReleased += callbackMethod;
            }
          }
          else
          {
            if (startEvent)
            {
              TriggerPressed -= callbackMethod;
            }
            else
            {
              TriggerReleased -= callbackMethod;
            }
          }
          break;
        case ButtonTypes.TriggerTouch:
          if (subscribe)
          {
            if (startEvent)
            {
              TriggerTouchStart += callbackMethod;
            }
            else
            {
              TriggerTouchEnd += callbackMethod;
            }
          }
          else
          {
            if (startEvent)
            {
              TriggerTouchStart -= callbackMethod;
            }
            else
            {
              TriggerTouchEnd -= callbackMethod;
            }
          }
          break;
        case ButtonTypes.GripPress:
          if (subscribe)
          {
            if (startEvent)
            {
              GripPressed += callbackMethod;
            }
            else
            {
              GripReleased += callbackMethod;
            }
          }
          else
          {
            if (startEvent)
            {
              GripPressed -= callbackMethod;
            }
            else
            {
              GripReleased -= callbackMethod;
            }
          }
          break;
        case ButtonTypes.GripTouch:
          if (subscribe)
          {
            if (startEvent)
            {
              GripTouchStart += callbackMethod;
            }
            else
            {
              GripTouchEnd += callbackMethod;
            }
          }
          else
          {
            if (startEvent)
            {
              GripTouchStart -= callbackMethod;
            }
            else
            {
              GripTouchEnd -= callbackMethod;
            }
          }
          break;
        case ButtonTypes.TouchpadPress:
          if (subscribe)
          {
            if (startEvent)
            {
              TouchpadPressed += callbackMethod;
            }
            else
            {
              TouchpadReleased += callbackMethod;
            }
          }
          else
          {
            if (startEvent)
            {
              TouchpadPressed -= callbackMethod;
            }
            else
            {
              TouchpadReleased -= callbackMethod;
            }
          }
          break;
        case ButtonTypes.TouchpadTouch:
          if (subscribe)
          {
            if (startEvent)
            {
              TouchpadTouchStart += callbackMethod;
            }
            else
            {
              TouchpadTouchEnd += callbackMethod;
            }
          }
          else
          {
            if (startEvent)
            {
              TouchpadTouchStart -= callbackMethod;
            }
            else
            {
              TouchpadTouchEnd -= callbackMethod;
            }
          }
          break;
        case ButtonTypes.ButtonOnePress:
          if (subscribe)
          {
            if (startEvent)
            {
              ButtonOnePressed += callbackMethod;
            }
            else
            {
              ButtonOneReleased += callbackMethod;
            }
          }
          else
          {
            if (startEvent)
            {
              ButtonOnePressed -= callbackMethod;
            }
            else
            {
              ButtonOneReleased -= callbackMethod;
            }
          }
          break;
        case ButtonTypes.ButtonOneTouch:
          if (subscribe)
          {
            if (startEvent)
            {
              ButtonOneTouchStart += callbackMethod;
            }
            else
            {
              ButtonOneTouchEnd += callbackMethod;
            }
          }
          else
          {
            if (startEvent)
            {
              ButtonOneTouchStart -= callbackMethod;
            }
            else
            {
              ButtonOneTouchEnd -= callbackMethod;
            }
          }
          break;
        case ButtonTypes.ButtonTwoPress:
          if (subscribe)
          {
            if (startEvent)
            {
              ButtonTwoPressed += callbackMethod;
            }
            else
            {
              ButtonTwoReleased += callbackMethod;
            }
          }
          else
          {
            if (startEvent)
            {
              ButtonTwoPressed -= callbackMethod;
            }
            else
            {
              ButtonTwoReleased -= callbackMethod;
            }
          }
          break;
        case ButtonTypes.ButtonTwoTouch:
          if (subscribe)
          {
            if (startEvent)
            {
              ButtonTwoTouchStart += callbackMethod;
            }
            else
            {
              ButtonTwoTouchEnd += callbackMethod;
            }
          }
          else
          {
            if (startEvent)
            {
              ButtonTwoTouchStart -= callbackMethod;
            }
            else
            {
              ButtonTwoTouchEnd -= callbackMethod;
            }
          }
          break;
        case ButtonTypes.StartMenuPress:
        if (subscribe)
        {
          if (startEvent)
          {
            StartMenuPressed += callbackMethod;
          }
          else
          {
            StartMenuReleased += callbackMethod;
          }
        }
        else
        {
          if (startEvent)
          {
            StartMenuPressed -= callbackMethod;
          }
          else
          {
            StartMenuReleased -= callbackMethod;
          }
        }
        break;
      }
    }
    #endregion

    
    #region Listeners
    protected virtual void TrackedControllerEnabled(object sender, VRTKTrackedControllerEventArgs e)
    {
      OnControllerEnabled(SetControllerEvent());
    }

    protected virtual void TrackedControllerDisabled(object sender, VRTKTrackedControllerEventArgs e)
    {
      DisableEvents(); 
      OnControllerDisabled(SetControllerEvent());
    }

    protected virtual void TrackedControllerIndexChanged(object sender, VRTKTrackedControllerEventArgs e)
    {
      OnControllerIndexChanged(SetControllerEvent());
    }
    #endregion

    #region Helpers
    protected virtual float CalculateTouchpadAxisAngle(Vector2 axis)
    {
      float angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg;
      angle = 90.0f - angle;
      if (angle < 0)
      {
          angle += 360.0f;
      }
      return angle;
    }



    #endregion
  
  }
}
