//————————— PlayByPierce - PROJECT ———————————————————————————————————————————
// Purpose: Write Purpose Here
//————————————————————————————————————————————————————————————————————————————
using UnityEditor;
using UnityEngine;

namespace PlayByPierce 
{
	/// <summary>
	/// A property drawer for fields marked with the NonNegative Attribute.
	/// </summary>
	[CustomPropertyDrawer(typeof(MinAttribute))]
	public class MinDrawer : PropertyDrawer 
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var minAttribute = (MinAttribute) attribute;

			switch (property.propertyType)
			{
				case SerializedPropertyType.Float:
					{
						EditorGUI.BeginChangeCheck();

						float x = EditorGUI.FloatField(position, label, property.floatValue);

						if (EditorGUI.EndChangeCheck() && x > minAttribute.minValue) property.floatValue = x;
					}
					break;
				case SerializedPropertyType.Integer:
					{
						EditorGUI.BeginChangeCheck();

						int x = EditorGUI.IntField(position, label, property.intValue);

						if (EditorGUI.EndChangeCheck() && x > minAttribute.minValue) property.intValue = x;
					}
					break;
				default:
					EditorGUI.LabelField(position, label.text, "Use Min with float or Int");
					break;
			}
		}
	}
}
