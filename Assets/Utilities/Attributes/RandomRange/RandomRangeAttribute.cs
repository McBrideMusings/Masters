//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PlayByPierce 
{
	/// <summary>
	/// Lets the user define a min-max range
	/// </summary>
	/// <seealso cref="UnityEngine.PropertyAttribute" />
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class RandomRangeAttribute : PropertyAttribute 
	{
		public float minLimit, maxLimit;

		public RandomRangeAttribute( float minLimit, float maxLimit )
		{
			this.minLimit = minLimit;
			this.maxLimit = maxLimit;
		}
	}
	[System.Serializable]
	public class RandomRange
	{
		public float rangeStart, rangeEnd;

		public float GetRandomValue()
		{
			return UnityEngine.Random.Range( rangeStart, rangeEnd );
		}
	}
}
