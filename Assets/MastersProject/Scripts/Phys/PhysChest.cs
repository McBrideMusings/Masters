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
  /// Physically tracks a position up to an arbitrary height above a ground object layer
	/// Ground being any rigidbody below the body that is set as a Ground layer
  /// </summary>
	public class PhysChest : PhysBase 
	{
		#region Public Fields
		[Space(10f)]
		public Transform target;
		public InteractableObject targetScript;
		public LayerMask groundMask;
		public float heightMax = 1f;
		#endregion

		#region Private Fields
		[SerializeField] [ReadOnly] private bool isGrabbed = false;
		[SerializeField] [ReadOnly] private bool foundGround = false;
		[SerializeField] [ReadOnly] private Vector3 height = Vector3.zero;
		[SerializeField] [ReadOnly] private Vector3 ground;
		#endregion

		#region Bookkeeping
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
		}
		public void OnInteractableUsed(object sender, InteractableEventArgs e) {}
		public void OnInteractableUnUsed(object sender, InteractableEventArgs e) {}
		#endregion

		#region Initialization
		protected override void Awake() 
		{
			base.Awake();
			if (targetScript)
			{
				targetScript.InteractableTouched += OnInteractableTouched;
				targetScript.InteractableUntouched += OnInteractableUnTouched;
				targetScript.InteractableGrabbed += OnInteractableGrabbed;
				targetScript.InteractableUngrabbed += OnInteractableUnGrabbed;
				targetScript.InteractableUsed += OnInteractableUsed;
				targetScript.InteractableUnused += OnInteractableUnUsed;
			}
			else 
			{
				initilized = true;
			}
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
			FindGround(target.position);
			GetHeight();
			Track(height,weight);
			Rotate(target);

			if (!isGrabbed)
			{
				target.position = height;
			}
		}
		#endregion

		#region Helper
		public void GetHeight() 
		{
			height = target.position;
			if (foundGround)
			{
				if (height.y - ground.y > heightMax)
				{
					height.y = heightMax + ground.y;
				}
			}
		}
		public void FindGround(Vector3 rayStart) 
		{
			RaycastHit hit;
			foundGround = Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, groundMask);
			if (foundGround) 
			{
				ground = hit.point;
			}
		}
		#endregion
	}
}
