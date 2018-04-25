//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PlayByPierce.Phys
{
	/// <summary>
  /// Partially completed Editor script for editing mass
  /// </summary>
	[CustomEditor(typeof(PhysMass))]
	public class PhysMassEditor : Editor 
	{
		#region Public Fields
		public PhysMass physMass;
		#endregion

		#region Private Fields
		private SerializedProperty[] customPhysMassDrawer;
		#endregion

		#region Bookkeeping
		public bool showDefaultInspector = true;
		#endregion

		#region Initialization
    void OnEnable()
    {
      ///customPhysMassDrawer = serializedObject.FindProperty
		}
		#endregion

		#region UI 
    public override void OnInspectorGUI()
    {
			physMass = (PhysMass)target;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Total Mass");
			EditorGUILayout.LabelField(physMass.totalMass.ToString());
			EditorGUILayout.EndHorizontal();	

			physMass.renderMass = EditorGUILayout.Toggle(new GUIContent("Render Mass", ""), physMass.renderMass); 

			serializedObject.Update();
			EditorUtilities.Show(serializedObject.FindProperty("bodyMasses"), false);
			serializedObject.ApplyModifiedProperties();
			/* 
			if (physMass.bodyMasses != null && physMass.bodyMasses.Length > 0)
			{
				foreach (BodyMass bodyMass in physMass.bodyMasses)
				{

					//customPhysMassDrawer = physMass.FindProperty(bodyMass);
					//EditorGUILayout.BeginHorizontal(); 
					//GUILayout.Space(10); 
					//bodyMass.MassPercent = EditorGUILayout.FloatField(new GUIContent("Body Mass Percent", "The percent of totalMass that will be distributed to this joint"), bodyMass.MassPercent); 
					//EditorGUILayout.EndHorizontal();						
				}
			}
			*/
			//DrawDefaultInspector();
			/*
			if (showDefaultInspector = EditorGUILayout.Foldout(showDefaultInspector, "Default Inspector"))
			{
				DrawDefaultInspector();
			}
			EditorGUILayout.Separator();
			 */
		}
		#endregion

		#region Helper
		#endregion
	}
}
