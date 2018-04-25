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
  /// class description
  /// </summary>
	public delegate void GrabbablesEventHandler();
	public class AvatarGrabbables : MonoBehaviour 
	{
		#region Config
		public LayerMask unGrabbableLayer;
		[ReadOnly] public List<Rigidbody> grabbables = new List<Rigidbody>();
		#endregion

		#region State
		private bool toSort = false;
		#endregion

		#region Events
		public event GrabbablesEventHandler Touching;
		public event GrabbablesEventHandler NotTouching;
		#endregion


		#region Initialization
		void Awake() 
		{
			
		}
		#endregion

		#region Core
		void Update() 
		{
			if (toSort)
			{
				grabbables.Sort(
					delegate(Rigidbody a, Rigidbody b)
					{
						return Vector3.Distance(this.transform.position,a.transform.position)
						.CompareTo(
						Vector3.Distance(this.transform.position,b.transform.position) );
					}
				);
				toSort = false;
			}
		}
		void OnTriggerEnter(Collider other) 
		{
			Rigidbody otherRB = other.GetComponent<Rigidbody>();
			//Debug.Log("ungrabbablelayer contains "+otherRB.gameObject.name+" = "+unGrabbableLayer.Contains(otherRB.gameObject.layer));
			if (otherRB)
			{
				if (!grabbables.Contains(otherRB) && !(unGrabbableLayer.Contains(otherRB.gameObject.layer)))
				{
					grabbables.Add(otherRB);
					toSort = true;
				}
			}
		}
		void OnTriggerExit(Collider other) 
		{
			Rigidbody otherRB = other.GetComponent<Rigidbody>();
			if (otherRB)
			{
				if (grabbables.Contains(otherRB))
				{
					//Debug.Log("ungrabbablelayer contains "+otherRB.gameObject.name+" = "+unGrabbableLayer.Contains(otherRB.gameObject.layer));
					grabbables.Remove(otherRB);
					toSort = true;
				}
			}		
		}
		#endregion

		#region Helper
		#endregion
	}
}
