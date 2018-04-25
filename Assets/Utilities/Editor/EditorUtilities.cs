//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using UnityEngine;
using UnityEditor;

namespace PlayByPierce
{
	/// <summary>
  /// class description
  /// </summary>
	public static class EditorUtilities 
	{
		public static void Show (SerializedProperty list, bool showListSize = true, bool showListLabel = true) 
		{
			if (showListLabel) 
			{
				EditorGUILayout.PropertyField(list);
				EditorGUI.indentLevel += 1;
			}
			if (!showListLabel || list.isExpanded)
			{
				if (showListSize) 
				{
					EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
				}
				for (int i = 0; i < list.arraySize; i++) 
				{
					EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
				}
			}
			if (showListLabel) 
			{
				EditorGUI.indentLevel -= 1;
			}
		}
	}
}
