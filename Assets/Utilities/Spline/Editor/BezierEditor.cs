//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using UnityEditor;
using UnityEngine;

namespace PlayByPierce 
{
	/// <summary>
  /// class description
  /// </summary>
	[CustomEditor(typeof(Bezier))]
	public class BezierEditor : Editor 
	{
		#region Public Fields
		#endregion


		#region Private Fields
		private Bezier bezier;
		private Transform handleTransform;
		private Quaternion handleRotation;
		private float guiScale = 0.5f;
		#endregion


		#region Bookkeeping 
		private const int lineSteps = 10;
		private const float directionScale = 0.5f;
		private const int stepsPerCurve = 10;
		private const float handleSize = 0.04f;
		private const float pickSize = 0.06f;
		private int selectedIndex = -1;
		private static Color[] modeColors = {
			Color.white,
			Color.yellow,
			Color.cyan
		};
		#endregion

		#region Initilization 
		#endregion



		#region Core 
		public override void OnInspectorGUI () 
		{
			bezier = target as Bezier;
			EditorGUI.BeginChangeCheck();
			bool loop = EditorGUILayout.Toggle("Loop", bezier.Loop);
			if (EditorGUI.EndChangeCheck()) 
			{
				Undo.RecordObject(bezier, "Toggle Loop");
				EditorUtility.SetDirty(bezier);
				bezier.Loop = loop;
			}
			if (selectedIndex >= 0 && selectedIndex < bezier.ControlPointCount) 
			{
				DrawSelectedPointInspector();
			}
			if (GUILayout.Button("Add Curve")) 
			{
				Undo.RecordObject(bezier, "Add Curve");
				bezier.AddCurve();
				EditorUtility.SetDirty(bezier);
			}
		}
		private void DrawSelectedPointInspector() 
		{
			GUILayout.Label("Selected Point");
			EditorGUI.BeginChangeCheck();
			Vector3 point = EditorGUILayout.Vector3Field("Position", bezier.GetControlPoint(selectedIndex));
			if (EditorGUI.EndChangeCheck()) 
			{
				Undo.RecordObject(bezier, "Move Point");
				EditorUtility.SetDirty(bezier);
				bezier.SetControlPoint(selectedIndex, point);
			}
			EditorGUI.BeginChangeCheck();
			ControlPointMode mode = (ControlPointMode)
				EditorGUILayout.EnumPopup("Mode", bezier.GetControlPointMode(selectedIndex));
			if (EditorGUI.EndChangeCheck()) 
			{
				Undo.RecordObject(bezier, "Change Point Mode");
				bezier.SetControlPointMode(selectedIndex, mode);
				EditorUtility.SetDirty(bezier);
			}
		}
		private void OnSceneGUI () 
		{
			bezier = target as Bezier;

			handleTransform = bezier.transform;
			handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

			Vector3 p0 = ShowPoint(0);
			for (int i = 1; i < bezier.ControlPointCount; i += 3) {
				Vector3 p1 = ShowPoint(i);
				Vector3 p2 = ShowPoint(i + 1);
				Vector3 p3 = ShowPoint(i + 2);
				
				Handles.color = Color.gray;
				Handles.DrawLine(p0, p1);
				Handles.DrawLine(p2, p3);
				
				Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
				p0 = p3;
			}
			ShowDirections();
		}
		#endregion

		#region Helper 
    bool PlusButton(Vector3 position, Quaternion direction)
    {
			Handles.color = new Color(0, 0.8f, 0, 1f);
			return Handles.Button(position, direction, 0.25f * guiScale, 0.25f * guiScale, Handles.DotHandleCap);
    }
		private Vector3 ShowPoint (int index) 
		{
			Vector3 point = handleTransform.TransformPoint(bezier.GetControlPoint(index));
			float size = HandleUtility.GetHandleSize(point);
			if (index == 0) 
			{
				size *= 2f;
			}
			Handles.color = modeColors[(int)bezier.GetControlPointMode(index)];
			if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap)) 
			{
				selectedIndex = index;
				Repaint();
			}
			if (selectedIndex == index) 
			{
				EditorGUI.BeginChangeCheck();
				point = Handles.DoPositionHandle(point, handleRotation);
				if (EditorGUI.EndChangeCheck()) 
				{
					Undo.RecordObject(bezier, "Move Point");
					EditorUtility.SetDirty(bezier);
					bezier.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
				}
			}
			return point;
		}
		private void ShowDirections () 
		{
			Handles.color = Color.green;
			Vector3 point = bezier.GetPoint(0f);
			Handles.DrawLine(point, point + bezier.GetDirection(0f) * directionScale);
			int steps = stepsPerCurve * bezier.CurveCount;
			for (int i = 1; i <= steps; i++) 
			{
				point = bezier.GetPoint(i / (float)steps);
				Handles.DrawLine(point, point + bezier.GetDirection(i / (float)steps) * directionScale);
			}
		}
		#endregion
	}
}
