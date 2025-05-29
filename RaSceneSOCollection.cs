using NestedSO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RaScenesSO
{
	[UnityEngine.CreateAssetMenu(menuName = "RaScenesSO/Create " + nameof(RaSceneSOCollection), fileName = nameof(RaSceneSOCollection))]
	public class RaSceneSOCollection : NestedSOCollectionBase<RaSceneSO>
	{
		public bool TryGetSceneConfig(SceneReference sceneReference, out RaSceneSO sceneConfig)
		{
			return TryGetSceneConfig(sceneReference.GetSceneName(), out sceneConfig);
		}

		public bool GetSceneConfig(SceneReference sceneReference)
		{
			TryGetSceneConfig(sceneReference, out RaSceneSO config);
			return config;
		}

		public bool TryGetSceneConfig(string sceneName, out RaSceneSO sceneConfig)
		{
			return TryGetItem(x => x.SceneName == sceneName, out sceneConfig);
		}

		public bool GetSceneConfig(string sceneName)
		{
			TryGetSceneConfig(sceneName, out RaSceneSO config);
			return config;
		}

		public bool TryGetSceneConfig(Scene scene, out RaSceneSO sceneConfig)
		{
			return TryGetSceneConfig(scene.name, out sceneConfig);
		}

		public RaSceneSO GetSceneConfig(Scene scene)
		{
			TryGetSceneConfig(scene, out RaSceneSO config);
			return config;
		}

		public RaSceneSO GetActiveSceneConfig()
		{
			TryGetSceneConfig(SceneManager.GetActiveScene(), out RaSceneSO config);
			return config;
		}

		protected void Awake()
		{
			hideFlags = HideFlags.DontUnloadUnusedAsset;

#if UNITY_EDITOR
			OnValidate();
#endif
		}

		protected override void OnAddedAsset(RaSceneSO asset)
		{
			base.OnAddedAsset(asset);
#if UNITY_EDITOR
			asset.Editor_Setup();
			OnValidate();
#endif
		}

		protected override void OnRemovedAsset(RaSceneSO asset)
		{
#if UNITY_EDITOR
			OnValidate();
#endif
		}

		protected virtual void OnValidate()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				return;
			}
			
			UnityEditor.EditorBuildSettingsScene[] scenes = new UnityEditor.EditorBuildSettingsScene[Count];
			for(int i = 0; i < Count; i++)
			{
				RaSceneSO raSceneSO = this[i];
				scenes[i] = new UnityEditor.EditorBuildSettingsScene(raSceneSO.ScenePath, true);
			}
			UnityEditor.EditorBuildSettings.scenes = scenes;
#endif
		}


#if UNITY_EDITOR
		internal static void Editor_RefreshSceneSettings()
		{
			string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(RaSceneSOCollection)}");
			for(int i = 0; i < guids.Length; i++)
			{
				string pathToCollection = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
				if(!string.IsNullOrEmpty(pathToCollection))
				{
					RaSceneSOCollection collection = UnityEditor.AssetDatabase.LoadAssetAtPath<RaSceneSOCollection>(pathToCollection);
					if(collection != null)
					{
						collection.OnValidate();
					}
				}
			}
		}
#endif
	}
}