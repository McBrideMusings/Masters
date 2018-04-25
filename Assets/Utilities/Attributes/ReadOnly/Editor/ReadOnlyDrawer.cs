using UnityEngine;
using UnityEditor;

/// <summary>
/// A property drawer that can be used for read-only fields in the inspector. 
/// </summary>
/// <seealso cref="UnityEditor.PropertyDrawer" />
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {   
        //GUI.enabled = false;
        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndDisabledGroup();
        //GUI.enabled = true;
    }
}