using System;
using UnityEngine;
// http://answers.unity3d.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html
/// <summary>
/// Used to mark inspectable fields as read-only (that is, making them uneditable, even if they are visible).
/// </summary>
/// <seealso cref="UnityEngine.PropertyAttribute" />
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ReadOnlyAttribute : PropertyAttribute {}