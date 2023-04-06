using UnityEngine;
using UnityEngine.SceneManagement;

namespace RaScenesSO
{
	public class RaSceneSO : ScriptableObject
	{
		[SerializeField]
		private string _nickname = string.Empty;

		[SerializeField]
		private SceneReference _sceneReference;

		[SerializeField, HideInInspector]
		private string _sceneName = string.Empty;

		#region Properties

		public string Nickname => string.IsNullOrEmpty(_nickname) ? name : _nickname;

		public string SceneName => _sceneName;

		public string ScenePath => _sceneReference.GetScenePath();

		#endregion

		#region Public Methods

		/// <summary>
		/// This method only works if the scene is currently loaded in
		/// </summary>
		public Scene GetUnitySceneData()
		{
			return SceneManager.GetSceneByName(SceneName);
		}

		#endregion

		#region Protected Methods

		protected void OnValidate()
		{
			_sceneName = _sceneReference.GetSceneName();
		}

		#endregion

	}
}