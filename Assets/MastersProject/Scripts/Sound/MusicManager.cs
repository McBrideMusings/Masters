//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Whatever that framework was
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace PlayByPierce.Masters
{
	/// <summary>
  /// class description
  /// </summary>
	[Serializable]
	public class MusicPlaylist {
    public bool Shuffle;
    public List<AudioClip> MusicList;
	}
	/// <summary>
  /// class description
  /// </summary>
	[RequireComponent(typeof (AudioSource))]
	public class MusicManager : MonoBehaviour 
	{
		#region Config Fields
    public List<MusicPlaylist> playlists = new List<MusicPlaylist>();

		[Header("Config")]
    public bool ShufflePlaylists;
    public bool RepeatPlaylists;
		public float fadeDuration = 0f;
		public bool playOnAwake = true;
		#endregion

		#region State Fields
		[Space(10f)]
		[SerializeField] [ReadOnly] private bool initilized;
		[SerializeField] [ReadOnly] private int playlistIndex;
		[SerializeField] [ReadOnly] private int songIndex;
		private AudioSource source;
		#endregion

		#region Properties
		public float Volume
    {
			get
			{
				return source.volume;
			}
			set
			{
				source.volume = value;
			}
    }
		#endregion

		#region Initialization
		/// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		protected void Awake() 
		{
			if (playlists.Count <= 0)
			{
				initilized = false;
				return;
			}
			// Get Required References
			source = gameObject.GetComponent<AudioSource>();
			// Set Initial Values	
			source.playOnAwake = false;
			if (fadeDuration > 0f)  
			{
				Volume = 0f;
			}
			else
			{
				Volume = 1f;
			}
			// Shuffle Playlists
			if (ShufflePlaylists && playlists.Count > 1)
			{
				playlists = playlists.Shuffle();
			}
			// Start playlist play
			if (playOnAwake) PlayAllTracks();
			initilized = true;
		}
		#endregion

		#region Core
    /// <summary>
    /// Use this method to play all possible tracks
    /// </summary>
    public void PlayAllTracks()
    {
			StopAllCoroutines();
			// Execute playlist with first element
			playlistIndex = 0;
			StartCoroutine(PlayPlaylist(playlists[playlistIndex]));
    }
    private IEnumerator PlayPlaylist(MusicPlaylist targetPlaylist)
    {
			// Shuffle target playlist if it is required
			if(targetPlaylist.Shuffle)
			{
				targetPlaylist.MusicList = targetPlaylist.MusicList.Shuffle();
			}

			// Execute target playlist until it finishes
			for (songIndex = 0; songIndex < targetPlaylist.MusicList.Count; songIndex++)
			{
				yield return StartCoroutine(PlaySongE(targetPlaylist.MusicList[songIndex]));
			}

			playlistIndex++;
			if(playlists.Count < playlistIndex)
			{
				StartCoroutine(PlayPlaylist(playlists[playlistIndex]));
			}
			// Otherwise, and if this system is set to loop, restart the play cycle
			else
			{
				playlistIndex = 0;
				StartCoroutine(PlayPlaylist(playlists[playlistIndex]));
			}
    }
    private IEnumerator PlaySongE(AudioClip clip)
    {
			source.Stop();
			source.clip = clip;
			source.Play();
			StartCoroutine(FadeIn());
			while (source.isPlaying)
			{
				if (source.clip.length - source.time <= fadeDuration)
				{
					yield return StartCoroutine(FadeOut());
				}
				yield return null;
			}
    }
		#endregion

		#region Helper
    private IEnumerator FadeOut()
    {
			if (fadeDuration > 0f)
			{
				float startTime = source.clip.length - fadeDuration;
				float lerpValue = 0f;
				while (lerpValue < 1f && source.isPlaying)
				{
					lerpValue = Mathf.InverseLerp(startTime, source.clip.length, source.time);
					Volume = Mathf.Lerp(Volume, 0f, lerpValue);
					yield return null;
				}
				Volume = 0f;
			}
    }
		private IEnumerator FadeIn()
    {
			if (fadeDuration > 0f)
			{
				var lerpValue = 0f;
				while (lerpValue < 1f && source.isPlaying)
				{
					lerpValue = Mathf.InverseLerp(0f, fadeDuration, source.time);
					Volume = Mathf.Lerp(0f, Volume, lerpValue);
					yield return null;
				}
				Volume = 1f;
			}
    }
		#endregion
	}
}
