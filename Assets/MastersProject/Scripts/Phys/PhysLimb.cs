//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Pierce R McBride
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	/// <summary>
  /// Not sure If I use this
  /// </summary>
	public class PhysLimb : PhysBase 
	{
		#region Config Fields
		[Space(10f)]
		public Transform target;
		public Transform interactable;
		public InteractableObject interactableScript;
		[NonNegative] public float maxDistance = 0.02f;
		#endregion

		#region State Fields
		private bool useInteractablePosition = false;
		#endregion

		#region Bookkeeping
		[SerializeField] [ReadOnly] private bool isGrabbed = false; // Is the target object for this limb grabbed?
		[SerializeField] [ReadOnly] private Transform bodyParent;
		#endregion

		#region Delegates
		public void OnInteractableTouched(object sender, InteractableEventArgs e) {}
		public void OnInteractableUnTouched(object sender, InteractableEventArgs e) {}
		public void OnInteractableGrabbed(object sender, InteractableEventArgs e) 
		{
			isGrabbed = true;
		}
		public void OnInteractableUnGrabbed(object sender, InteractableEventArgs e) 
		{
			isGrabbed = false;
			interactable.position = target.position;
		}
		public void OnInteractableUsed(object sender, InteractableEventArgs e) {}
		public void OnInteractableUnUsed(object sender, InteractableEventArgs e) {}
		#endregion

		#region Initialization
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		protected override void Awake() 
		{
			base.Awake();
			// Set Initial Values
			
			// Get Required References
			interactableScript.InteractableTouched += OnInteractableTouched;
			interactableScript.InteractableUntouched += OnInteractableUnTouched;
			interactableScript.InteractableGrabbed += OnInteractableGrabbed;
			interactableScript.InteractableUngrabbed += OnInteractableUnGrabbed;
			interactableScript.InteractableUsed += OnInteractableUsed;
			interactableScript.InteractableUnused += OnInteractableUnUsed;

			bodyParent = body.transform.parent;
			interactable.position = target.position;
			// Check Required References
		}
		#endregion

		#region Core
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		protected override void FixedUpdate() 
		{
			base.FixedUpdate();
			if (isGrabbed) 
			{
				if (Vector3.Distance(interactable.position, bodyParent.position) > maxDistance)
				{
					target.position = bodyParent.position + ((interactable.position - bodyParent.position).normalized * maxDistance);
				}
				else
				{
					target.position = interactable.position;
				}
				
				Track(target,weight);
				Rotate(target);
			}
			else
			{
				target.position = body.position;
				interactable.position = target.position;
			}
		}
		#endregion

		#region Helper
		#endregion

		#region Listeners
		#endregion
	}
}
