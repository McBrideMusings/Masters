//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PlayByPierce
{
	/// <summary>
	/// A standard PID controller implementation.
	/// </summary>
	/// <remarks>See https://en.wikipedia.org/wiki/PID_controller.</remarks>
	public class PID 
	{
		#region Private Fields
		private float integralMax;
		private float integral;

		private float kp;
		private float ki;
		private float kd;
		#endregion

		#region Bookkeeping
		private const float MAXOUTPUT = 1000.0f;
		private float lastError = 0f;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the proportional gain.
		/// </summary>
		/// <value>The proportional gain.</value>
		public float Kp
		{
			get
			{
				return kp;
			}
			set
			{
				if (value < 0.0f)
				{
					throw new ArgumentOutOfRangeException("value", "Kp must be a non-negative number.");
				}
				kp = value;
			}
		}
		/// <summary>
		/// Gets or sets the integral gain.
		/// </summary>
		/// <value>The integral gain.</value>
		public float Ki
		{
			get
			{
				return ki;
			}
			set
			{
				if (value < 0.0f)
				{
					throw new ArgumentOutOfRangeException("value", "Ki must be a non-negative number.");
				}
				ki = value;
				integralMax = MAXOUTPUT / Ki;
				integral = Mathf.Clamp(integral, -integralMax, integralMax);
			}
		}
		/// <summary>
		/// Gets or sets the derivative gain.
		/// </summary>
		/// <value>The derivative gain.</value>
		public float Kd
		{
			get
			{
				return kd;
			}
			set
			{
				if (value < 0.0f)
				{
					throw new ArgumentOutOfRangeException("value", "Kd must be a non-negative number.");
				}
				kd = value;
			}
		}
		#endregion

		#region Initialization
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
		public PID(float kp, float ki, float kd)
		{
			if (kp < 0.0f)
			{
				throw new ArgumentOutOfRangeException("kp", "kp must be a non-negative number.");
			}
			if (ki < 0.0f)
			{
				throw new ArgumentOutOfRangeException("ki", "ki must be a non-negative number.");
			}
			if (kd < 0.0f)
			{
				throw new ArgumentOutOfRangeException("kd", "kd must be a non-negative number.");
			}
			Kp = kp;
			Ki = ki;
			Kd = kd;
			integralMax = MAXOUTPUT / Ki;
		}
		#endregion

		#region Core
		/// <summary>
		/// Computes the corrective output.
		/// </summary>
		/// <param name="error">The current error of the signal.</param>
		/// <param name="delta">The delta of the signal since last frame.</param>
		/// <param name="deltaTime">The delta time.</param>
		/// <returns>The corrective output.</returns>
		public float ComputeOutput(float error, float delta, float deltaTime)
		{
			integral += (error * deltaTime);
			integral = Mathf.Clamp(integral, -integralMax, integralMax);

			float derivative = delta / deltaTime;
			lastError = error;
			float output = (Kp * error) + (Ki * integral) + (Kd * derivative);

			output = Mathf.Clamp(output, -MAXOUTPUT, MAXOUTPUT);

			return output;
		}
		/// <summary>
		/// Computes the corrective output.
		/// </summary>
		/// <param name="error">The current error of the signal.</param>
		/// <param name="deltaTime">The delta time.</param>
		/// <returns>The corrective output.</returns>
		public float ComputeOutput(float error, float deltaTime)
		{
			integral += (error * deltaTime);
			integral = Mathf.Clamp(integral, -integralMax, integralMax);

			float derivative = (error - lastError) / deltaTime;
			lastError = error;
			float output = (Kp * error) + (Ki * integral) + (Kd * derivative);

			output = Mathf.Clamp(output, -MAXOUTPUT, MAXOUTPUT);

			return output;
		}
		#endregion
	}
}
