using System.Collections;

namespace UnityEngine.SceneManagement
{
	/// <summary> Scriptable object that allows to load scenes using a loading screen. </summary>

	[CreateAssetMenu(order = 202, menuName = "Loading Screen", fileName = "NewLoadingScreen")]
	public class LoadingScreen : ScriptableObject
	{
		// -----------------------------------------------------------------
		#region Editor

#if UNITY_EDITOR

		[Space][SerializeField] UnityEditor.SceneAsset loadingSceneAsset = null;

		private void OnValidate()
		{
			_loadingSceneName = loadingSceneAsset != null ? loadingSceneAsset.name : "";
			_loadingScenePath = UnityEditor.AssetDatabase.GetAssetPath(loadingSceneAsset);
		}

#endif

		#endregion

		// ----------

		/// <summary> Name of the scene used as loading screen. </summary>
		public string LoadingSceneName => _loadingSceneName;
		[SerializeField, HideInInspector] private string _loadingSceneName = "";
		/// <summary> Path of the scene used as loading screen relative to the project folder. </summary>
		public string LoadingScenePath => _loadingScenePath;
		[SerializeField, HideInInspector] public string _loadingScenePath = "";

		/// <summary> The minimum time the loading screen is shown, even if the loading process takes less. </summary>
		[SerializeField][Min(0)] public float MinLoadingTime = 0;

		/// <summary> The scene that will be loaded after the loading screen. </summary>
		private string _targetSceneName;
		/// <summary> Progress of the async loading process. </summary>
		public static float LoadingProgress => _loadingProgress;
		private static float _loadingProgress;

		/// If we are using or not a transition effect to change between scenes.
		private bool _usingTransition = false;
		private SceneTransitionEffect _transitionEffect = null;


		// -----------------------------------------------------------------
		#region Load Scene

		/// <summary> Load target scene using this loading screen. </summary>
		public void LoadScene(SceneReference targetScene)
		{
			LoadScene(targetScene.SceneName);
		}

		public void LoadScene(string targetScene)
		{
			_usingTransition = false;
			_transitionEffect = null;

			LoadSceneAfterLoadingScreen(targetScene);
		}

		#endregion

		// ----------
		#region Transition To Scene

		/// <summary> Load target scene using this loading screen and playing a transition effect found in the scene. </summary>
		public void TransitionToScene(SceneReference targetScene)
		{
			TransitionToScene(targetScene.SceneName);
		}

		public void TransitionToScene(string targetScene)
		{
			SceneTransitionEffect transitionEffect = FindObjectOfType<SceneTransitionEffect>(false);
			TransitionToScene(targetScene, transitionEffect);
		}

		/// <summary> Load target scene using this loading screen and playing the given transition effect. </summary>
		public void TransitionToScene(SceneReference targetScene, SceneTransitionEffect transitionEffect)
		{
			TransitionToScene(targetScene.SceneName, transitionEffect);
		}

		public void TransitionToScene(string targetScene, SceneTransitionEffect transitionEffect)
		{
			_usingTransition = true;
			_transitionEffect = transitionEffect;

			if (transitionEffect == null)
				LoadSceneAfterLoadingScreen(targetScene);
			else
				transitionEffect.AnimateTransitionTo(true, ()=> LoadSceneAfterLoadingScreen(targetScene));
		}

		#endregion

		// ----------
		#region Loading Screen

		/// <summary> Open the loading scene before loading the target scene. </summary>
		private void LoadSceneAfterLoadingScreen(string targetScene)
		{
			_targetSceneName = targetScene;

			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneLoaded += OnSceneLoaded;

			SceneManager.LoadScene(LoadingSceneName);
		}

		/// Function called when a scene is loaded.
		private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
		{
			if (scene.name == LoadingSceneName)
				OnLoadingSceneLoaded();
			else if (scene.name == _targetSceneName)
				OnTargetSceneLoaded();
		}

		private void OnLoadingSceneLoaded()
		{
			var gameObj = new GameObject("Loading Screen Corutiner");
			var component = gameObj.AddComponent<LoadingScreenComponent>();

			component.StartCoroutine(AsyncLoadingRoutine());
		}

		private void OnTargetSceneLoaded()
		{
			// Play transition effect.
			if (_usingTransition)
			{
				if (_transitionEffect == null)
					_transitionEffect = FindObjectOfType<SceneTransitionEffect>(false);
				if (_transitionEffect != null)
					_transitionEffect.AnimateTransitionTo(false);
			}

			// Reset.
			_targetSceneName = null;
			_loadingProgress = 0;

			_usingTransition = false;
			_transitionEffect = null;

			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		/// Coroutine that loads the target scene asyncronously.
		IEnumerator AsyncLoadingRoutine()
		{
			float timeCounter = 0;
			_loadingProgress = 0;

			// Play scene transition.
			if (_usingTransition)
			{
				if (_transitionEffect == null)
					_transitionEffect = FindObjectOfType<SceneTransitionEffect>(false);

				if (_transitionEffect != null)
					_transitionEffect.AnimateTransitionTo(false);
			}

			yield return new WaitForEndOfFrame();       // AsyncOperation.allowSceneActivation doesn't work unless you wait this.

			// Load the target scene in the background.
			Application.backgroundLoadingPriority = ThreadPriority.Low;
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_targetSceneName);
			asyncLoad.allowSceneActivation = false;

			while (_loadingProgress < 0.9f)
			{
				yield return null;

				timeCounter += Time.deltaTime;
				_loadingProgress = asyncLoad.progress;
			}

			// When the scene is ready, make sure that we wait at least the given amount of min time.
			while(timeCounter < MinLoadingTime)
			{
				yield return null;

				timeCounter += Time.deltaTime;
				_loadingProgress = Mathf.Lerp(0.9f, 1, timeCounter / MinLoadingTime);
			}

			// Play scene transition.
			if (_usingTransition && _transitionEffect != null)
			{
				while (_transitionEffect.Animating)
					yield return null;

				_transitionEffect.AnimateTransitionTo(true);

				while(_transitionEffect.Animating)
					yield return null;
			}

			// Go to the target scene.
			_loadingProgress = 1;
			asyncLoad.allowSceneActivation = true;
		}

		#endregion


		// -----------------------------------------------------------------

		/// <summary> Component to add to a game object to be able to start corroutines. </summary>
		private class LoadingScreenComponent : MonoBehaviour { }

	}
}
