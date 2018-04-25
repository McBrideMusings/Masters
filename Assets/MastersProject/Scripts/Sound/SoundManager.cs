//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce.Masters
{
	public enum SoundMaterials
	{
		_DEFAULT,
		CARPET,
		WOOD,
		METAL,
		GLASS,
		PLASTIC,
		CARDBOARD,
	}
	/// <summary>
  /// class description
  /// </summary>
	public class SoundManager : MonoBehaviour 
	{
		#region Public Fields
		[Tooltip("The max number of sounds that can possibly be playing at once.")]
		public int soundPoolSize = 100;

		[Tooltip("Turns on or off randomizing the pitch of the collision sounds")]
		public bool pitchModulationEnabled = true;

		[Range(0f, 3f)]
		public float pitchModulationRange = 0.5f;

		[Tooltip("Don't play collision sounds that will produce an impact with a volume lower than this number")]
		public float minCollisionVolume = 0.1f;
		public float maxCollisionVelocity = 5;
		#endregion
		//—————————
		#region Private Fields
		#endregion
		//—————————
		#region Bookkeeping 
		public System.Type typeCache;
		private string audioSourcePrefabPath = "CollisionSoundPrefab";
		private string collisionSoundsPath = "CollisionSounds";
		private GameObject audioSourcePrefab;
		private AudioSource[] audioPool;
		private int currentPoolIndex;
		private Dictionary<SoundMaterials, List<AudioClip>> clips;
		#endregion
		//—————————
		#region Initilization 
		public void Awake()
		{
			typeCache = typeof(SoundMaterials);	
			audioPool = new AudioSource[soundPoolSize];

			audioSourcePrefab = Resources.Load<GameObject>(audioSourcePrefabPath);

			for (int index = 0; index < audioPool.Length; index++)
			{
				audioPool[index] = GameObject.Instantiate<GameObject>(audioSourcePrefab).GetComponent<AudioSource>();
				audioPool[index].transform.parent = this.transform;
			}

			AudioClip[] loadedClips = Resources.LoadAll<AudioClip>(collisionSoundsPath);
			clips = new Dictionary<SoundMaterials, List<AudioClip>>();
			for (int index = 0; index < loadedClips.Length; index++)
			{
				string name = loadedClips[index].name;
				int dividerIndex = name.IndexOf("__");
				if (dividerIndex >= 0)
				{
					name = name.Substring(0, dividerIndex);
				}

				SoundMaterials material = ParseName(name);
				if (clips.ContainsKey(material) == false || clips[material] == null)
				{
					clips[material] = new List<AudioClip>();
				}
				clips[material].Add(loadedClips[index]);
			}
		}
		#endregion
		//—————————
		#region Core 
		public void Play(SoundMaterials material, Vector3 position, float impactVolume)
		{
			if (pitchModulationEnabled == true)
			{
				audioPool[currentPoolIndex].pitch = Random.Range(1 - pitchModulationRange, 1 + pitchModulationRange);
			}

			audioPool[currentPoolIndex].transform.position = position;
			audioPool[currentPoolIndex].volume = impactVolume;
			audioPool[currentPoolIndex].clip = GetClip(material);
			audioPool[currentPoolIndex].Play();

			currentPoolIndex++;

			if (currentPoolIndex >= audioPool.Length)
			{
				currentPoolIndex = 0;
			}
		}
		#endregion
		//—————————
		#region Helper 
		private AudioClip GetClip(SoundMaterials material)
		{ 
				if (clips.ContainsKey(material) == false)
				{
					material = SoundMaterials._DEFAULT;
					Debug.LogError("Trying to play sound for material without a clip. Need a clip at: " + collisionSoundsPath + "/" + material.ToString());
				}

				int index = Random.Range(0, clips[material].Count);

				return clips[material][index];
		}
		public SoundMaterials ParseName(string materialString)
		{
			materialString = materialString.ToUpper();
			bool defined = System.Enum.IsDefined(typeCache, materialString);
			
			if (defined == true)
			{
				return (SoundMaterials)System.Enum.Parse(typeCache, materialString);
			}
			else
			{
				return (SoundMaterials)System.Enum.Parse(typeCache, "_DEFAULT");
			}
		}
		#endregion
	}
}
