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
  /// Basic physics tracking. Always tracks to a specific target.
  /// </summary>
	public class PhysTrack : PhysBase 
	{
		#region Public Fields
		[Space(10f)]
		public Transform target;
		#endregion

		#region Private Fields
		#endregion

		#region Bookkeeping
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
			Track(target,weight);
			Rotate(target);
		}
		#endregion

		#region Helper
		#endregion
	}
}
