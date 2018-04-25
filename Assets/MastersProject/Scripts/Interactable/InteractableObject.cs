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
	/// Event Payload
	/// </summary>
	/// <param name="interactable">The Interactable that is initiating the interaction (e.g. a controller).</param>
	public struct InteractableEventArgs { public GameObject interactable; }

	/// <summary>
	/// Event Payload
	/// </summary>
	/// <param name="sender">this object</param>
	/// <param name="e"><see cref="InteractableEventArgs"/></param>
	public delegate void InteractableEventHandler(object sender, InteractableEventArgs e);

	/// <summary>
  /// Basically the VRTK class re-written. I found myself getting frustrated with VRTK's code complexity, and also found myself 
  /// trying to change it often enough, I felt it would be easier to rewrite it so I at least understood how it worked
  /// </summary>
	public class InteractableObject : MonoBehaviour 
	{
		#region Config
		[Tooltip("If this is checked then the interactable Interactable script will be disabled when the Interactable is not being interacted with. This will eliminate the potential number of calls the interactable Interactables make each frame.")]
		public bool disableWhenIdle = true;

		[Header("Touch Options")]
		[Tooltip("The colour to highlight the Interactable when it is touched. This colour will override any globally set colour (for instance on the `VRTK_InteractTouch` script).")]
		public Color touchHighlightColor = Color.clear;

		[Tooltip("An array of colliders on the Interactable to ignore when being touched.")]
		public Collider[] ignoredColliders;

		[Header("Grab Options")]
		[Tooltip("Determines if the Interactable can be grabbed.")]
		public bool isGrabbable = false;

		[Tooltip("This determines how the grabbed item will be attached to the controller when it is grabbed. If one isn't provided then the first Grab Attach script on the GameObject will be used, if one is not found and the Interactable is grabbable then a Fixed Joint Grab Attach script will be created at runtime.")]
		public InteractableGrabBase grabAttachMechanicScript;

		[Header("Use Options")]
		[Tooltip("Determines if the Interactable can be used.")]
		public bool isUsable = false;

    [Tooltip("Determines if use button must be held while using")]
		public bool holdButtonToUse = false;

		[Tooltip("Determines what button is used to use the Interactable")]
		public ControllerEvents.ButtonTypes useButton = ControllerEvents.ButtonTypes.Undefined;
		#endregion


		#region State
		/// <summary>
		/// The current using state of the Interactable. `0` not being used, `1` being used.
		/// </summary>
		[HideInInspector] public int usingState = 0;

		public bool isKinematic {
      get {
        if (rigidbody) {
            return rigidbody.isKinematic;
        }
        return true;
      }
      set {
        if (rigidbody) {
          rigidbody.isKinematic = value;
        }
      }
		}
		protected List<GameObject> touchingControllers = new List<GameObject>();
		protected GameObject grabbingController = null;
    protected GameObject usingController = null;

		#endregion

		#region Bookkeeping 
		protected new Rigidbody rigidbody;
		protected Transform previousParent;
		protected bool previousKinematicState;
		protected bool previousIsGrabbable;
		protected bool forcedDropped;
		protected bool forceDisabled;
		protected InteractableHighlightBase objectHighlighter;
		protected bool autoHighlighter = false;
		protected bool hoveredOverSnapDropZone = false;
		protected bool snappedInSnapDropZone = false;
		protected Vector3 previousLocalScale = Vector3.zero;
		protected List<GameObject> currentIgnoredColliders = new List<GameObject>();
		protected bool startDisabled = false;
		#endregion

		#region Delegates
		/// <summary>
		/// Emitted when another Interactable touches the current Interactable.
		/// </summary>
		public event InteractableEventHandler InteractableTouched;
		/// <summary>
		/// Emitted when the other Interactable stops touching the current Interactable.
		/// </summary>
		public event InteractableEventHandler InteractableUntouched;
		/// <summary>
		/// Emitted when another Interactable grabs the current Interactable (e.g. a controller).
		/// </summary>
		public event InteractableEventHandler InteractableGrabbed;
		/// <summary>
		/// Emitted when the other Interactable stops grabbing the current Interactable.
		/// </summary>
		public event InteractableEventHandler InteractableUngrabbed;
		/// <summary>
		/// Emitted when another Interactable uses the current Interactable (e.g. a controller).
		/// </summary>
		public event InteractableEventHandler InteractableUsed;
		/// <summary>
		/// Emitted when the other Interactable stops using the current Interactable.
		/// </summary>
		public event InteractableEventHandler InteractableUnused;
		/// <summary>
		/// Emitted when the Interactable enters a snap drop zone.
		/// </summary>
		public event InteractableEventHandler InteractableEnteredSnapDropZone;
		/// <summary>
		/// Emitted when the Interactable exists a snap drop zone.
		/// </summary>
		public event InteractableEventHandler InteractableExitedSnapDropZone;
		/// <summary>
		/// Emitted when the Interactable gets snapped to a drop zone.
		/// </summary>
		public event InteractableEventHandler InteractableSnappedToDropZone;
		/// <summary>
		/// Emitted when the Interactable gets unsnapped from a drop zone.
		/// </summary>
		public event InteractableEventHandler InteractableUnsnappedFromDropZone;
		public virtual void OnInteractableTouched(InteractableEventArgs e)
		{
			if (InteractableTouched != null)
			{
				InteractableTouched(this, e);
			}
		}

		public virtual void OnInteractableUntouched(InteractableEventArgs e)
		{
			if (InteractableUntouched != null)
			{
				InteractableUntouched(this, e);
			}
		}

		public virtual void OnInteractableGrabbed(InteractableEventArgs e)
		{
      if (InteractableGrabbed != null)
      {
        InteractableGrabbed(this, e);
      }
		}

		public virtual void OnInteractableUngrabbed(InteractableEventArgs e)
		{
      if (InteractableUngrabbed != null)
      {
        InteractableUngrabbed(this, e);
      }
		}

		public virtual void OnInteractableUsed(InteractableEventArgs e)
		{
      if (InteractableUsed != null)
      {
        InteractableUsed(this, e);
      }
		}

		public virtual void OnInteractableUnused(InteractableEventArgs e)
		{
      if (InteractableUnused != null)
      {
        InteractableUnused(this, e);
      }
		}

		public virtual void OnInteractableEnteredSnapDropZone(InteractableEventArgs e)
		{
      if (InteractableEnteredSnapDropZone != null)
      {
        InteractableEnteredSnapDropZone(this, e);
      }
		}

		public virtual void OnInteractableExitedSnapDropZone(InteractableEventArgs e)
		{
      if (InteractableExitedSnapDropZone != null)
      {
        InteractableExitedSnapDropZone(this, e);
      }
		}

		public virtual void OnInteractableSnappedToDropZone(InteractableEventArgs e)
		{
      if (InteractableSnappedToDropZone != null)
      {
        InteractableSnappedToDropZone(this, e);
      }
		}

		public virtual void OnInteractableUnsnappedFromDropZone(InteractableEventArgs e)
		{
      if (InteractableUnsnappedFromDropZone != null)
      {
        InteractableUnsnappedFromDropZone(this, e);
      }
		}

		public InteractableEventArgs SetInteractableEvent(GameObject interactable)
		{
      InteractableEventArgs e;
      e.interactable = interactable;
      return e;
		}
		#endregion

    #region Public Methods
    /// <summary>
    /// The IsTouched method is used to determine if the object is currently being touched.
    /// </summary>
    /// <returns>Returns `true` if the object is currently being touched.</returns>
    public virtual bool IsTouched() { return (touchingControllers.Count > 0); }

    /// <summary>
    /// The IsGrabbed method is used to determine if the object is currently being grabbed.
    /// </summary>
    /// <returns>Returns `true` if the object is currently being grabbed.</returns>
    public virtual bool IsGrabbed() { return (grabbingController != null); }

    /// <summary>
    /// The IsUsing method is used to determine if the object is currently being used.
    /// </summary>
    /// <returns>Returns `true` if the object is currently being used.</returns>
    public virtual bool IsUsing() { return (usingController != null); }

    /// <summary>
    /// determines if this object is currently idle
    /// used to determine whether or not the script
    /// can be disabled for now
    /// </summary>
    /// <returns>whether or not the script is currently idle</returns>
    protected virtual bool IsIdle() { return !IsTouched() && !IsGrabbed() && !IsUsing(); }

    /// <summary>
    /// The StartTouching method is called automatically when the object is touched initially. It is also a virtual method to allow for overriding in inherited classes.
    /// </summary>
    /// <param name="currentTouchingObject">The object that is currently touching this object.</param>
    public virtual void StartTouching(ControllerTouch givenTouchingController) {
      if (givenTouchingController != null) {
        IgnoreColliders(givenTouchingController.gameObject);
        if (!touchingControllers.Contains(givenTouchingController.gameObject)) {
          ToggleEnableState(true);
          touchingControllers.Add(givenTouchingController.gameObject);
          OnInteractableTouched(SetInteractableEvent(givenTouchingController.gameObject));
        }
      }
    }

    /// <summary>
    /// The StopTouching method is called automatically when the object has stopped being touched. It is also a virtual method to allow for overriding in inherited classes.
    /// </summary>
    /// <param name="formerTouchingObject">The object that was previously touching this object.</param>
    public virtual void StopTouching(ControllerTouch formerTouchingController) {
      if (formerTouchingController != null && touchingControllers.Contains(formerTouchingController.gameObject)) {
        ResetUseState(formerTouchingController.gameObject);
        OnInteractableUntouched(SetInteractableEvent(formerTouchingController.gameObject));
        touchingControllers.Remove(formerTouchingController.gameObject);
      }
    }

    /// <summary>
    /// The Grabbed method is called automatically when the object is grabbed initially. It is also a virtual method to allow for overriding in inherited classes.
    /// </summary>
    /// <param name="currentGrabbingObject">The object that is currently grabbing this object.</param>
    public virtual void Grabbed(ControllerGrab givenGrabbingController, Rigidbody givenControllerAttachPoint) {
      ToggleEnableState(true);
      grabAttachMechanicScript.StartGrab(givenGrabbingController.gameObject, givenControllerAttachPoint);
      grabbingController = givenGrabbingController.gameObject;
      OnInteractableGrabbed(SetInteractableEvent(givenGrabbingController.gameObject));
    }

    /// <summary>
    /// The Ungrabbed method is called automatically when the object has stopped being grabbed. It is also a virtual method to allow for overriding in inherited classes.
    /// </summary>
    /// <param name="previousGrabbingObject">The object that was previously grabbing this object.</param>
    public virtual void Ungrabbed(ControllerGrab formerGrabbingController = null) {
      UnpauseCollisions();
      ResetUseState(formerGrabbingController.gameObject);
      grabbingController = null;
      LoadPreviousState();
      grabAttachMechanicScript.StopGrab(true);
      
      OnInteractableUngrabbed(SetInteractableEvent(formerGrabbingController.gameObject));
    }

    /// <summary>
    /// The StartUsing method is called automatically when the object is used initially. It is also a virtual method to allow for overriding in inherited classes.
    /// </summary>
    /// <param name="currentUsingObject">The object that is currently using this object.</param>
    public virtual void StartUsing(ControllerUse givenUsingController) {
      ToggleEnableState(true);
      if (IsUsing()) { ResetUsingObject(); }
      OnInteractableUsed(SetInteractableEvent(givenUsingController.gameObject));
      usingController = givenUsingController.gameObject;
    }

    /// <summary>
    /// The StopUsing method is called automatically when the object has stopped being used. It is also a virtual method to allow for overriding in inherited classes.
    /// </summary>
    /// <param name="previousUsingObject">The object that was previously using this object.</param>
    public virtual void StopUsing(ControllerUse formerUsingController = null) {
      OnInteractableUnused(SetInteractableEvent(formerUsingController.gameObject));
      ResetUsingObject();
      usingState = 0;
      usingController = null;
    }

    /// <summary>
    /// The ToggleHighlight method is used to turn on or off the colour highlight of the object.
    /// </summary>
    /// <param name="toggle">The state to determine whether to activate or deactivate the highlight. `true` will enable the highlight and `false` will remove the highlight.</param>
    public virtual void ToggleHighlight(bool toggle) {
      InitialiseHighlighter();
      if (touchHighlightColor != Color.clear && objectHighlighter) {
        if (toggle && !IsGrabbed()) { objectHighlighter.Highlight(touchHighlightColor); }
        else { objectHighlighter.Unhighlight(); }
      }
    }

    /// <summary>
    /// The ResetHighlighter method is used to reset the currently attached highlighter.
    /// </summary>
    public virtual void ResetHighlighter() {
      if (objectHighlighter) { objectHighlighter.ResetHighlighter(); }
    }

    /// <summary>
    /// The PauseCollisions method temporarily pauses all collisions on the object at grab time by removing the object's rigidbody's ability to detect collisions. This can be useful for preventing clipping when initially grabbing an item.
    /// </summary>
    /// <param name="delay">The amount of time to pause the collisions for.</param>
    public virtual void PauseCollisions(float delay) {
      if (delay > 0f) {
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>()) {
          rb.detectCollisions = false;
        }
        Invoke("UnpauseCollisions", delay);
      }
    }

    /// <summary>
    /// The ZeroVelocity method resets the velocity and angular velocity to zero on the rigidbody attached to the object.
    /// </summary>
    public virtual void ZeroVelocity() {
      if (rigidbody) {
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
      }
    }

    /// <summary>
    /// The SaveCurrentState method stores the existing object parent and the object's rigidbody kinematic setting.
    /// </summary>
    public virtual void SaveCurrentState() {
      if (!IsGrabbed()) {
        previousParent = transform.parent;
        if (rigidbody) {
          previousKinematicState = rigidbody.isKinematic;
        }
      }
    }

    /// <summary>
    /// The GetTouchingObjects method is used to return the collecetion of valid game objects that are currently touching this object.
    /// </summary>
    /// <returns>A list of game object of that are currently touching the current object.</returns>
    public virtual List<GameObject> GetTouchingc() { return touchingControllers; }

    /// <summary>
    /// The GetGrabbingObject method is used to return the game object that is currently grabbing this object.
    /// </summary>
    /// <returns>The game object of what is grabbing the current object.</returns>
    public virtual GameObject GetGrabbingObject() { return grabbingController; }

    /// <summary>
    /// The GetGrabbingObject method is used to return the game object that is currently grabbing this object.
    /// </summary>
    /// <returns>The game object of what is grabbing the current object.</returns>
    public virtual ControllerGrab GetGrabbingController() { return grabbingController.GetComponent<ControllerGrab>(); }

    /// <summary>
    /// The GetUsingObject method is used to return the GameObject that is currently using this object.
    /// </summary>
    /// <returns>The GameObject of what is using the current object.</returns>
    public virtual GameObject GetUsingObject() { return usingController; }

    /// <summary>
    /// The GetUsingScript method is used to return the InteractUse script that is currently using this object.
    /// </summary>
    /// <returns>The InteractUse script of the object that is using the current object.</returns>
    public virtual ControllerUse GetUsingController() { return usingController.GetComponent<ControllerUse>(); }

    /// <summary>
    /// The ForceStopInteracting method forces the object to no longer be interacted with and will cause a controller to drop the object and stop touching it. This is useful if the controller is required to auto interact with another object.
    /// </summary>
    public virtual void ForceStopInteracting()
    {
      if (gameObject.activeInHierarchy) {
        forceDisabled = false;
        StartCoroutine(ForceStopInteractingAtEndOfFrame());
      }

      if (!gameObject.activeInHierarchy && forceDisabled) {
        ForceStopAllInteractions();
        forceDisabled = false;
      }
    }

    /// <summary>
    /// the StoreLocalScale method saves the current transform local scale values.
    /// </summary>
    public virtual void StoreLocalScale() {
      previousLocalScale = transform.localScale;
    }

    /// <summary>
    /// The ResetIgnoredColliders method is used to clear any stored ignored colliders in case the `Ignored Colliders` array parameter is changed at runtime. This needs to be called manually if changes are made at runtime.
    /// </summary>
    public virtual void ResetIgnoredColliders() {
      //Go through all the existing set up ignored colliders and reset their collision state
      for (int x = 0; x < currentIgnoredColliders.Count; x++) {
        if (currentIgnoredColliders[x] != null) {
          Collider[] touchingColliders = currentIgnoredColliders[x].GetComponentsInChildren<Collider>();
          if (ignoredColliders != null) {
            for (int i = 0; i < ignoredColliders.Length; i++) {
              for (int j = 0; j < touchingColliders.Length; j++) {
                Physics.IgnoreCollision(touchingColliders[j], ignoredColliders[i], false);
              }
            }
          }
        }
      }
      currentIgnoredColliders.Clear();
    }
    #endregion
    //—————————
		#region Initilization 
		protected virtual void Awake() {
			rigidbody = GetComponent<Rigidbody>();
			if (rigidbody != null) {
				rigidbody.maxAngularVelocity = float.MaxValue;
			}

			if (disableWhenIdle && enabled && IsIdle()) {
				startDisabled = true;
				enabled = false;
			}
		}
		protected virtual void OnEnable() {
      InitialiseHighlighter();
      forceDisabled = false;
      if (forcedDropped) {
        LoadPreviousState();
      }
      forcedDropped = false;
      startDisabled = false;
		}
		#endregion
    //—————————
		#region Deinitilization
		protected virtual void OnDisable() {
      if (autoHighlighter) {
        Destroy(objectHighlighter);
        objectHighlighter = null;
      }

      if (!startDisabled) {
        forceDisabled = true;
        ForceStopInteracting();
      }
		}
		#endregion
    //—————————
		#region Core 
		protected virtual void FixedUpdate() {
      if (IsGrabbed() && grabAttachMechanicScript != null) {
        grabAttachMechanicScript.ProcessFixedUpdate();
      }
		}
		protected virtual void Update() {
      AttemptSetGrabMechanic();
      if (IsGrabbed() && grabAttachMechanicScript != null) {
        grabAttachMechanicScript.ProcessUpdate();
      }
		}
    protected virtual void LateUpdate() {
      if (disableWhenIdle && IsIdle()) {
        ToggleEnableState(false);
      }
    }
		#endregion
    //—————————
		#region Helper 
    protected virtual void LoadPreviousState() {
      if (gameObject.activeInHierarchy) {
        transform.SetParent(previousParent);
        forcedDropped = false;
      }
      if (rigidbody != null) {
        rigidbody.isKinematic = previousKinematicState;
      }
    }

    protected virtual void InitialiseHighlighter() {
      if (touchHighlightColor != Color.clear && objectHighlighter == null)
      {
        autoHighlighter = false;
        objectHighlighter = InteractableHighlightBase.GetActiveHighlighter(gameObject);
        if (objectHighlighter == null)
        {
          autoHighlighter = true;
          objectHighlighter = gameObject.AddComponent<InteractableHighlightColor>();
        }
        objectHighlighter.Initialise(touchHighlightColor);
      }
    }

    protected virtual void IgnoreColliders(GameObject touchingObject)
    {
      if (ignoredColliders != null && !currentIgnoredColliders.Contains(touchingObject))
      {
        bool objectIgnored = false;
        Collider[] touchingColliders = touchingObject.GetComponentsInChildren<Collider>();
        for (int i = 0; i < ignoredColliders.Length; i++)
        {
          for (int j = 0; j < touchingColliders.Length; j++)
          {
            Physics.IgnoreCollision(touchingColliders[j], ignoredColliders[i]);
            objectIgnored = true;
          }
        }

        if (objectIgnored)
        {
          currentIgnoredColliders.Add(touchingObject);
        }
      }
    }

    protected virtual void ToggleEnableState(bool state)
    {
      if (disableWhenIdle)
      {
        enabled = state;
      }
    }

    protected virtual void AttemptSetGrabMechanic() {
      if (isGrabbable && grabAttachMechanicScript == null) {
        InteractableGrabBase setGrabMechanic = GetComponent<InteractableGrabBase>();
        if (!setGrabMechanic) {
          setGrabMechanic = gameObject.AddComponent<InteractableGrabJoint>();
        }
        grabAttachMechanicScript = setGrabMechanic;
      }
    }

    protected virtual void ForceReleaseGrab() {
      if (grabbingController != null) {
        GetGrabbingController().ForceRelease();
      }
    }

    protected virtual void UnpauseCollisions() {
      Rigidbody[] childRigidbodies = GetComponentsInChildren<Rigidbody>();
      for (int i = 0; i < childRigidbodies.Length; i++)
      {
        childRigidbodies[i].detectCollisions = true;
      }
    }
    protected virtual void ResetUseState(GameObject checkObject)
    {
      if (checkObject != null)
      {
        ControllerUse usingObjectCheck = checkObject.GetComponent<ControllerUse>();
        if (usingObjectCheck != null)
        {
          if (holdButtonToUse)
          {
            usingObjectCheck.ForceStopUsing();
          }
        }
      }
    }

    protected virtual IEnumerator ForceStopInteractingAtEndOfFrame()
    {
      yield return new WaitForEndOfFrame();
      ForceStopAllInteractions();
    }

    protected virtual void ForceStopAllInteractions()
    {
      StopTouchingInteractions();
      StopGrabbingInteractions();
      StopUsingInteractions();
    }

    protected virtual void StopTouchingInteractions()
    {
      for (int i = 0; i < touchingControllers.Count; i++) {
        GameObject touchingObject = touchingControllers[i];
        if (touchingObject.activeInHierarchy || forceDisabled) {
          touchingObject.GetComponent<ControllerTouch>().ForceStopTouching();
        }
      }
    }

    protected virtual void StopGrabbingInteractions()
    {
      if (grabbingController != null && (grabbingController.gameObject.activeInHierarchy || forceDisabled)) {
        grabbingController.GetComponent<ControllerTouch>().ForceStopTouching();
        GetGrabbingController().ForceRelease();
        forcedDropped = true;
      }
    }

    protected virtual void StopUsingInteractions()
    {
      if (usingController != null && (usingController.gameObject.activeInHierarchy || forceDisabled))
      {
        usingController.GetComponent<ControllerTouch>().ForceStopTouching();
        GetUsingController().ForceStopUsing();
      }
    }

    protected virtual void ResetUsingObject()
    {
      if (usingController != null) {
        GetUsingController().ForceResetUsing();
      }
    }
    #endregion
	}
}
