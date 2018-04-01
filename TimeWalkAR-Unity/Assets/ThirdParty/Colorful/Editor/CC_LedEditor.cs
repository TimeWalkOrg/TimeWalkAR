using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CC_Led))]
public class CC_LedEditor : Editor
{
	SerializedProperty p_scale;
	SerializedProperty p_ratio;
	SerializedProperty p_automaticRatio;
	SerializedProperty p_brightness;
	SerializedProperty p_mode;

	static string[] modes = { "Resolution Independent", "Pixel Perfect" };

	void OnEnable()
	{
		p_scale = serializedObject.FindProperty("scale");
		p_ratio = serializedObject.FindProperty("ratio");
		p_automaticRatio = serializedObject.FindProperty("automaticRatio");
		p_brightness = serializedObject.FindProperty("brightness");
		p_mode = serializedObject.FindProperty("mode");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		p_mode.intValue = EditorGUILayout.Popup("Mode", p_mode.intValue, modes);
		EditorGUILayout.PropertyField(p_scale);
		EditorGUILayout.PropertyField(p_automaticRatio);

		if (!p_automaticRatio.boolValue)
			EditorGUILayout.PropertyField(p_ratio);

		EditorGUILayout.PropertyField(p_brightness);

		serializedObject.ApplyModifiedProperties();
    }
}
