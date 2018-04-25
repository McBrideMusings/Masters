//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	public delegate void CollisionDelegate(Collision collision, Collider parent);
	/// <summary>
  /// class description
  /// </summary>
	public class ColliderEvents : MonoBehaviour 
	{
		#region Delegates
		public event CollisionDelegate CollisionEnter;
		public event CollisionDelegate CollisionStay;
		public event CollisionDelegate CollisionExit;
		#endregion

		#region Helper
		void OnCollisionEnter(Collision collision) 
		{
			// Check if someone is listening to our event
			if(CollisionEnter != null)
			{
				CollisionEnter(collision, transform.parent.GetComponent<Collider>());
			}
    }
		void OnCollisionStay(Collision collision)
		{
			// Check if someone is listening to our event
			if(CollisionStay != null)
			{
				CollisionStay(collision, transform.parent.GetComponent<Collider>());
			}
		}
    void OnCollisionExit(Collision collision)
    {
			// Check if someone is listening to our event
			if(CollisionExit != null)
			{
				CollisionExit(collision, transform.parent.GetComponent<Collider>());
			}
    }
		#endregion
	}
}