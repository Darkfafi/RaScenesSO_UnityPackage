using System.IO;
using UnityEngine;

namespace RaScenesSO
{
	[System.Serializable]
	public struct SceneReference
	{
		#region Editor Variables

		[SerializeField]
		private string _scenePath;

		#endregion

		#region Public Methods

		public string GetSceneName()
		{
			return Path.GetFileNameWithoutExtension(_scenePath);
		}

		public string GetScenePath()
		{
			return _scenePath;
		}

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(_scenePath);
		}

		#endregion
	}
}