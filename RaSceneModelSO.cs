using System;
using System.Threading;
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
		private RaSceneLoaderBase _loaderPrefab = null;

		[SerializeField]
		private RaSceneSOCollection _sceneConfigCollection = null;

		#endregion

		#region  Variables

		private RaSceneLoaderBase _loaderInstance = null;
		private Scene _loadingScene = default;

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

				_loaderInstance = Instantiate(_loaderPrefab);
				SceneManager.MoveGameObjectToScene(_loaderInstance.gameObject, _loadingScene);

				LoadProcess();
			}
		}

		public Scene GetCurrentUnitySceneData()
		{
			return SceneManager.GetActiveScene();
		}

		#endregion

		#region Protected Methods

		protected override void OnInit()
		{
			_loadingScene = SceneManager.CreateScene(nameof(RaSceneModelSO));

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

		#region  Private Methods

		private void MarkStartLoadingScene()
		{
			if(IsLoading)
			{
				SceneLoadStartedEvent?.Invoke(NextScene);
			}
		}

		private void MarkRefreshCurrentScene()
		{
			if(IsLoading)
			{
				CurrentScene = NextScene;
			}
		}

		private void MarkEnd()
		{
			// Destroy Loader
			if(_loaderInstance != null)
			{
				Destroy(_loaderInstance.gameObject);
				_loaderInstance = null;
			}

			// Process End
			if(IsLoading)
			{
				RaSceneSO loadedScene = NextScene;
				NextScene = null;
				SceneLoadEndedEvent?.Invoke(loadedScene);
			}
		}

		private async void LoadProcess()
		{
			try
			{
				LoadingProcess unloadPreProcess;
				LoadingProcess unloadProcess;
				LoadingProcess unloadPostProcess;

				LoadingProcess loadPreProcess;
				LoadingProcess loadProcess;
				LoadingProcess loadPostProcess;

				LoadingProcess[] processess = new LoadingProcess[]
				{
					unloadPreProcess = new LoadingProcess(),
					unloadProcess = new LoadingProcess(),
					unloadPostProcess = new LoadingProcess(),


					loadPreProcess = new LoadingProcess(),
					loadProcess = new LoadingProcess(),
					loadPostProcess = new LoadingProcess()
				};

				using(LoadingInfo progressManager = new LoadingInfo(processess))
				{
					RaSceneSO oldScene = PreviousScene;
					RaSceneSO newScene = NextScene;

					MarkStartLoadingScene();

					_loaderInstance.Internal_Run(progressManager);
					await _loaderInstance.Internal_Intro(CancellationToken);
					CancellationToken.ThrowIfCancellationRequested();

					await _loaderInstance.Internal_UnloadScene(oldScene, unloadPreProcess, unloadProcess, unloadPostProcess, CancellationToken);
					CancellationToken.ThrowIfCancellationRequested();

					MarkRefreshCurrentScene();

					await _loaderInstance.Internal_LoadScene(newScene, loadPreProcess, loadProcess, loadPostProcess, CancellationToken);
					CancellationToken.ThrowIfCancellationRequested();

					await _loaderInstance.Internal_Outro(CancellationToken);
					CancellationToken.ThrowIfCancellationRequested();

					_loaderInstance.Internal_End();
					MarkEnd();
				}
			}
			catch(OperationCanceledException)
			{
				Debug.LogWarning("Loading Scene Cancelled");
			}
		}

		#endregion
		
		public class LoadingProcess : IDisposable
		{
			public event Action<float> ProgressSetEvent;
			public event Action<float> FinishedEvent;

			public string Message
			{
				get; private set;
			}

			public float Progress
			{
				get; private set;
			}

			public bool HasFinished
			{
				get; private set;
			}

			public void SetMessage(string message)
			{
				Message = message;
			}

			public void SetProgress(float progress)
			{
				if(HasFinished)
				{
					return;
				}

				Progress = Mathf.Clamp01(progress);
				ProgressSetEvent?.Invoke(Progress);
			}

			public void MarkAsCompleted()
			{
				if(HasFinished)
				{
					return;
				}

				Progress = 1f;
				HasFinished = true;
				ProgressSetEvent?.Invoke(Progress);
				FinishedEvent?.Invoke(Progress);
			}

			public void Dispose()
			{
				Message = default;
				Progress = default;
				HasFinished = default;
				ProgressSetEvent = null;
				FinishedEvent = null;
			}
		}

		public class LoadingInfo : IDisposable
		{
			public event Action<LoadingInfo> ProgressedEvent;

			private LoadingProcess[] _progresses;

			public int TotalProcesses => _progresses.Length;

			public float Progress
			{
				get; private set;
			}

			public int Index
			{
				get; private set;
			}

			public LoadingProcess CurrentProcess
			{
				get; private set;
			}

			public LoadingInfo(LoadingProcess[] progresses)
			{
				Index = -1;
				_progresses = progresses;
				TrySetNextProgress();
			}

			private void TrySetNextProgress()
			{
				if(CurrentProcess != null)
				{
					CurrentProcess.ProgressSetEvent -= OnProgressedProcessEvent;
					CurrentProcess.FinishedEvent -= OnFinishedProcessEvent;
					CurrentProcess = null;
				}

				if(Index < TotalProcesses - 1)
				{
					Index++;
					CurrentProcess = _progresses[Index];
					CurrentProcess.ProgressSetEvent += OnProgressedProcessEvent;
					CurrentProcess.FinishedEvent += OnFinishedProcessEvent;
					OnProgressedProcessEvent(CurrentProcess.Progress);
				}
			}

			private void OnProgressedProcessEvent(float progress)
			{
				float step = 1f / TotalProcesses;
				Progress = (step * Index) + (step * progress);
				ProgressedEvent?.Invoke(this);
			}

			private void OnFinishedProcessEvent(float progress)
			{
				TrySetNextProgress();
			}

			public void Dispose()
			{
				if(_progresses != null)
				{
					for(int i = _progresses.Length - 1; i >= 0; i--)
					{
						_progresses[i].Dispose();
					}
					_progresses = null;
					Progress = default;
					Index = -1;
				}
			}
		}
	}
}