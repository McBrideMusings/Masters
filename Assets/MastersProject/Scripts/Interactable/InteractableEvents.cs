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
  /// Based class that writes boilerplate event listeners for interactable objects
  /// </summary>
	public class InteractableEvents : MonoBehaviour 
	{
		#region Config
		[Header("References")]
		public InteractableObject interactable;
		#endregion


		#region Delegates
		public virtual void OnInteractableTouched(object sender, InteractableEventArgs e) {}
		public virtual void OnInteractableUnTouched(object sender, InteractableEventArgs e) {}
		public virtual void OnInteractableGrabbed(object sender, InteractableEventArgs e) {}
		public virtual void OnInteractableUnGrabbed(object sender, InteractableEventArgs e) {}
		public virtual void OnInteractableUsed(object sender, InteractableEventArgs e) {}
		public virtual void OnInteractableUnUsed(object sender, InteractableEventArgs e) {}
		#endregion


		#region Initialization
    // Set Delegates
		protected virtual void Awake() 
		{
			if (interactable == null) interactable = GetComponent<InteractableObject>();
			interactable.InteractableTouched += OnInteractableTouched;
			interactable.InteractableUntouched += OnInteractableUnTouched;
			interactable.InteractableGrabbed += OnInteractableGrabbed;
			interactable.InteractableUngrabbed += OnInteractableUnGrabbed;
			interactable.InteractableUsed += OnInteractableUsed;
			interactable.InteractableUnused += OnInteractableUnUsed;
		}
		#endregion
	}
}
