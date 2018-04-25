//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Pierce R McBride
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	public delegate void HealthTrailDelegate(HealthTrail trail);
	/// <summary>
  /// Simple script that turns on/off a XR trail renderer based on whether the 
	/// Associated interactable object is moving
  /// </summary>
	public class HealthTrail : MonoBehaviour 
	{
		#region Config Fields
		[Header("References")]
		public InteractableObject interactableScript;
		public XRTrailRenderer xRTrail;
		#endregion
		
		#region Delegates
		public event HealthTrailDelegate onStartTracking;
		public event HealthTrailDelegate onStopTracking;
		#endregion

		#region Initialization
		protected void Awake() 
		{
			// Set Initial Values
			if (xRTrail) xRTrail.enabled = false;
			// Get Required References
			interactableScript.InteractableGrabbed += OnInteractableGrabbed;
			interactableScript.InteractableUngrabbed += OnInteractableUnGrabbed;
		}
		#endregion

		#region Listeners
		protected void OnInteractableGrabbed(object sender, InteractableEventArgs e)
		{
			if (xRTrail) xRTrail.Clear();
			if (xRTrail) xRTrail.enabled = true;
			if (onStartTracking != null) onStartTracking(this);
		}
		protected void OnInteractableUnGrabbed(object sender, InteractableEventArgs e)
		{
			if (xRTrail) xRTrail.enabled = false;			
			if (onStopTracking != null) onStopTracking(this);
		}
		#endregion
	}
}
