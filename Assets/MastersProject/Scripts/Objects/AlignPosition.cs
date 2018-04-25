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
  /// Hack that aligns the position of the juggler balance ball target to below the main juggler handle
	/// but Y offset to a little above the floor. 
  /// </summary>
	public class AlignPosition : MonoBehaviour 
	{
		#region Config
		[Header("Config")]
		public Transform pos1;
		public Transform fixedGround;
		public float height = 1f;
		#endregion

		#region Core
    /// Updates Once Per Frame
		protected void Update() 
		{
			Vector3 newPosition = new Vector3(pos1.position.x, fixedGround.position.y + height, pos1.position.z);
			transform.position = newPosition;
		}
		#endregion

		#region Listeners
		#endregion
	}
}
