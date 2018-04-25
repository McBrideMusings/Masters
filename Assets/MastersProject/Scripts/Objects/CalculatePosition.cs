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
  /// HACK that finds a position between two transforms and sets Y as an offsef above 
	/// the ground. Used for the balance beam character's chest
  /// </summary>
	public class CalculatePosition : MonoBehaviour 
	{
		#region Config
		[Header("Config")]
		public Transform pos1;
		public Transform pos2;
		public Transform fixedGround;
		public float height = 1f;
		#endregion

		#region Core
    /// Updates Once Per Frame
		protected void Update() 
		{
			Vector3 newPosition = Vector3.Lerp(pos1.position, pos2.position, 0.5f); // I'm into this use of Lerp
			newPosition.y = fixedGround.position.y + height;
			transform.position = newPosition;
			
			Quaternion newRotation = Quaternion.Lerp(pos1.rotation, pos2.rotation, 0.5f);
			transform.rotation = newRotation;
		}
		#endregion

		#region Listeners
		#endregion
	}
}
