using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RaScenesSO.Editors
{
	[CustomEditor(typeof(RaSceneSO), editorForChildClasses: true)]
	public class SceneConfigEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			RaSceneSO raSceneSO = target as RaSceneSO;

			if (!string.IsNullOrEmpty(raSceneSO.ScenePath))
			{
				if (GUILayout.Button("Load"))
				{
					EditorSceneManager.OpenScene(raSceneSO.ScenePath);
				}

				if (GUILayout.Button("Launch"))
				{
					if (EditorApplication.isPlaying)
					{
						EditorApplication.isPlaying = false;
					}
					EditorSceneManager.OpenScene(raSceneSO.ScenePath);
					EditorApplication.isPlaying = true;
				}
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}