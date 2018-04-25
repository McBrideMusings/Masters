//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Pierce R McBride
//————————————————————————————————————————————————————————————————————————————
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayByPierce.Phys
{
	/// <summary>
  /// Might not use this
  /// </summary>
	[System.Serializable]
	public class BodyMass 
	{
		public BodyMass pair;
		public Rigidbody rigidbody;
		public float mass;
		public bool hasChanged = false;
		public BodyMass(Rigidbody rigidbody)
		{
			this.pair = null;
			this.rigidbody = rigidbody;
			this.mass = rigidbody.mass;
		}
	}
	/// <summary>
  /// class description
  /// </summary>
	public class PhysMass : MonoBehaviour 
	{
		#region Public Fields
		public bool renderMass = false;
		[Min(0.1f)] public float totalMass = 5f;
		public BodyMass[] bodyMasses;
		#endregion

		#region Private Fields
		#endregion

		#region Bookkeeping
		//private bool rootChanged = false;
		[SerializeField] [ReadOnly] private Rigidbody[] rigidbodies;
		private float currentTotalMass = 0f;
		private const float MAX_MASS_RENDER = 0.3f;
		private const float MIN_MASS_RENDER = 0.005f;
		private 
		#endregion

		#region Initialization
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		void Awake() 
		{
			
		}
		#endregion

		#region Core
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		void Update() 
		{
			
		}
		#endregion

		#region Helper
		private void CalcTotalMass() // Should Caculate total mass
		{
			foreach (Rigidbody body in rigidbodies)
			{
				currentTotalMass += body.mass;
			}
		}
		private void BalanceMassPercentages()
		{
			// should be run to validate changes made to mass percentages
			// distributes change evenly across all unlocked masses
		}
		public void CanDistTotalMass()
		{

		}
		private void DistTotalMass()
		{
			foreach (Rigidbody body in rigidbodies)
			{
				//Should use distributed mass percentages to distribute total mass
			}
		}
		#endregion

		#region Editor
		void OnDrawGizmos() 
		{
			if (bodyMasses != null && bodyMasses.Length > 0 && renderMass)
			{
				//float biggestMass = bodyMasses[0].mass;
				//float smallestMass = bodyMasses[bodyMasses.Length-1].mass;
				//float biggestSize
				for (int i = 0; i < bodyMasses.Length; i++)
				{
					float percentOfTotalMass = Mathf.InverseLerp(0,totalMass,bodyMasses[i].mass);
					percentOfTotalMass = Mathf.Clamp(percentOfTotalMass, MIN_MASS_RENDER, MAX_MASS_RENDER);
					Gizmos.color = new Color(76,175,80);
					Gizmos.DrawWireSphere(bodyMasses[i].rigidbody.transform.position, percentOfTotalMass);
					Gizmos.color = new Color(27,94,32);
					Gizmos.DrawSphere(bodyMasses[i].rigidbody.transform.position, MIN_MASS_RENDER);
				}
				foreach (BodyMass body in bodyMasses)
				{
					//float percentOfTotalMass = Mathf.InverseLerp(0,totalMass,distributedMass);
					/// Set a max size Gizmo Sphere
					/// Find sorting Algorithm so I can sort by size. Also work off of a different array that's copied from 
					/// bodymasses because the editor will shift a ton if not
					/// no. float percentOfTotalMass = Mathf.InverseLerp(0,totalMass,distributedMass);
					/// let it shift order That's actually probably more useful anyway.!-- 
					/// torso and such as the top of the list
					/// figure out the size of the gizmo based off inverse lerp between biggest sphere and zero
					/// Draw darker dot in center
					/// 
				}
			}
    }
		private void OnValidate() 
		{
			if (bodyMasses == null || bodyMasses.Length == 0) 
			{
				ValidateBodyMasses();
			}
			else
			{
				for (int i = 0; i < bodyMasses.Length; i++)
				{
					ChangeBodyMasses(i);
					if (bodyMasses[i].hasChanged)
					{
						//ChangeBodyMasses(i);
					}
				}
			}
		}
		private void ValidateBodyMasses()
		{
			if (rigidbodies == null) rigidbodies = transform.GetComponentsInChildren<Rigidbody>();
			if (rigidbodies.Length == 0) 
			{
				Debug.LogError("No Rigidbodies found as children of "+name);
				return;
			}
			bodyMasses = new BodyMass[rigidbodies.Length];
			for (int i = 0; i < rigidbodies.Length; i++)
			{
				bodyMasses[i] = new BodyMass(rigidbodies[i]);
			}
			SortBodyMasses();
		}
		private void ChangeBodyMasses(int index)
		{
			//SortBodyMasses();
			bodyMasses[index].rigidbody.mass = bodyMasses[index].mass;
			//Debug.Log(bodyMasses[index].rigidbody.name+" Index =="+index);
			bodyMasses[index].hasChanged = false;
		}
		private void SortBodyMasses()
		{
			bodyMasses = bodyMasses.OrderByDescending(bodyMasses=>bodyMasses.mass).ToArray();
		}
		#endregion
	}
}
