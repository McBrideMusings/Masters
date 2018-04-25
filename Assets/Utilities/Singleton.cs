//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using UnityEngine;

namespace PlayByPierce
{
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour 
	{
		#region Private Fields
		protected static T instance;
		private static bool applicationIsQuitting = false;
		#endregion

		/// <summary>
		/// Returns the instance of this singleton.
		/// </summary>
		public static T Instance
		{
			get
			{
				if (applicationIsQuitting)
				{
					Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit.");
					return null;
				}
				if (instance == null)
				{
					instance = (T)FindObjectOfType(typeof(T));

					if (instance == null)
					{
						Debug.LogError("An instance of " + typeof(T) + " is needed in the scene, but there is none.");
					}
				}
				return instance;
			}
		}

		public virtual void Awake() 
		{
			if (instance == null) 
			{
				instance = this as T;
				DontDestroyOnLoad(this.gameObject);
			} 
			else 
			{
				Destroy(gameObject);
				return;
			}
		}

		/// <summary>
		/// When Unity quits, it destroys objects in a random order.
		/// In principle, a Singleton is only destroyed when application quits.
		/// If any script calls Instance after it have been destroyed, 
		///   it will create a buggy ghost object that will stay on the Editor scene
		///   even after stopping playing the Application. Really bad!
		/// So, this was made to be sure we're not creating that buggy ghost object.
		/// </summary>
		public void OnDestroy()
		{
			applicationIsQuitting = true;
		}
	}
}