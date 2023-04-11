using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RaScenesSO
{
	public class RaSceneLoaderGeneric : RaSceneLoaderBase
	{
		[SerializeField]
		private CanvasGroup _content = null;

		[SerializeField]
		private AudioListener _audioListener = null;

		[SerializeField]
		private EventSystem _eventSystem = null;

		[SerializeField]
		private Image _progressBar = null;

		[SerializeField]
		private Text _progressDisplay = null;

		[SerializeField]
		private FillType _fillType = FillType.ScaleX;

		protected override void OnInitialize()
		{
			if(_content != null)
			{
				_content.alpha = 0f;
			}

			if(_audioListener != null)
			{
				_audioListener.enabled = false;
			}

			if(_eventSystem != null)
			{
				_eventSystem.enabled = false;
			}
		}

		protected override async Task DoIntro(CancellationToken token)
		{
			if(_content != null)
			{
				float fade = 0f;
				while(fade <= 1f)
				{
					_content.alpha = fade;
					fade += Time.deltaTime;
					await Task.Yield();
					token.ThrowIfCancellationRequested();
				}
			}
			else
			{
				await Task.Yield();
			}
		}

		protected override async Task DoOutro(CancellationToken token)
		{
			if(_audioListener != null)
			{
				_audioListener.enabled = false;
			}

			if(_eventSystem != null)
			{
				_eventSystem.enabled = false;
			}

			if(_content != null)
			{

				float fade = 1f;
				while(fade >= 0f)
				{
					_content.alpha = fade;
					fade -= Time.deltaTime;
					await Task.Yield();

					if(token.IsCancellationRequested)
					{
						return;
					}
				}
			}
			else
			{
				await Task.Yield();
			}
		}

		protected override void OnDeinitialize()
		{
			if(_content != null)
			{
				_content.alpha = 0f;
			}
		}

		protected override void OnProgress(LoadingInfo loadingProgress)
		{
			if(_progressBar != null)
			{
				switch(_fillType)
				{
					case FillType.ScaleX:
						Vector3 scale = _progressBar.transform.localScale;
						scale.x = loadingProgress.Progress;
						_progressBar.transform.localScale = scale;
						break;
					case FillType.ImageFill:
						_progressBar.fillAmount = loadingProgress.Progress;
						break;
				}
			}

			if(_progressDisplay != null)
			{
				_progressDisplay.text = $"{Mathf.RoundToInt(loadingProgress.Progress * 100)}% ({loadingProgress.Index + 1}/{loadingProgress.TotalProcesses})";
			}
		}

		public enum FillType
		{
			ImageFill,
			ScaleX
		}
	}
}