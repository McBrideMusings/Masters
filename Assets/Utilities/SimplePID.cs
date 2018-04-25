//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce {
	/// <summary>
	/// A very simple PID controller component class.
	/// </summary>
	[System.Serializable]
	public class SimplePID {
		#region Public Fields
		public string name;
		public float Kp = 1;
		public float Ki = 0;
		public float Kd = 0.1f;
		#endregion


		#region Private Fields
		[ReadOnly] private float P, I, D;
		[ReadOnly] private float prevError;
		#endregion

		#region Core 
    /// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		public float GetOutput(float currentError, float deltaTime)
		{
			P = currentError;
			I += P * deltaTime;
			D = (P - prevError) / deltaTime;
			prevError = currentError;
			
			return P*Kp + I*Ki + D*Kd;
		}
		#endregion

		#region Helper 
		#endregion
	}
}