//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using UnityEngine;

namespace PlayByPierce 
{
	/// <summary>
  /// class description
  /// </summary>
	public class MinAttribute : PropertyAttribute 
	{
		public float minValue;

		public MinAttribute( float minValue )
		{
			this.minValue = minValue;
		}
	}
}
