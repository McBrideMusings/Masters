//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	public delegate void TriggerDelegate(Collider other);
	/// <summary>
  /// class description
  /// </summary>
	public class TriggerEvents : MonoBehaviour 
	{
		#region Config Fields
		#endregion

		#region State Fields
		#endregion

		#region Delegates
		public event TriggerDelegate TriggerEnter;
		public event TriggerDelegate TriggerStay;
		public event TriggerDelegate TriggerExit;
		#endregion

		#region Helper
		void OnTriggerEnter(Collider other) 
		{
			// Check if someone is listening to our event
			if(TriggerEnter != null)
			{
				TriggerEnter(other);
			}
    }
		void OnTriggerStay(Collider other)
		{
			// Check if someone is listening to our event
			if(TriggerStay != null)
			{
				TriggerStay(other);
			}
		}
    void OnTriggerExit(Collider other)
    {
			// Check if someone is listening to our event
			if(TriggerExit != null)
			{
				TriggerExit(other);
			}
    }
		#endregion
	}
}
