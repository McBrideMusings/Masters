//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Pierce R McBride
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	public enum PosMethod { PID, AddForce };
	public enum PosTrackingMode { Global, Local };
	public enum RotTrackingMode { Match, Face };
	/// <summary>
  /// Base class for my physics system. Most of the work is done by the PID controller
  /// </summary>
	public class PhysBase : MonoBehaviour 
	{
		#region Config Fields
		public Rigidbody body;
		[Header("Position")]
		public bool trackPosition = true;
		public PosTrackingMode posMode = PosTrackingMode.Global;
		public int solverIterationCount = 6;
		public Vector3 weight = new Vector3(1,1,1);
		[Range(0f, 10f)] public float weightFalloff = 1f;
		[NonNegative] public float deadZone = 0.02f;
		public float forceMax = 500f;
		[NonNegative] public float posP = 8.0f;
		[NonNegative] public float posI = 0.0f;
		[NonNegative] public float posD = 0.05f;
		[Header("Rotation")]
		public bool trackRotation = true;
		public RotTrackingMode rotMode = RotTrackingMode.Match;
		public Vector3 trackingAxis = new Vector3(1,1,1);
		[NonNegative] public float kp = 8.0f;
		[NonNegative] public float ki = 0.0f;
		[NonNegative] public float kd = 0.05f;
		#endregion

		#region State Fields
		private PIDQuaternion pidController = new PIDQuaternion(8.0f, 0.0f, 0.05f);
		private PIDVector3 posPIDController = new PIDVector3(8.0f, 0.0f, 0.05f);
		#endregion

		#region Bookkeeping
		public new Transform transform { get; private set; }
		protected Quaternion desiredOrientation;
		protected bool initilized = false;
		protected float lastReadTime;
		protected ConfigurableJoint joint;
		protected Quaternion targetWorldRotation, targetRotation, toParentSpace, localRotationConvert;
		protected Vector3 targetVelocity, targetPosition, targetAngularVelocity, bodyVelocity;
		#endregion

		#region Initialization
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		protected virtual void Awake() 
		{
			if (body)
			{
				transform = body.transform;
				joint = body.gameObject.GetComponent<ConfigurableJoint>();
				if (joint)
				{
					if (joint.connectedBody != null) 
					{
						joint.autoConfigureConnectedAnchor = false;
					}					
				}
				initilized = true;
			}
		}
		#endregion

		#region Core
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		protected virtual void FixedUpdate() 
		{
			if (!initilized) return;
			if (body.solverIterations != solverIterationCount) body.solverIterations = solverIterationCount;
		}
		#endregion

		#region Helper
		protected virtual void Track(Transform setTarget, Vector3 setWeight)
		{
			TrackPID(setTarget.position, setWeight);
			//Track(setTarget.position, setWeight);
		}
		protected virtual void Track(Vector3 setTarget, Vector3 setWeight)
		{
			TrackPID(setTarget, setWeight);
		}
		// Most important script. Uses a target, weight and p,i,d, values to
		// apply iterative physics force to the body (part of the ragdoll)
		protected virtual void TrackPID(Vector3 setTarget, Vector3 setWeight) 
		{
			if (!trackPosition || weight == Vector3.zero) return;
			if (Vector3.Distance(setTarget, body.position) > deadZone)
			{
				Vector3 testPosition = setTarget + ((body.position - setTarget).normalized * deadZone);
				posPIDController.Kp = posP;
				posPIDController.Ki = posI;
				posPIDController.Kd = posD;
				Vector3 force = posPIDController.ComputeVelocity(body.position,testPosition,Time.fixedDeltaTime);
				force = Vector3.Scale(force,setWeight);
				if (posMode == PosTrackingMode.Global)
				{
					body.AddForce(force, ForceMode.Force);
				}
				else
				{
					body.AddRelativeForce(force, ForceMode.Force);
				}
			}
		}
		// Rotation is also based on PID system but uses Quaternion functions to derive
		// DesiredValue
		protected virtual void Rotate(Transform setTarget) 
		{ 
			if (!trackRotation) return;
			switch (rotMode)
			{
				case RotTrackingMode.Match:
					desiredOrientation = setTarget.rotation; 
					break;
				case RotTrackingMode.Face:
					desiredOrientation = Quaternion.LookRotation(setTarget.position - body.position, Vector3.up); 
					break;
				default:
					break;
			}
			RotatePID();
		}
		protected virtual void RotateMatch(Quaternion setRotation) 
		{ 
			if (!trackRotation) return;
			desiredOrientation = Quaternion.FromToRotation(body.rotation.eulerAngles, Vector3.Scale(setRotation.eulerAngles,trackingAxis));
			//desiredOrientation = setRotation; 
			RotatePID();
		}
		protected virtual void RotateFace(Vector3 setPosition) 
		{ 
			if (!trackRotation) return;
			desiredOrientation = Quaternion.LookRotation(setPosition - body.position, Vector3.up); 
			RotatePID();
		}
		protected void RotatePID() 
		{
			pidController.Kp = kp;
			pidController.Ki = ki;
			pidController.Kd = kd;
			// The PID controller takes the current orientation of an object, its desired orientation and the current angular velocity
			// and returns the required angular acceleration to rotate towards the desired orientation.
			Vector3 angularAcceleration = pidController.ComputeRequiredAngularAcceleration(body.rotation,
																																											desiredOrientation,
																																											body.angularVelocity,
																																											Time.fixedDeltaTime);

			body.AddTorque(angularAcceleration, ForceMode.Force);
		}
		#endregion

		#region Core
		protected virtual void OnValidate()
		{
			trackingAxis.x = MakeBool(trackingAxis.x);
			trackingAxis.y = MakeBool(trackingAxis.y);
			trackingAxis.z = MakeBool(trackingAxis.z);
		}
		protected virtual float MakeBool(float num)
		{	
			if (num == 1 || num == 0) return num;
			else 
			{
				if (num > 1) return 1;
				else return 0;
			}
		}
		#endregion
	}
}
