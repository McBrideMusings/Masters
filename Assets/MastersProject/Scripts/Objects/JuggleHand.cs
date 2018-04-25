//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Pierce R McBride
//————————————————————————————————————————————————————————————————————————————
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters 
{
	/// <summary>
  /// class description
  /// </summary>
	public class JuggleHand : MonoBehaviour 
	{
		#region Config
		[Header("References")]
		public Rigidbody overrideRigidbody;
		
		[Header("Config")]
		public float refractoryPeriod = 1f;
		public float velocityThreshold = 1f;
		#endregion

		#region State
		[Space(10f)]
		private new Rigidbody rigidbody;
		private List<JuggleBall> balls = new List<JuggleBall>();
		private float refractoryCurrent = 0f;
		#endregion

		#region Delegates
		#endregion

		#region Properties
		#endregion

		#region Public
		public void AddBall(JuggleBall newBall) 
		{
			balls.Add(newBall);
		}
		#endregion

		#region Initialization
    // Get References and Set Initial Values
		protected void Awake() 
		{
			if (overrideRigidbody)
			{
				rigidbody = overrideRigidbody;
			}
			else
			{
				rigidbody = GetComponent<Rigidbody>();
			}
		}
		#endregion

		#region Core
    /// Updates Once Per Physics Update
		protected void FixedUpdate() 
		{
			if (refractoryCurrent > 0) 
			{
				refractoryCurrent -= Time.deltaTime;
			}
			else
			{
				if (rigidbody.velocity.y > velocityThreshold)
				{
					if (balls.Count > 0)
					{
						JuggleBall poppedBall = balls[0];
						balls.RemoveAt(0);
						poppedBall.Bounce();
						refractoryCurrent = refractoryPeriod;
					}
				}
			}
		}
		#endregion
	}
}
