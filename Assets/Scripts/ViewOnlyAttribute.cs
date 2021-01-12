using UnityEngine;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

//Note: NEVER put it in an Editor folder.

[AttributeUsage (AttributeTargets.Field,Inherited = true)]
public class ViewOnlyAttribute : PropertyAttribute {}

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer (typeof(ViewOnlyAttribute))]
public class ViewOnlyAttributeDrawer : UnityEditor.PropertyDrawer
{
	public override void OnGUI(Rect rect, UnityEditor.SerializedProperty prop, GUIContent label)
	{
		bool wasEnabled = GUI.enabled;
		GUI.enabled = false;
		UnityEditor.EditorGUI.PropertyField(rect,prop, true); //True to include children
		GUI.enabled = wasEnabled;
	}
}
#endif
