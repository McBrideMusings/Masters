//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce
{
	/// <summary>
  /// class description
  /// </summary>
	public class ExampleQuaObject : MonoBehaviour 
	{
		#region Public Fields
		public Quaternion DesiredOrientation { get; set; }
		#endregion

		#region Private Fields
		private PIDQuaternion pidController = new PIDQuaternion(8.0f, 0.0f, 0.05f);

		private Transform currentTransform;
		private Rigidbody objectRigidbody;

		public float Kp;
		public float Ki;
		public float Kd;
		#endregion

		#region Bookkeeping
		#endregion

		#region Initialization
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		void Awake() 
		{
			currentTransform = transform;
      objectRigidbody = GetComponent<Rigidbody>();
		}
		#endregion

		#region Core
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		void FixedUpdate() 
		{
			// DesiredOrientation == null || 
			if (currentTransform == null || objectRigidbody == null) {
				return;
			}

			pidController.Kp = Kp;
			pidController.Ki = Ki;
			pidController.Kd = Kd;

			// The PID controller takes the current orientation of an object, its desired orientation and the current angular velocity
			// and returns the required angular acceleration to rotate towards the desired orientation.
			Vector3 requiredAngularAcceleration = pidController.ComputeRequiredAngularAcceleration(currentTransform.rotation,
																																														DesiredOrientation,
																																														objectRigidbody.angularVelocity,
																																														Time.fixedDeltaTime);

			objectRigidbody.AddTorque(requiredAngularAcceleration, ForceMode.Acceleration);
		}
		#endregion
	}
}
