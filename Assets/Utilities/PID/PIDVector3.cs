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
  /// class description
  /// </summary>
	public class PIDVector3 
	{
		#region Config Fields
		#endregion

		#region State Fields
		private PID[] internalController;
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
				return internalController[0].Kp;
			}
			set
			{
				if (value < 0.0f)
					throw new ArgumentOutOfRangeException("value", "Kp must be a non-negative number.");

				internalController[0].Kp = value;
				internalController[1].Kp = value;
				internalController[2].Kp = value;
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
				return internalController[0].Ki;
			}
			set
			{
				if (value < 0.0f)
					throw new ArgumentOutOfRangeException("value", "Ki must be a non-negative number.");

				internalController[0].Ki = value;
				internalController[1].Ki = value;
				internalController[2].Ki = value;
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
				return internalController[0].Kd;
			}
			set
			{
				if (value < 0.0f)
					throw new ArgumentOutOfRangeException("value", "Kd must be a non-negative number.");

				internalController[0].Kd = value;
				internalController[1].Kd = value;
				internalController[2].Kd = value;
			}
		}		
		#endregion

		#region Initialization
		/// <summary>
		/// Initializes a new instance of the <see cref="PidQuaternionController" /> class.
		/// </summary>
		/// <param name="kp">The proportional gain.</param>
		/// <param name="ki">The integral gain.</param>
		/// <param name="kd">The derivative gain.</param>
		/// <exception cref="ArgumentException">If one of the parameters is negative.</exception>
		public PIDVector3(float kp, float ki, float kd)
		{
			if (kp < 0.0f)
				throw new ArgumentOutOfRangeException("kp", "kp must be a non-negative number.");
			if (ki < 0.0f)
				throw new ArgumentOutOfRangeException("ki", "ki must be a non-negative number.");
			if (kd < 0.0f)
				throw new ArgumentOutOfRangeException("kd", "kd must be a non-negative number.");
			internalController = new[]
														{
																new PID(kp, ki, kd),
																new PID(kp, ki, kd),
																new PID(kp, ki, kd)
														};
		}
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		protected void Awake() 
		{
			// Set Initial Values
			
			// Get Required References
			
			// Check Required References
		}
		#endregion

		#region Core
		/// <summary>
		/// Computes the angular acceleration required to rotate from the current orientation to
		/// a desired orientation based on the specified current angular velocity for the current frame.
		/// </summary>
		/// <param name="currentPosition">The current position.</param>
		/// <param name="desiredPosition">The desired position.</param>
		/// <param name="deltaTime">The frame delta time.</param>
		/// <returns>The angular acceleration required to rotate from the current orientation to the desired orientation.</returns>
		public Vector3 ComputeVelocity(Vector3 currentPosition, Vector3 desiredPosition, float deltaTime)
		{
			Vector3 error = desiredPosition - currentPosition;

			Vector3 result = ComputeOutput(error, deltaTime);
			return result;
		}
		private Vector3 ComputeOutput(Vector3 error, float deltaTime)
		{
			var output = new Vector3
										{
												x = internalController[0].ComputeOutput(error.x, deltaTime),
												y = internalController[1].ComputeOutput(error.y, deltaTime),
												z = internalController[2].ComputeOutput(error.z, deltaTime)
										};

			return output;
		}
		#endregion
	}
}
