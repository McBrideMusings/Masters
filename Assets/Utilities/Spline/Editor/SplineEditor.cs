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
	[CustomEditor(typeof(Spline))]
	public class SplineEditor : Editor 
	{
		#region Public Fields
		private Spline spline;
		private Transform handleTransform;
		private Quaternion handleRotation;
		private const float guiSize = 0.04f;
		private const float pickGuiSize = 0.06f;
		private float guiScale;
		private int selectedIndex = -1;
		/* 
		private static Color[] modeColors = {
			Color.white,
			Color.yellow,
			Color.cyan
		};
		*/
		#endregion


		#region Private Fields
		#endregion


		#region Bookkeeping 

		#endregion


		#region Initilization 
		void OnEnable() 
		{
			spline = (Spline)target;

    	if (Application.isPlaying) return;
			if (spline.PointsLength == 0)
			{
				spline.Reset();
			}
		}
    /// <summary>
    /// Summary
    /// </summary>
    /// <param name="param">Param Description</param>
    /// <returns>Return Description</returns>
		void Awake() 
		{
			
		}
		#endregion

		#region Inspector 
		public override void OnInspectorGUI () 
		{
			spline = target as Spline;

			if (GUILayout.Button("Reset")) 
			{
				Undo.RecordObject(spline, "Reset");
				EditorUtility.SetDirty(spline);
				spline.Reset();
			}
			/* 
			EditorGUI.BeginChangeCheck();
			bool loop = EditorGUILayout.Toggle("Loop", spline.loop);
			if (EditorGUI.EndChangeCheck()) 
			{
				Undo.RecordObject(spline, "Toggle Loop");
				EditorUtility.SetDirty(spline);
				spline.loop = loop;
			}
			*/
			if (selectedIndex >= 0 && selectedIndex < spline.PointsLength) 
			{
				DrawSelectedPointInspector();
			}
		}
		private void DrawSelectedPointInspector() 
		{
			GUILayout.Label("Selected Point");
			EditorGUI.BeginChangeCheck();
			Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetPoint(selectedIndex));
			if (EditorGUI.EndChangeCheck()) 
			{
				Undo.RecordObject(spline, "Move Point");
				EditorUtility.SetDirty(spline);
				spline.SetPoint(selectedIndex, point);
			}

			EditorGUI.BeginChangeCheck();
			ControlMode mode = (ControlMode) EditorGUILayout.EnumPopup("Mode", spline.GetPointMode(selectedIndex));
			if (EditorGUI.EndChangeCheck()) 
			{
				Undo.RecordObject(spline, "Change Point Mode");
				spline.SetPointMode(selectedIndex, mode);
				EditorUtility.SetDirty(spline);
			}

			if (selectedIndex != 0 && selectedIndex != spline.PointsLength - 1)
			{
				if (GUILayout.Button("Delete Point")) 
				{
					Undo.RecordObject(spline, "Delete Point");
					EditorUtility.SetDirty(spline);
					spline.DeletePoint(point);
				}
			}
		}
		#endregion

		#region Scene 
		private void OnSceneGUI () 
		{
			spline = target as Spline;

			handleTransform = spline.transform;
			handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;
			
			Vector3 p0 = handleTransform.TransformPoint(spline.GetPoint(0));
			for (int i = 1; i < spline.PointsLength; i += 3) {
				Vector3 p1 = RenderPoint(i);
				Vector3 p2 = RenderPoint(i + 1);
				Vector3 p3 = RenderPoint(i + 2);
				
				Handles.color = Color.gray;
				Handles.DrawLine(p0, p1);
				Handles.DrawLine(p2, p3);
				
				Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
				if (RenderAddButton(p0 + (p3 - p0)/2, handleRotation))
				{
					Undo.RecordObject(spline, "Add Point");
					EditorUtility.SetDirty(spline);
					spline.InsertPoint(i, handleTransform.InverseTransformPoint(p0), handleTransform.InverseTransformPoint(p3));
					selectedIndex = i;
				}
				p0 = p3;
			}
		}
		private Vector3 RenderPoint(int index) 
		{
			Vector3 point = handleTransform.TransformPoint(spline.GetPoint(index));

			guiScale = HandleUtility.GetHandleSize(point);
			if (index == 0 || index == spline.PointsLength - 1) 
			{
				Handles.color = Color.blue;
				guiScale *= 2f;
			}

			Handles.color = Color.white;
			if (Handles.Button(point, handleRotation, guiScale * guiSize, guiScale * pickGuiSize, Handles.DotHandleCap)) 
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
					Undo.RecordObject(spline, "Move Point");
					EditorUtility.SetDirty(spline);
					spline.SetPoint(index, handleTransform.InverseTransformPoint(point));
				}
			}
			return point;
		}
		bool RenderAddButton(Vector3 position, Quaternion direction)
    {
      Handles.color = new Color(0, 0.8f, 0, 1f);
			guiScale = HandleUtility.GetHandleSize(position);
      //Handles.DotHandleCap(0, position, direction, 0.2f * guiScale);
      return Handles.Button(position, direction, guiScale * guiSize, guiScale * pickGuiSize, Handles.DotHandleCap);
    }
		#endregion

		#region Helper 
		#endregion
	}
}
