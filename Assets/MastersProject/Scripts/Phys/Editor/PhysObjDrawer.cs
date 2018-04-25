//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlayByPierce.Phys
{
	/// <summary>
  /// class description
  /// </summary>
	//[CustomPropertyDrawer(typeof(PhysObj))]
	public class PhysObjDrawer : PropertyDrawer 
	{
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) 
		{
			//EditorGUI.PropertyField(position, property, label, true);
			/* 
			int orgIndentLevel = EditorGUI.indentLevel;

			string propertyName = "Add a Rigidbody!";
			if (property.FindPropertyRelative("rigidbody").objectReferenceValue)
			{
				propertyName = property.FindPropertyRelative("rigidbody").objectReferenceValue.name;
			}

			label = EditorGUI.BeginProperty(position, new GUIContent(propertyName), property);
			Rect contentPosition = EditorGUI.PrefixLabel(position, label);
			EditorGUIUtility.labelWidth = 14f;
			// Calculate rects
			//Rect nameRect = new Rect (contentPosition.x, contentPosition.y, 30, contentPosition.height);
			Rect rigidbodyRect = new Rect (contentPosition.x+15, contentPosition.y, contentPosition.width, contentPosition.height);
			Rect targetRect = new Rect (contentPosition.x+30, contentPosition.y, contentPosition.width, contentPosition.height);
			Rect deleteRect = new Rect (contentPosition.x+45, contentPosition.y, contentPosition.width, contentPosition.height);
			
			// Rigidbody
			EditorGUI.BeginChangeCheck ();
			EditorGUI.PropertyField(rigidbodyRect,property.FindPropertyRelative("rigidbody"),new GUIContent("RB"));
			EditorGUI.EndChangeCheck();

			// Target
			EditorGUI.BeginChangeCheck ();
			EditorGUI.PropertyField(targetRect,property.FindPropertyRelative("target"),new GUIContent("T"));
			EditorGUI.EndChangeCheck();

			// Delete "Button"
			EditorGUI.BeginChangeCheck ();
			EditorGUI.PropertyField(deleteRect,property.FindPropertyRelative("toDelete"),new GUIContent("X"));
			EditorGUI.EndChangeCheck();

			EditorGUI.EndProperty();
			EditorGUI.indentLevel = orgIndentLevel;
			*/
		}
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      return EditorGUI.GetPropertyHeight(property);
    }
	}
}
