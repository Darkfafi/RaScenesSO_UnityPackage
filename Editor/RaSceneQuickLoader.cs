using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RaScenesSO.Editors
{
	[InitializeOnLoad]
	public class RaSceneQuickLoader : EditorWindow
	{
		#region Constants

		private const string PREV_SCENE_PREF = "RaScene_QuickLoader_PrevScene";
		private const string ACTIVE_PREF = "RaScene_QuickLoader_IsActive";

		#endregion

		#region Private Variables

		private string[] _scenePaths;
		private string[] _sceneNames;
		private int _selectedIndex = 0;

		#endregion

		#region Initialization

		static RaSceneQuickLoader()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		[MenuItem("Tools/RaScenesSO/QuickLoader")]
		public static void ShowWindow()
		{
			GetWindow<RaSceneQuickLoader>("Quick Loader").Show();
		}

		private void OnEnable()
		{
			RefreshScenes();
		}

		#endregion

		#region GUI Methods

		private void OnGUI()
		{
			GUILayout.Space(10);
			GUILayout.Label("Play & Return", EditorStyles.boldLabel);
			GUILayout.Label("Temporarily play a target scene. You will automatically return to your current scene upon exiting Play Mode.", EditorStyles.wordWrappedLabel);
			GUILayout.Space(10);

			if (_sceneNames == null || _sceneNames.Length == 0)
			{
				EditorGUILayout.HelpBox("No scenes found in the Assets folder.", MessageType.Info);
				if (GUILayout.Button("Refresh"))
				{
					RefreshScenes();
				}
				return;
			}

			_selectedIndex = EditorGUILayout.Popup("Target Scene", _selectedIndex, _sceneNames);

			GUILayout.Space(15);
			GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
			if (GUILayout.Button("Play Scene & Return Here", GUILayout.Height(35)))
			{
				PlayTargetScene(_scenePaths[_selectedIndex]);
			}
			GUI.backgroundColor = Color.white;


			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("<- Return", GUILayout.Height(35)))
				{
					LoadPreviousScene();
				}

				if (GUILayout.Button("Load ->", GUILayout.Height(35)))
				{
					LoadTargetScene(_scenePaths[_selectedIndex]);
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			if (GUILayout.Button("Refresh Scene List"))
			{
				RefreshScenes();
			}
		}

		#endregion

		#region Public Methods


		public static void PlayTargetScene(string targetScenePath)
		{
			LoadTargetScene(targetScenePath);
			EditorApplication.isPlaying = true;
		}

		public static void LoadTargetScene(string targetScenePath)
		{
			if (EditorApplication.isPlaying)
			{
				return;
			}

			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				return;
			}

			string currentScenePath = EditorSceneManager.GetActiveScene().path;
			EditorPrefs.SetString(PREV_SCENE_PREF, currentScenePath);
			EditorPrefs.SetBool(ACTIVE_PREF, true);

			EditorSceneManager.OpenScene(targetScenePath, OpenSceneMode.Single);
		}

		public static void LoadPreviousScene()
		{
			if (EditorApplication.isPlaying)
			{
				return;
			}

			if (EditorPrefs.GetBool(ACTIVE_PREF, false))
			{
				EditorPrefs.SetBool(ACTIVE_PREF, false);

				string prevScene = EditorPrefs.GetString(PREV_SCENE_PREF, string.Empty);

				if (string.IsNullOrEmpty(prevScene))
				{
					EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
				}
				else
				{
					EditorSceneManager.OpenScene(prevScene, OpenSceneMode.Single);
				}
			}
		}


		#endregion

		#region Private Methods

		private void RefreshScenes()
		{
			string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
			List<string> paths = new List<string>();
			List<string> names = new List<string>();

			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				paths.Add(path);
				names.Add(System.IO.Path.GetFileNameWithoutExtension(path));
			}

			_scenePaths = paths.ToArray();
			_sceneNames = names.ToArray();
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredEditMode)
			{
				LoadPreviousScene();
			}
		}

		#endregion
	}
}