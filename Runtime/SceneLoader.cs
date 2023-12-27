using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagementExtensions
{
	/// <summary>
	/// 
	/// </summary>

	public class SceneLoader : MonoBehaviour
	{
		[Space]
		[Tooltip("Scene used as loading screen.")]
		[SerializeField] private SceneReference _loadingScene = null;

		[Tooltip("The minimum time the loading screen is shown, even if the loading process takes less.")]
		[SerializeField][Min(0)] private float _minLoadingTime = 0;

		// ---------

		/// Singleton.
		private static SceneLoader _instance = null;
		/// <summary> Reference to the current instance of the SceneLoader. </summary>
		public static SceneLoader Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = FindObjectOfType<SceneLoader>(true);
				}
				if (_instance == null)
				{
					var gameObj = new GameObject("Scene Loader");
					_instance = gameObj.AddComponent<SceneLoader>();
				}
				return _instance;
			}
		}

		// ---------

		/// <summary> Name of the scene used as loading screen. </summary>
		public static SceneReference LoadingScene
		{
			get => Instance._loadingScene;
			set => Instance._loadingScene = value;
		}

		/// <summary> Name of the scene used as loading screen. </summary>
		private static string LoadingSceneName
		{
			get => LoadingScene != null ? LoadingScene.SceneName : "";
		}

		/// <summary> The minimum time the loading screen is shown, even if the loading process takes less. </summary>
		public static float MinLoadingTime
		{
			get => Instance._minLoadingTime;
			set => Instance._minLoadingTime = value;
		}

		/// <summary> Scene we will load after the loading screen. </summary>
		private static string targetScene = null;


		// -----------------------------------------------------------------

		static SceneLoader()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			// If we reach the loading scene, start loading the target asynchronously.
			if (scene.name == LoadingSceneName && string.IsNullOrEmpty(targetScene))
			{	
				Debug.Log("Estamos en la pantalla de carga.");
				SceneManager.LoadScene("");
			}
			else
			{
				targetScene = null;

				Debug.Log("No estamos en la pantalla de carga.");
			}
		}


		// -----------------------------------------------------------------

		private static void LoadScene(string scene)
		{
			// Abrir escena indicada.
		}


		// -----------------------------------------------------------------

		public static void LoadSceneWithLoadingScreen(SceneReference targetScene)
		{
			LoadSceneWithLoadingScreen(targetScene.SceneName, LoadingSceneName);
		}

		public static void LoadSceneWithLoadingScreen(string targetScene)
		{
			LoadSceneWithLoadingScreen(targetScene, LoadingSceneName);
		}
		
		public static void LoadSceneWithLoadingScreen(SceneReference targetScene, SceneReference loadingScene)
		{
			LoadSceneWithLoadingScreen(targetScene.SceneName, loadingScene.SceneName);
		}

		public static void LoadSceneWithLoadingScreen(string targetScene, string loadingScene)
		{
			if (string.IsNullOrEmpty(loadingScene))
			{
				Debug.LogError("The loading scene was not set on the SceneLoader.", Instance);

				LoadScene(targetScene);
				return;
			}

			SceneLoader.targetScene = targetScene;
			SceneManager.LoadScene(loadingScene);
		}
	}
}

