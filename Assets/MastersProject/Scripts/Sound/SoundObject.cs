//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     NewtonVR
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	/// <summary>
  /// class description
  /// </summary>
	public class SoundObject : MonoBehaviour 
	{
		#region Public Fields
		public SoundMaterials material;
		public float volumeMultiplier = 1f;
		#endregion

		#region Private Fields
		#endregion

		#region Bookkeeping 
		#endregion
		
		#region Core 
		protected virtual void OnCollisionEnter(Collision collision)
		{
			float volume = CalculateImpactVolume(collision);
			if (volume < GameManager.Instance.sound.minCollisionVolume)
			{
				//Debug.Log("Volume too low to play: " + volume);
				return;
			}
			GameManager.Instance.sound.Play(material, collision.contacts[0].point, volume);
		}
		#endregion

		#region Helper 
		/// <summary>
		/// Easing equation function for a cubic (t^3) easing out: 
		/// decelerating from zero velocity.
		/// </summary>
		/// <param name="velocity">Current time in seconds.</param>
		/// <param name="startingValue">Starting value.</param>
		/// <param name="changeInValue">Change in value.</param>
		/// <param name="maxCollisionVelocity">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float CubicEaseOut(float velocity, float startingValue = 0, float changeInValue = 1)
		{
			return changeInValue * ((velocity = velocity / GameManager.Instance.sound.maxCollisionVelocity - 1) * velocity * velocity + 1) + startingValue;
		}
		    public static float EaseInQuint(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value * value * value + start;
    }
		private float CalculateImpactVolume(Collision collision)
		{
			float Volume;
			//Debug.Log("Velocity: " + Collision.relativeVelocity.magnitude.ToString());
			Volume = CubicEaseOut(collision.relativeVelocity.magnitude) * volumeMultiplier;
			return Volume;
		}

		#endregion
	}
}
