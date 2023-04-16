using RaModelsSO;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace RaScenesSO
{
	public abstract class RaSceneLoaderBase : MonoBehaviour
	{
		public UnityEvent LoadingStartedEvent;
		public UnityEvent LoadingEndedEvent;

		[SerializeField]
		private RaModelSOLocator _models = null;

		private CancellationTokenSource _cancellationTokenSource = null;

		protected RaSceneModelSO SceneModel
		{
			get; private set;
		}

		protected void Awake()
		{
			_cancellationTokenSource = new CancellationTokenSource();
			SceneModel = _models.GetModelSO<RaSceneModelSO>();
			OnInitialize();
		}

		protected async void Start()
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
					progressManager.ProgressedEvent += OnProgress;

					CancellationToken token = _cancellationTokenSource.Token;

					RaSceneSO oldScene = SceneModel.PreviousScene;
					RaSceneSO newScene = SceneModel.NextScene;

					LoadingStartedEvent.Invoke();

					await DoIntro(token);
					token.ThrowIfCancellationRequested();

					await UnloadScene(oldScene, unloadPreProcess, unloadProcess, unloadPostProcess, token);
					token.ThrowIfCancellationRequested();

					SceneModel.Loader_RefreshCurrentScene();

					await LoadScene(newScene, loadPreProcess, loadProcess, loadPostProcess, token);
					token.ThrowIfCancellationRequested();

					await DoOutro(token);
					token.ThrowIfCancellationRequested();

					SceneManager.UnloadSceneAsync(gameObject.scene);

					SceneModel.Loader_End();

					LoadingEndedEvent.Invoke();
				}
			}
			catch(OperationCanceledException)
			{
				Debug.LogWarning("Loading Scene Cancelled");
			}
		}

		protected abstract void OnProgress(LoadingInfo loadingProgress);

		protected void OnDestroy()
		{
			OnDeinitialize();
			SceneModel = null;

			if(_cancellationTokenSource != null)
			{
				_cancellationTokenSource.Cancel();
				_cancellationTokenSource.Dispose();
				_cancellationTokenSource = null;
			}

			LoadingStartedEvent.RemoveAllListeners();
			LoadingEndedEvent.RemoveAllListeners();
		}

		private async Task UnloadScene(RaSceneSO oldScene, LoadingProcess preProcess, LoadingProcess mainProcess, LoadingProcess postProcess, CancellationToken token)
		{
			preProcess.SetProgress(0);
			{
				await PreUnloadSceneJob(preProcess, token);
			}
			preProcess.MarkAsCompleted();

			mainProcess.SetProgress(0);
			{
				await PerformAsyncOperation(SceneManager.UnloadSceneAsync(oldScene.SceneName), mainProcess, token);
			}
			mainProcess.MarkAsCompleted();

			postProcess.SetProgress(0);
			{
				await PostUnloadSceneJob(postProcess, token);
			}
			postProcess.MarkAsCompleted();
		}

		private async Task LoadScene(RaSceneSO newScene, LoadingProcess preProcess, LoadingProcess mainProcess, LoadingProcess postProcess, CancellationToken token)
		{
			AsyncOperation loadOperation;

			preProcess.SetProgress(0);
			{
				await PreLoadSceneJob(preProcess, token);
			}
			preProcess.MarkAsCompleted();

			mainProcess.SetProgress(0);
			{
				loadOperation = SceneManager.LoadSceneAsync(newScene.SceneName, LoadSceneMode.Additive);
				loadOperation.allowSceneActivation = false;
				await PerformAsyncOperation(loadOperation, mainProcess, token);
			}
			mainProcess.MarkAsCompleted();

			postProcess.SetProgress(0);
			{
				await PostLoadSceneJob(postProcess, token);
				loadOperation.allowSceneActivation = true;
			}
			postProcess.MarkAsCompleted();
		}

		protected abstract void OnInitialize();
		protected abstract Task DoIntro(CancellationToken token);
		protected abstract Task DoOutro(CancellationToken token);
		
		protected abstract void OnDeinitialize();

		protected virtual async Task PreUnloadSceneJob(LoadingProcess loadingProcess, CancellationToken token)
		{
			await Task.Yield();
		}

		protected virtual async Task PostUnloadSceneJob(LoadingProcess loadingProcess, CancellationToken token)
		{
			await Task.Yield();
		}

		protected virtual async Task PreLoadSceneJob(LoadingProcess loadingProcess, CancellationToken token)
		{
			await Task.Yield();
		}

		protected virtual async Task PostLoadSceneJob(LoadingProcess loadingProcess, CancellationToken token)
		{
			await Task.Yield();
		}

		private async Task PerformAsyncOperation(AsyncOperation operation, LoadingProcess loadingProcess, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			while(true)
			{
				loadingProcess.SetProgress(operation.progress);
				await Task.Yield();
				token.ThrowIfCancellationRequested();
				if(operation.progress >= 0.9f)
				{
					break;
				}
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
	}
}