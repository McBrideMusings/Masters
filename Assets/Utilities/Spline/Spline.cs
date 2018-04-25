//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayByPierce 
{
	public enum ControlMode { Free, Aligned, Mirrored }
	/// <summary>
  /// class description
  /// </summary>
	public class Spline : MonoBehaviour 
	{
		#region Public Fields
		public bool loop;
		#endregion


		#region Private Fields
		private List<Vector3> points = new List<Vector3>();
		private List<ControlMode> modes = new List<ControlMode>();
		#endregion


		#region Bookkeeping 
		#endregion

		#region Properties 
		public int PointsLength { get { return points.Count; } }
		public int CurveCount { get { return (points.Count - 1) / 3; } }
		public bool Loop 
		{
			get { return loop; }
			set 
			{
				loop = value;
				if (value == true) 
				{
					modes[modes.Count - 1] = modes[0];
					SetPoint(0, points[0]);
				}
			}
		}
		#endregion

		#region Initilization 
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
		private void EnforceMode (int index) 
		{
			int modeIndex = (index + 1) / 3;
			ControlMode mode = modes[modeIndex];
			if (mode == ControlMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Count - 1)) 
			{
				return;
			}

			int middleIndex = modeIndex * 3;
			int fixedIndex, enforcedIndex;
			if (index <= middleIndex) 
			{
				fixedIndex = middleIndex - 1;
				if (fixedIndex < 0) 
				{
					fixedIndex = points.Count - 2;
				}
				enforcedIndex = middleIndex + 1;
				if (enforcedIndex >= points.Count) 
				{
					enforcedIndex = 1;
				}
			}
			else 
			{
				fixedIndex = middleIndex + 1;
				if (fixedIndex >= points.Count) 
				{
					fixedIndex = 1;
				}
				enforcedIndex = middleIndex - 1;
				if (enforcedIndex < 0) 
				{
					enforcedIndex = points.Count - 2;
				}
			}

			Vector3 middle = points[middleIndex];
			Vector3 enforcedTangent = middle - points[fixedIndex];
			if (mode == ControlMode.Aligned) 
			{
				enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
			}
			points[enforcedIndex] = middle + enforcedTangent;
		}
		#endregion

		#region Helper 
		public Vector3 GetPoint(int index) 
		{ 
			return points[index]; 
		}
		public Vector3 GetLocalPoint(int index) 
		{ 
			return transform.InverseTransformPoint(points[index]); 
		}
		public Vector3 GetDirection(float t) 
		{
			return GetVelocity(t).normalized;
		}
		public Vector3 GetVelocity(float t) 
		{
			int i;
			if (t >= 1f) {
				t = 1f;
				i = points.Count - 4;
			}
			else {
				t = Mathf.Clamp01(t) * CurveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}
			return transform.TransformPoint(GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
		}
		public ControlMode GetPointMode(int index) 
		{
			return modes[(index + 1) / 3];
		}
		public void SetPoint(int index, Vector3 point) 
		{ 
			points[index] = point;
			if (index % 3 == 0) {
				Vector3 delta = point - points[index];
				if (loop) {
					if (index == 0) {
						points[1] += delta;
						points[points.Count - 2] += delta;
						points[points.Count - 1] = point;
					}
					else if (index == points.Count - 1) {
						points[0] = point;
						points[1] += delta;
						points[index - 1] += delta;
					}
					else {
						points[index - 1] += delta;
						points[index + 1] += delta;
					}
				}
				else {
					if (index > 0) {
						points[index - 1] += delta;
					}
					if (index + 1 < points.Count) {
						points[index + 1] += delta;
					}
				}
			}
			points[index] = point; 
			EnforceMode(index);
		}
		public void SetPointMode (int index, ControlMode mode) 
		{
			int modeIndex = (index + 1) / 3;
			modes[modeIndex] = mode;
			if (loop) 
			{
				if (modeIndex == 0) 
				{
					modes[modes.Count - 1] = mode;
				}
				else if (modeIndex == modes.Count - 1) 
				{
					modes[0] = mode;
				}
			}
			EnforceMode(index);
		}
		public void InsertPoint(int index, Vector3 p0, Vector3 p1) 
		{
			points.Insert(index, p0 + (p1 - p0)*0.4f);
			points.Insert(index+2, p0 + (p1 - p0)*0.6f);
			points.Insert(index+1, p0 + (p1 - p0)*0.5f);

			modes.Add(modes[modes.Count - 1]);
			EnforceMode(points.Count - 4);
			
			if (loop) 
			{
				points[points.Count - 1] = points[0];
				modes[modes.Count - 1] = modes[0];
				EnforceMode(0);
			}
		}
		public void DeletePoint(int index) 
		{ 
			points.RemoveAt(index);
		}
		public void DeletePoint(Vector3 point) 
		{ 
			points.Remove(point);
		}
		public Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			t = Mathf.Clamp01(t);
			float oneMinusT = 1f - t;
			return
				3f * oneMinusT * oneMinusT * (p1 - p0) +
				6f * oneMinusT * t * (p2 - p1) +
				3f * t * t * (p3 - p2);
		}
		public void Reset() 
		{
			points = new List<Vector3>()
			{
				new Vector3(0f, 0f, 0f),
				new Vector3(1f, 0f, 0f),
				new Vector3(2f, 0f, 0f),
				new Vector3(3f, 0f, 0f)
			};
			modes = new List<ControlMode>()
			{
				ControlMode.Free,
				ControlMode.Free
			};
		}
		#endregion
	}
}
