//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayByPierce.Masters
{
	public static class Utilities 
	{
		/// <summary>
		/// Returns false and if specified target variable is null. ErrorLogs the missing component if log is true.
		/// </summary>
		public static bool Validate(this MonoBehaviour monoBehaviour, object target, bool log = false) {
			if(target == null) 
			{
				if (log)
				{
					Debug.LogError(monoBehaviour.name + ": Is missing component reference to: " + target.GetType().Name, monoBehaviour);
				}
				return false;
			}
			return true;
		}
		/// <summary>
		/// Returns false and if specified target variable is null. ErrorLogs the missing component if log is true.
		/// </summary>
		public static bool Validate(this MonoBehaviour monoBehaviour, object[] targets, bool log = false) {
			bool check = true;
			foreach (var target in targets)
			{
				if (!Validate(monoBehaviour, target, log))
				{
					check = false;
				}
			}
			return check;
		}
		///	<summary>
		/// Returns true if float is less than ANY of the Vector3's values (x,y,z)
		/// </summary>
		public static bool FloatLessVector3(float f, Vector3 v3)
		{
			if (f < v3.x || f < v3.y || f < v3.z)
			{
				return true;
			}
			return false;
		}
		///	<summary>
		/// Returns true if float is greater than ANY of the Vector3's values (x,y,z)
		/// </summary>
		public static bool FloatGreaterVector3(float f, Vector3 v3)
		{
			if (f > v3.x || f > v3.y || f > v3.z)
			{
				return true;
			}
			return false;
		}
		///	<summary>
		/// Returns true if float is less than or equal to ANY of the Vector3's values (x,y,z)
		/// </summary>
		public static bool FloatLessEqualVector3(float f, Vector3 v3)
		{
			if (f <= v3.x || f <= v3.y || f <= v3.z)
			{
				return true;
			}
			return false;
		}
		///	<summary>
		/// Returns true if float is greater than or equal to ANY of the Vector3's values (x,y,z)
		/// </summary>
		public static bool FloatGreaterEqualVector3(float f, Vector3 v3)
		{
			if (f >= v3.x || f >= v3.y || f >= v3.z)
			{
				return true;
			}
			return false;
		}
		///	<summary>
		/// Returns true if the layer parameter is contained within the layermask object
		/// </summary>
		public static bool Contains(this LayerMask mask, int layer)
    {
      return mask == (mask | (1 << layer));
    }
		///	<summary>
		/// Returns an array of child transforms of Gameobject
		/// </summary>
    public static GameObject[] GetChildren(this GameObject go)
    {
      GameObject[] children = new GameObject[go.transform.childCount];
			for (int i = 0; i < go.transform.childCount; i++)
			{
				children[i] = go.transform.GetChild(i).gameObject;
			}
      return children;
    }
    /// <summary>
    /// Use this method to randomize any list
    /// </summary>
    public static List<T> Shuffle<T>(this List<T> list)
    {
			return list.OrderBy(x => UnityEngine.Random.value).ToList();
    }
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
			Vector3 AB = b - a;
			Vector3 AV = value - a;
			return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }
	}
}
