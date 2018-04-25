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
	public class HapticsOnCollide : MonoBehaviour 
	{
		#region Config Fields
		[Header("References")]
		public InteractableObject interactableObject;
		public GameObject[] collisionListeners;
		
		[Header("Logic")]
		public float collisionTreshold = 1f; // God Only knows what this data looks like

		[Header("UX")]
		public float hapticsCoefficient = 1f;
		[Tooltip("Denotes how strong the rumble in the controller will be on grab.")]
		[Range(0, 1)]
		public float strengthCoefficient = 0.5f;
		[Tooltip("Denotes how long the rumble in the controller will last on grab.")]
		public float durationCoefficient = 0.1f;
		[Tooltip("Denotes interval betweens rumble in the controller on grab.")]
		public float intervalOn = minInterval;
		#endregion

		#region State Fields
		[Space(10f)]
		[SerializeField] [ReadOnly] protected const float minInterval = 0.05f;
		[SerializeField] [ReadOnly] protected GameObject controller;
		private Collider[] collidersToWatch;
		private ColliderEvents[] collideEventsToWatch;
		#endregion

		#region Bookkeeping
		#endregion
		// Use this for initialization
		void Awake() {
			collidersToWatch = new Collider[collisionListeners.Length];
			collideEventsToWatch = new ColliderEvents[collisionListeners.Length];
			for (int i = 0; i < collisionListeners.Length; i++)
			{
				collidersToWatch[i] = collisionListeners[i].GetComponent<Collider>();
				if (collidersToWatch[i] != null)
				{
					collideEventsToWatch[i] = collisionListeners[i].AddComponent(typeof(ColliderEvents)) as ColliderEvents;
					collideEventsToWatch[i].CollisionEnter += OnCollisionWatchEnter;
				}
			}
			interactableObject.InteractableGrabbed += OnInteractableGrabbed;
			interactableObject.InteractableUngrabbed += OnInteractableUnGrabbed;
		}
		#region Listeners
		public void OnInteractableGrabbed(object sender, InteractableEventArgs e) { controller = e.interactable; }
		public void OnInteractableUnGrabbed(object sender, InteractableEventArgs e) { controller = null; }
		public void OnCollisionWatchEnter(Collision collision, Collider parent) 
		{
			if (!controller && System.Array.IndexOf(collidersToWatch, collision.collider) != -1) return;
			if (parent && collision.collider == parent) return;
			if (collision.relativeVelocity.magnitude < collisionTreshold) return;

			//Debug.Log("Object Collision with "+collision.collider.name);
			TriggerHapticPulse(GetControllerReference(controller), strengthCoefficient, durationCoefficient, intervalOn);
		}
		#endregion

		#region Helper
		protected VRTK_ControllerReference GetControllerReference(GameObject interactable)
		{
			return VRTK_ControllerReference.GetControllerReference(interactable);
		}
		protected void TriggerHapticPulse(VRTK_ControllerReference controllerReference, float strength, float duration, float interval)
		{
			VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, strength, duration, (interval >= minInterval ? interval : minInterval));
		}
		#endregion
	}
}