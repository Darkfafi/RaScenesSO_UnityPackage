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

#if UNITY_EDITOR
		internal void Editor_Setup()
		{
			_sceneName = _sceneReference.GetSceneName();
		}
#endif

		#region Protected Methods

		protected void Awake()
		{
#if UNITY_EDITOR
			Editor_Setup();
#endif
		}

		internal void OnValidate()
		{
#if UNITY_EDITOR
			string oldName = _sceneName;
			Editor_Setup();

			if(oldName != _sceneName)
			{
				RaSceneSOCollection.Editor_RefreshSceneSettings();
			}
#endif
		}

		#endregion

	}
}