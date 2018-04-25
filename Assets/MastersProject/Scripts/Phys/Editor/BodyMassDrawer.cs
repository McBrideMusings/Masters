//———————————— PlayByPierce ——————————————————————————————————————————————————
// Project:    MastersProject
// Author:     Pierce R McBride
//————————————————————————————————————————————————————————————————————————————
using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayByPierce.Phys
{
	/// <summary>
  /// Partially completed script that lets me visualize and adjust the mass of a given ragdoll proportionally
  /// </summary>
	[CustomPropertyDrawer(typeof(BodyMass))]
	public class BodyMassDrawer : PropertyDrawer 
	{
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) 
		{
			int orgIndentLevel = EditorGUI.indentLevel;

			label = EditorGUI.BeginProperty(position, new GUIContent(property.FindPropertyRelative("rigidbody").objectReferenceValue.name), property);
			Rect contentPosition = EditorGUI.PrefixLabel(position, label);
			if (position.height > 16f) 
			{
				position.height = 16f;
				EditorGUI.indentLevel += 1;
				contentPosition = EditorGUI.IndentedRect(position);
				contentPosition.y += 18f;
			}

			EditorGUI.BeginChangeCheck ();
			EditorGUI.PropertyField(contentPosition,property.FindPropertyRelative("mass"),new GUIContent("Mass"));
			property.FindPropertyRelative("hasChanged").boolValue = EditorGUI.EndChangeCheck();

			EditorGUI.EndProperty();
			EditorGUI.indentLevel = orgIndentLevel;
		}
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) 
		{
			return Screen.width < 333 ? (16f + 18f) : 16f;
		}
	}
}
