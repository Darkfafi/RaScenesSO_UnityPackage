using UnityEditor;
using UnityEngine;

namespace RaScenesSO.Editors
{
	[CustomPropertyDrawer(typeof(SceneReference))]
	public class SceneReferencePropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			var scenePathProp = property.FindPropertyRelative("_scenePath");
			string scenePath = scenePathProp.stringValue;

			var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

			EditorGUI.BeginChangeCheck();
			var newScene = EditorGUI.ObjectField(position, "Scene", oldScene, typeof(SceneAsset), false) as SceneAsset;

			if(EditorGUI.EndChangeCheck())
			{
				var newPath = AssetDatabase.GetAssetPath(newScene);
				scenePathProp.stringValue = newPath;
			}
			EditorGUI.EndProperty();
		}
	}
}