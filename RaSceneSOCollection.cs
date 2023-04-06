using NestedSO;
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

#if UNITY_EDITOR

		protected void OnValidate()
		{
			UnityEditor.EditorBuildSettingsScene[] scenes = new UnityEditor.EditorBuildSettingsScene[Count];
			for(int i = 0; i < Count; i++)
			{
				RaSceneSO sceneConfig = this[i];
				scenes[i] = new UnityEditor.EditorBuildSettingsScene(sceneConfig.ScenePath, true);
			}
			UnityEditor.EditorBuildSettings.scenes = scenes;
		}

#endif
	}
}