//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Pierce R McBride
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	/// <summary>
  /// Manages the audience made up of spectators, which are cataloged based off parent/child relationship.
	/// Health slowly deteriorates, unless puppet moves inside trigger box. Motion Trackers must be on trackerLayer.
  /// </summary>
	[RequireComponent(typeof (AudioSource))]
	public class AudienceManager : MonoBehaviour 
	{
		#region Config
		[Header("References")]
		public GameObject[] spectatorsParents; // All children are considered audience members

		[Header("Config")]
		public LayerMask trackerLayer;
		public AudioClip spectatorAdd;
		public AudioClip spectatorRemove;
		public float healthMax = 1f;
		public float healthGainRate = 0.2f;
		public float healthDecayRate = 0.2f;
		#endregion

		#region State
		[Space(10f)]
		[SerializeField] [ReadOnly] private float healthCurrent;
		[SerializeField] [ReadOnly] private List<HealthTrail> trackers = new List<HealthTrail>();
		[SerializeField] [ReadOnly] private List<HealthTrail> trackersActive = new List<HealthTrail>();
		[SerializeField] [ReadOnly] private GameObject[] spectators;
		[SerializeField] [ReadOnly] private int spectatorsTotal;
		[SerializeField] [ReadOnly] private int spectatorsVisible; 
		private AudioSource source;
		#endregion

		#region Delegates
		#endregion

		#region Initialization
		protected void Awake() 
		{
			// Set Initial Values
			healthCurrent = healthMax;
			source = GetComponent<AudioSource>();
			// Get Required References
			List<GameObject> tempSpectator = new List<GameObject>();
			for (int i = 0; i < spectatorsParents.Length; i++)
			{
				tempSpectator.AddRange(spectatorsParents[i].GetChildren()); 
			}
			spectators = tempSpectator.ToArray();
			spectatorsTotal = spectators.Length;
			spectatorsVisible = 0;
			// Check Required References
		}
		#endregion

		#region Core
		protected void Update() 
		{
			if (trackersActive.Count > 0)
			{
				healthCurrent = Mathf.Clamp(healthCurrent+healthGainRate, 0, healthMax);
			}
			else 
			{
				healthCurrent = Mathf.Clamp(healthCurrent-healthDecayRate, 0, healthMax);
			}
			UpdateVisibleSpectators(Mathf.RoundToInt(spectatorsTotal * healthCurrent));
			source.volume = (Mathf.InverseLerp(0,healthMax,healthCurrent));
		}
		protected void OnTriggerStay(Collider other)
		{
			GameObject tracker = other.gameObject;
			if (trackerLayer.Contains(tracker.layer))
			{
				HealthTrail trackerTrail = tracker.GetComponent<HealthTrail>();
				if (!trackers.Contains(trackerTrail))
				{
					trackers.Add(trackerTrail);	
					trackerTrail.onStartTracking += TrackerStartsTracking;
					trackerTrail.onStopTracking += TrackerStopsTracking;
				}
			}
		}
		protected void OnTriggerExit(Collider other)
		{
			GameObject tracker = other.gameObject;
			if (trackerLayer.Contains(tracker.layer))
			{
				HealthTrail trackerTrail = tracker.GetComponent<HealthTrail>();
				if (trackers.Contains(trackerTrail))
				{
					trackerTrail.onStartTracking -= TrackerStartsTracking;
					trackerTrail.onStopTracking -= TrackerStopsTracking;
					trackers.Remove(trackerTrail);	
				}
			}
		}
		#endregion

		#region Helper
		protected void UpdateVisibleSpectators(int newVisible)
		{
			if (spectatorsVisible == newVisible) return;

			if (spectatorsVisible < newVisible)
			{
				spectatorsVisible++;
				if (spectatorAdd != null)
				{
					source.clip = spectatorAdd;
					source.Play();
				}
				spectators[spectatorsVisible].GetComponent<ParticleSystem>().Play();
				spectators[spectatorsVisible].GetComponent<Renderer>().enabled = true;
			}
			else 
			{
				spectatorsVisible--;
				if (spectatorRemove != null)
				{
					source.clip = spectatorRemove;
					source.Play();
				}
				spectators[spectatorsVisible].GetComponent<ParticleSystem>().Play();
				spectators[spectatorsVisible].GetComponent<Renderer>().enabled = false;
			}
		}
		#endregion

		#region Listeners
		protected void TrackerStartsTracking(HealthTrail trail)
		{
			trackersActive.Add(trail);
		}
		protected void TrackerStopsTracking(HealthTrail trail)
		{
			if (trackersActive.Contains(trail)) trackersActive.Remove(trail);
			else
			{
				Debug.Log("This should never happen"); // This has definitely happened
			}
		}
		#endregion
	}
}
