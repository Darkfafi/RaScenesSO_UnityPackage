using RaModelsSO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RaScenesSO
{
	public class RaSceneModelSO : RaModelSOBase
	{
		#region Events

		public delegate void SceneHandler(RaSceneSO scene);
		public event SceneHandler SceneLoadStartedEvent;
		public event SceneHandler SceneLoadEndedEvent;

		#endregion

		#region Editor Variables

		[SerializeField]
		private RaSceneSO _loadingScreenScene = null;

		[SerializeField]
		private RaSceneSOCollection _sceneConfigCollection = null;

		#endregion

		#region Properties

		public RaSceneSO CurrentScene
		{
			get; private set;
		}

		public RaSceneSO PreviousScene
		{
			get; private set;
		}

		public RaSceneSO NextScene
		{
			get; private set;
		}

		public bool IsLoading => NextScene != null;

		#endregion

		#region Public Methods

		public void LoadScene(string sceneName)
		{
			if(_sceneConfigCollection.TryGetSceneConfig(sceneName, out RaSceneSO config))
			{
				LoadScene(config);
			}
		}

		public void LoadScene(RaSceneSO sceneConfig)
		{
			if(!IsLoading)
			{
				PreviousScene = CurrentScene;
				NextScene = sceneConfig;
				SceneManager.LoadSceneAsync(_loadingScreenScene.SceneName, LoadSceneMode.Additive);
			}
		}

		public Scene GetCurrentUnitySceneData()
		{
			return SceneManager.GetActiveScene();
		}

		#endregion

		internal void Loader_StartLoadingScene()
		{
			if(IsLoading)
			{
				SceneLoadStartedEvent?.Invoke(NextScene);
			}
		}

		internal void Loader_RefreshCurrentScene()
		{
			if(IsLoading)
			{
				CurrentScene = NextScene;
			}
		}

		internal void Loader_End()
		{
			if(IsLoading)
			{
				RaSceneSO loadedScene = NextScene;
				NextScene = null;
				SceneLoadEndedEvent?.Invoke(loadedScene);
			}
		}

		#region Protected Methods

		protected override void OnInit()
		{
			CurrentScene = _sceneConfigCollection.GetActiveSceneConfig();
			PreviousScene = null;
			NextScene = null;
		}

		protected override void OnDeinit()
		{
			CurrentScene = null;
			PreviousScene = null;
			NextScene = null;
			SceneLoadEndedEvent = null;
			SceneLoadStartedEvent = null;
		}

		#endregion
	}
}