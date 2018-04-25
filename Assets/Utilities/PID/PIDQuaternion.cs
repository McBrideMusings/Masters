﻿//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PlayByPierce
{
	/// <summary>
	/// A Quaternion-based PID controller implementation.
	/// </summary>
	/// <remarks>It uses four internal controllers to make sure the integral parts don't get mixed up between quaternion components.</remarks>
	public class PIDQuaternion 
	{
		#region Public Fields
		#endregion

		#region Private Fields
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
				internalController[3].Kp = value;
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
				internalController[3].Ki = value;
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
				internalController[3].Kd = value;
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
		public PIDQuaternion(float kp, float ki, float kd)
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
																new PID(kp, ki, kd),
																new PID(kp, ki, kd)
														};
		}
		#endregion

		#region Core
		/// <summary>
		/// Multiplies a Matrix by a quaternion treating the quaternion as a Vector4.
		/// </summary>
		/// <param name="matrix">The matrix.</param>
		/// <param name="quaternion">The quaternion.</param>
		/// <returns>The resulting quaternion.</returns>
		public Quaternion MultiplyAsVector(Matrix4x4 matrix, Quaternion quaternion)
		{
			var vector = new Vector4(quaternion.w, quaternion.x, quaternion.y, quaternion.z);

			Vector4 result = matrix * vector;

			return new Quaternion(result.y, result.z, result.w, result.x);
		}
		/// <summary>
		/// Transforms the specified Euler angle vector to a quaternion containing these Euler angles.
		/// </summary>
		/// <param name="eulerAngles">The Euler angles vector.</param>
		/// <returns>A quaternion containing Euler angles.</returns>
		public Quaternion ToEulerAngleQuaternion(Vector3 eulerAngles)
		{
			return new Quaternion(eulerAngles.x, eulerAngles.y, eulerAngles.z, 0);
		}
		/// <summary>
		/// Computes the angular acceleration required to rotate from the current orientation to
		/// a desired orientation based on the specified current angular velocity for the current frame.
		/// </summary>
		/// <param name="currentOrientation">The current orientation.</param>
		/// <param name="desiredOrientation">The desired orientation.</param>
		/// <param name="currentAngularVelocity">The current angular velocity.</param>
		/// <param name="deltaTime">The frame delta time.</param>
		/// <returns>The angular acceleration required to rotate from the current orientation to the desired orientation.</returns>
		public Vector3 ComputeRequiredAngularAcceleration(Quaternion currentOrientation, Quaternion desiredOrientation, Vector3 currentAngularVelocity, float deltaTime)
		{
			Quaternion requiredRotation = QuaTools.RequiredRotation(currentOrientation, desiredOrientation);

			Quaternion error = Quaternion.identity.Subtract(requiredRotation);
			Quaternion angularVelocity = ToEulerAngleQuaternion(currentAngularVelocity);
			Quaternion delta = angularVelocity * requiredRotation;

			var orthogonalizeMatrix = new Matrix4x4()
																{
																		m00 =
																				-requiredRotation.x * -requiredRotation.x + -requiredRotation.y * -requiredRotation.y +
																				-requiredRotation.z * -requiredRotation.z,
																		m01 =
																				-requiredRotation.x * requiredRotation.w + -requiredRotation.y * -requiredRotation.z +
																				-requiredRotation.z * requiredRotation.y,
																		m02 =
																				-requiredRotation.x * requiredRotation.z + -requiredRotation.y * requiredRotation.w +
																				-requiredRotation.z * -requiredRotation.x,
																		m03 =
																				-requiredRotation.x * -requiredRotation.y + -requiredRotation.y * requiredRotation.x +
																				-requiredRotation.z * requiredRotation.w,
																		m10 =
																				requiredRotation.w * -requiredRotation.x + -requiredRotation.z * -requiredRotation.y +
																				requiredRotation.y * -requiredRotation.z,
																		m11 =
																				requiredRotation.w * requiredRotation.w + -requiredRotation.z * -requiredRotation.z +
																				requiredRotation.y * requiredRotation.y,
																		m12 =
																				requiredRotation.w * requiredRotation.z + -requiredRotation.z * requiredRotation.w +
																				requiredRotation.y * -requiredRotation.x,
																		m13 =
																				requiredRotation.w * -requiredRotation.y + -requiredRotation.z * requiredRotation.x +
																				requiredRotation.y * requiredRotation.w,
																		m20 =
																				requiredRotation.z * -requiredRotation.x + requiredRotation.w * -requiredRotation.y +
																				-requiredRotation.x * -requiredRotation.z,
																		m21 =
																				requiredRotation.z * requiredRotation.w + requiredRotation.w * -requiredRotation.z +
																				-requiredRotation.x * requiredRotation.y,
																		m22 =
																				requiredRotation.z * requiredRotation.z + requiredRotation.w * requiredRotation.w +
																				-requiredRotation.x * -requiredRotation.x,
																		m23 =
																				requiredRotation.z * -requiredRotation.y + requiredRotation.w * requiredRotation.x +
																				-requiredRotation.x * requiredRotation.w,
																		m30 =
																				-requiredRotation.y * -requiredRotation.x + requiredRotation.x * -requiredRotation.y +
																				requiredRotation.w * -requiredRotation.z,
																		m31 =
																				-requiredRotation.y * requiredRotation.w + requiredRotation.x * -requiredRotation.z +
																				requiredRotation.w * requiredRotation.y,
																		m32 =
																				-requiredRotation.y * requiredRotation.z + requiredRotation.x * requiredRotation.w +
																				requiredRotation.w * -requiredRotation.x,
																		m33 =
																				-requiredRotation.y * -requiredRotation.y + requiredRotation.x * requiredRotation.x +
																				requiredRotation.w * requiredRotation.w,
																};

			Quaternion neededAngularVelocity = ComputeOutput(error, delta, deltaTime);

			neededAngularVelocity = MultiplyAsVector(orthogonalizeMatrix, neededAngularVelocity);

			Quaternion doubleNegative = neededAngularVelocity.Multiply(-2.0f);
			Quaternion result = doubleNegative * Quaternion.Inverse(requiredRotation);

			return new Vector3(result.x, result.y, result.z);
		}
		private Quaternion ComputeOutput(Quaternion error, Quaternion delta, float deltaTime)
		{
			var output = new Quaternion
										{
												x = internalController[0].ComputeOutput(error.x, delta.x, deltaTime),
												y = internalController[1].ComputeOutput(error.y, delta.y, deltaTime),
												z = internalController[2].ComputeOutput(error.z, delta.z, deltaTime),
												w = internalController[3].ComputeOutput(error.w, delta.w, deltaTime)
										};

			return output;
		}
		#endregion
	}
}
