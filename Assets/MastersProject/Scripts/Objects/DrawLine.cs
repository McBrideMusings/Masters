//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Pierce R McBride
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce
{
	/// <summary>
  /// Simple Utility that uses LineRenderer to draw a line between two points at runtime.
  /// </summary>
	public class DrawLine : MonoBehaviour 
	{
		#region Public Fields
		public Transform point1;
		public Transform point2;
		#endregion

		#region Private Fields
		
		#endregion

		#region Bookkeeping
		protected bool initilized = false;
		protected LineRenderer line;
		#endregion

		#region Initialization
		void Awake() 
		{
			line = GetComponent<LineRenderer>();
			if (line && point1 && point2)
			{
				line.enabled = true;
				initilized = true;
			}
		}
		#endregion

		#region Core
		void Update() 
		{
			if (initilized)
			{
				line.SetPosition(0,point1.position);
				line.SetPosition(1,point2.position);
			}
		}
		#endregion

		#region Helper
		#endregion
	}
}
