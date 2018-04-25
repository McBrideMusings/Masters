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
	/// Event Payload
	/// </summary>
	/// <param name="controllerReference">The reference to the controller doing the interaction.</param>
	/// <param name="target">The GameObject of the interactable object that is being interacted with by the controller.</param>
	public struct ObjectInteractEventArgs
	{
		public VRTK_ControllerReference controllerReference;
		public GameObject target;
	}

	/// <summary>
	/// Event Payload
	/// </summary>
	/// <param name="sender">this object</param>
	/// <param name="e"><see cref="ObjectInteractEventArgs"/></param>
	public delegate void ObjectInteractEventHandler(object sender, ObjectInteractEventArgs e);

	/// <summary>
  /// class description
  /// </summary>
	public class ControllerTouch : MonoBehaviour 
	{
		#region Public Fields
		public bool invisibleOnTouch = false;
		public bool doHaptics = false;

		#endregion

		#region Private Fields
		#endregion

		#region Bookkeeping 
		[SerializeField] protected GameObject touchedObject;
		[SerializeField] protected List<Collider> touchedColliders = new List<Collider>();
		protected GameObject controllerCollisionDetector;
		protected bool triggerRumble;
		protected Rigidbody touchRigidBody;
		protected Object defaultColliderPrefab;
		protected VRTK_ControllerReference controllerReference
		{
			get
			{
				return VRTK_ControllerReference.GetControllerReference(gameObject);
			}
		}			
		#endregion
		//—————————
		#region Delegates 
		/// <summary>
		/// Emitted when the touch of a valid object has started.
		/// </summary>
		public event ObjectInteractEventHandler ControllerStartTouchInteractableObject;
		/// <summary>
		/// Emitted when a valid object is touched.
		/// </summary>
		public event ObjectInteractEventHandler ControllerTouchInteractableObject;
		/// <summary>
		/// Emitted when the untouch of a valid object has started.
		/// </summary>
		public event ObjectInteractEventHandler ControllerStartUntouchInteractableObject;
		/// <summary>
		/// Emitted when a valid object is no longer being touched.
		/// </summary>
		public event ObjectInteractEventHandler ControllerUntouchInteractableObject;
		/// <summary>
		/// Emitted when the controller rigidbody is activated.
		/// </summary>
		public event ObjectInteractEventHandler ControllerRigidbodyActivated;
		/// <summary>
		/// Emitted when the controller rigidbody is deactivated.
		/// </summary>
		public event ObjectInteractEventHandler ControllerRigidbodyDeactivated;

		public virtual void OnControllerStartTouchInteractableObject(ObjectInteractEventArgs e)
		{
				if (ControllerStartTouchInteractableObject != null)
				{
						ControllerStartTouchInteractableObject(this, e);
				}
		}

		public virtual void OnControllerTouchInteractableObject(ObjectInteractEventArgs e)
		{
			if (ControllerTouchInteractableObject != null)
			{
				ControllerTouchInteractableObject(this, e);
			}
		}

		public virtual void OnControllerStartUntouchInteractableObject(ObjectInteractEventArgs e)
		{
			if (ControllerStartUntouchInteractableObject != null)
			{
				ControllerStartUntouchInteractableObject(this, e);
			}
		}

		public virtual void OnControllerUntouchInteractableObject(ObjectInteractEventArgs e)
		{
			if (ControllerUntouchInteractableObject != null)
			{
				ControllerUntouchInteractableObject(this, e);
			}
		}

		public virtual void OnControllerRigidbodyActivated(ObjectInteractEventArgs e)
		{
			if (ControllerRigidbodyActivated != null)
			{
				ControllerRigidbodyActivated(this, e);
			}
		}

		public virtual void OnControllerRigidbodyDeactivated(ObjectInteractEventArgs e)
		{
			if (ControllerRigidbodyDeactivated != null)
			{
				ControllerRigidbodyDeactivated(this, e);
			}
		}
		public virtual ObjectInteractEventArgs SetControllerInteractEvent(GameObject target)
		{
			ObjectInteractEventArgs e;
			e.controllerReference = controllerReference;
			e.target = target;
			return e;
		}	
		#endregion
		//—————————
		#region Public Methods
		/// <summary>
		/// The GetTouchedObject method returns the closest touched object. GetTouchedObjectList returns whole list.
		/// </summary>
		/// <returns>The game object of the closest touched object by the controller.</returns>
		public virtual GameObject GetTouchedObject()
		{
			return touchedObject;
		}

		/// <summary>
		/// The GetTouchedObject method returns the closest touched object. GetTouchedObjectList returns whole list.
		/// </summary>
		/// <returns>The game object of the closest touched object by the controller.</returns>
		public virtual InteractableObject GetTouchedObjectScript()
		{
			return GetInteractableScript(touchedObject);
		}
		/// <summary>
		/// The IsObjectInteractable method is used to check if a given game object is of type `VRTK_InteractableObject` and whether the object is enabled.
		/// </summary>
		/// <param name="obj">The game object to check to see if it's interactable.</param>
		/// <returns>Is true if the given object is of type `VRTK_InteractableObject`.</returns>
		public virtual bool IsObjectInteractable(GameObject obj)
		{
			if (obj != null)
			{
				InteractableObject io = GetInteractableScript(obj);
				if (io != null)
				{
					if (io.disableWhenIdle && !io.enabled)
					{
							return true;
					}
					return io.enabled;
				}
			}
			return false;
		}
		/// <summary>
		/// The ForceStopTouching method will stop the controller from touching an object even if the controller is physically touching the object still.
		/// </summary>
		public virtual void ForceStopTouching()
		{
			if (touchedObject)
			{
				UnTouch(touchedObject, GetInteractableScript(touchedObject));
			}
		}
		/// <summary>
		/// The ForceTouch method will attempt to force the controller to touch the given game object. This is useful if an object that isn't being touched is required to be grabbed or used as the controller doesn't physically have to be touching it to be forced to interact with it.
		/// </summary>
		/// <param name="obj">The game object to attempt to force touch.</param>
		public virtual void ForceTouch(GameObject obj)
		{
				Collider objCollider = (obj != null ? obj.GetComponentInChildren<Collider>() : null);
				if (objCollider != null)
				{
					OnTriggerEnter(objCollider);
				}
		}
		/// <summary>
		/// The ToggleControllerRigidBody method toggles the controller's rigidbody's ability to detect collisions. If it is true then the controller rigidbody will collide with other collidable game objects.
		/// </summary>
		/// <param name="state">The state of whether the rigidbody is on or off. `true` toggles the rigidbody on and `false` turns it off.</param>
		/// <param name="forceToggle">Determines if the rigidbody has been forced into it's new state by another script. This can be used to override other non-force settings. Defaults to `false`</param>
		public virtual void ToggleControllerRigidBody(bool state, bool forceToggle = false)
		{
			if (controllerCollisionDetector != null && touchRigidBody != null)
			{
				touchRigidBody.isKinematic = !state;
				Collider[] foundColliders = controllerCollisionDetector.GetComponentsInChildren<Collider>();
				for (int i = 0; i < foundColliders.Length; i++)
				{
						foundColliders[i].isTrigger = !state;
				}
				if (state)
				{
					OnControllerRigidbodyActivated(SetControllerInteractEvent(null));
				}
				else
				{
					OnControllerRigidbodyDeactivated(SetControllerInteractEvent(null));
				}
			}
		}
		/// <summary>
		/// The IsRigidBodyActive method checks to see if the rigidbody on the controller object is active and can affect other rigidbodies in the scene.
		/// </summary>
		/// <returns>Is true if the rigidbody on the controller is currently active and able to affect other scene rigidbodies.</returns>
		public virtual bool IsRigidBodyActive()
		{
			return !touchRigidBody.isKinematic;
		}

		/// <summary>
		/// The ControllerColliders method retrieves all of the associated colliders on the controller.
		/// </summary>
		/// <returns>An array of colliders that are associated with the controller.</returns>
		public virtual Collider[] ControllerColliders()
		{
			return (controllerCollisionDetector != null && controllerCollisionDetector.GetComponents<Collider>().Length > 0 ? controllerCollisionDetector.GetComponents<Collider>() : controllerCollisionDetector.GetComponentsInChildren<Collider>());
		}
		#endregion
		//—————————
		#region Initilization 
		protected virtual void Awake()
		{
			VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
		}

		protected virtual void OnEnable()
		{
			SDK_BaseController.ControllerHand controllerHand = VRTK_DeviceFinder.GetControllerHand(gameObject);
			defaultColliderPrefab = Resources.Load(VRTK_SDK_Bridge.GetControllerDefaultColliderPath(controllerHand));

			VRTK_PlayerObject.SetPlayerObject(gameObject, VRTK_PlayerObject.ObjectTypes.Controller);
			triggerRumble = false;
			CreateTouchCollider();
			CreateTouchRigidBody();
		}
		#endregion
		//—————————
		#region Core 
		protected virtual void FixedUpdate()
		{
			if (touchedColliders.Count > 0)
			{
				if (touchedColliders.Count > 1)
				{
					SortTouchedCollidersList();
				}
				if (touchedObject != touchedColliders[0].gameObject)
				{
					if (touchedObject != null)
					{
						UnTouch(touchedObject,GetInteractableScript(touchedObject));
					}
					GameObject touched = touchedColliders[0].gameObject;
					InteractableObject touchedScript = GetInteractableScript(touchedColliders[0]);
					Touch(touched, touchedScript);
				}
			}
			else
			{
				touchedObject = null;
			}
		}
		protected void SortTouchedCollidersList()
    {
			touchedColliders.Sort(
				delegate(Collider a, Collider b)
				{
					return Vector3.Distance(this.transform.position,a.transform.position)
					.CompareTo(
					Vector3.Distance(this.transform.position,b.transform.position) );
				}
			);
    }
		protected virtual void OnTriggerEnter(Collider collider)
		{

			GameObject touched = collider.gameObject;
			InteractableObject touchedScript = touched.GetComponent<InteractableObject>();
			//If the new collider is not part of the existing touched object (and the object isn't being grabbed) then start touching the new object
			if (touched != null && touchedScript != null && touchedColliders.Contains(collider) == false)
			{
				//CancelInvoke("ResetTriggerRumble");
				//ResetTriggerRumble();
				touchedColliders.Add(collider);
			}
		}
		protected virtual void OnTriggerExit(Collider collider)
		{
			GameObject unTouched = collider.gameObject;
			InteractableObject unTouchedScript = (unTouched != null ? unTouched.GetComponent<InteractableObject>() : null);
			//If the new collider is not part of the existing touched object (and the object isn't being grabbed) then start touching the new object
			if (unTouched != null && unTouchedScript != null && touchedColliders.Contains(collider) == true)
			{
				touchedColliders.Remove(collider);
				if (unTouched == touchedObject)
				{
					UnTouch(unTouched, unTouchedScript);
				}
			}
		}
		#endregion
		//—————————
		#region Touch
		protected void Touch(GameObject touched, InteractableObject touchedScript) 
		{
			OnControllerStartTouchInteractableObject(SetControllerInteractEvent(touchedScript.gameObject));

			touchedObject = touched;
			touchedScript.StartTouching(this);
			touchedScript.ToggleHighlight(true);
			//CancelInvoke("ResetTriggerRumble");
			//CheckRumbleController(touchedScript);

			OnControllerTouchInteractableObject(SetControllerInteractEvent(touchedScript.gameObject));
		}
		#endregion
		//—————————
		#region UnTouch
		protected void UnTouch(GameObject unTouched, InteractableObject unTouchedScript) 
		{
			OnControllerStartUntouchInteractableObject(SetControllerInteractEvent(unTouchedScript.gameObject));

			unTouchedScript.StopTouching(this);
			unTouchedScript.ToggleHighlight(false);

			touchedObject = null;

			OnControllerUntouchInteractableObject(SetControllerInteractEvent(unTouchedScript.gameObject));
		}
		#endregion
		//—————————
		#region Helper
		public InteractableObject GetInteractableScript(GameObject obj)
		{
			//Is the object a snap zone? Escape and return Null if so.
			/* 
			if (obj.GetComponent<SnapZone>())
			{
				return null;
			}
			*/
			InteractableObject check = obj.GetComponent<InteractableObject>();
			return (check != null ? check : null);
		}
		public InteractableObject GetInteractableScript(Collider collider)
		{
			return (GetInteractableScript(collider.gameObject));
		}
		protected virtual void ResetTriggerRumble()
		{
			triggerRumble = false;
		}
		protected virtual void CreateTouchCollider()
		{
			if (defaultColliderPrefab == null)
			{
				//VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.SDK_OBJECT_NOT_FOUND, "default collider prefab", "Controller SDK"));
				return;
			}
			controllerCollisionDetector = Instantiate(defaultColliderPrefab, transform.position, transform.rotation) as GameObject;
			controllerCollisionDetector.transform.SetParent(transform);
			controllerCollisionDetector.transform.localScale = transform.localScale;
			controllerCollisionDetector.name = VRTK_SharedMethods.GenerateVRTKObjectName(true, "Controller", "CollidersContainer");
			controllerCollisionDetector.AddComponent<VRTK_PlayerObject>().objectType = VRTK_PlayerObject.ObjectTypes.Collider;
		}
		protected virtual void CreateTouchRigidBody()
		{
			touchRigidBody = (GetComponent<Rigidbody>() ? GetComponent<Rigidbody>() : gameObject.AddComponent<Rigidbody>());
			touchRigidBody.isKinematic = true;
			touchRigidBody.useGravity = false;
			touchRigidBody.constraints = RigidbodyConstraints.FreezeAll;
			touchRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		}
		#endregion
		//—————————
		#region DeInitilization
		protected virtual void OnDisable()
		{
			ForceStopTouching();
		}
		protected virtual void OnDestroy()
		{
			VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
		}
		#endregion
	}
}
