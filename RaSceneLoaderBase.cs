using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using static RaScenesSO.RaSceneModelSO;

namespace RaScenesSO
{
	public abstract class RaSceneLoaderBase : MonoBehaviour
	{
		public UnityEvent LoadingStartedEvent;
		public UnityEvent LoadingEndedEvent;

		public LoadingInfo LoadingInfo
		{
			get; private set;
		}

		internal void Internal_Run(LoadingInfo loadingInfo)
		{
			LoadingInfo = loadingInfo;

			LoadingInfo.ProgressedEvent += OnProgress;

			OnInitialize();
			LoadingStartedEvent?.Invoke();
		}

		internal void Internal_End()
		{
			LoadingEndedEvent?.Invoke();

			OnDeinitialize();

			LoadingStartedEvent.RemoveAllListeners();
			LoadingEndedEvent.RemoveAllListeners();

			LoadingInfo.ProgressedEvent -= OnProgress;

			LoadingInfo = null;
		}

		protected abstract void OnProgress(LoadingInfo loadingInfo);

		internal async Task Internal_UnloadScene(RaSceneSO oldScene, LoadingProcess preProcess, LoadingProcess mainProcess, LoadingProcess postProcess, CancellationToken token)
		{
			preProcess.SetProgress(0);
			{
				await PreUnloadSceneJob(preProcess, token);
			}
			preProcess.MarkAsCompleted();

			mainProcess.SetProgress(0);
			{
				if (oldScene != null)
				{
					AsyncOperation operation = SceneManager.UnloadSceneAsync(oldScene.SceneName);
					await PerformAsyncOperation(operation, mainProcess, token);
				}
			}
			mainProcess.MarkAsCompleted();

			postProcess.SetProgress(0);
			{
				await PostUnloadSceneJob(postProcess, token);
			}
			postProcess.MarkAsCompleted();

		}

		internal async Task Internal_LoadScene(RaSceneSO newScene, LoadingProcess preProcess, LoadingProcess mainProcess, LoadingProcess postProcess, CancellationToken token)
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

				if (loadOperation != null)
				{
					loadOperation.allowSceneActivation = true;
				}
			}
			postProcess.MarkAsCompleted();
		}

		internal async Task Internal_Intro(CancellationToken token)
		{
			await DoIntro(token);
		}

		internal async Task Internal_Outro(CancellationToken token)
		{
			await DoOutro(token);
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
			while (true)
			{
				loadingProcess.SetProgress(operation.progress);
				await Task.Yield();
				token.ThrowIfCancellationRequested();
				if (operation.progress >= 0.9f)
				{
					break;
				}
			}
		}
	}
}