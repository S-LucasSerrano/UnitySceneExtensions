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
			LoadingSceneName = loadingSceneAsset != null ? loadingSceneAsset.name : "";
			LoadingScenePath = UnityEditor.AssetDatabase.GetAssetPath(loadingSceneAsset);
		}

#endif

		#endregion

		// ----------

		/// <summary> Name of the scene used as loading screen. </summary>
		[SerializeField, HideInInspector] public string LoadingSceneName = "";
		/// <summary> Path of the scene used as loading screen relative to the project folder. </summary>
		[SerializeField, HideInInspector] public string LoadingScenePath = "";
		/// <summary> The minimum time the loading screen is shown, even if the loading process takes less. </summary>
		[SerializeField][Min(0)] public float MinLoadingTime = 0;

		/// <summary> The scene that will be loaded after the loading screen. </summary>
		private string _targetScene;
		/// <summary> Progress of the async loading process. </summary>
		public static float LoadingProgress => _loadingProgress;
		private static float _loadingProgress;

		/// If we are using or not a transition effect to change between scenes.
		private bool _usingTransition = false;
		private SceneTransitionEffect _transitionEffect = null;


		// -----------------------------------------------------------------

		/// <summary> Load target scene using this loading screen. </summary>
		public void LoadScene(SceneReference targetScene)
		{
			LoadScene(targetScene.SceneName);
		}

		public void LoadScene(string targetScene)
		{
			_targetScene = targetScene;
			SceneManager.sceneLoaded += OnSceneLoaded;

			SceneManager.LoadScene(LoadingSceneName);
		}

		// ----------

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
				LoadScene(targetScene);
			else
				transitionEffect.AnimateTransitionTo(true, ()=>LoadScene(targetScene));
		}

		// ----------

		/// Function called when a scene is loaded.
		private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
		{
			if (scene.name == _targetScene)
				OnTargetSceneLoaded();
			else if (scene.name == LoadingSceneName)
				OnLoadingSceneLoaded();
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

			_targetScene = null;
			_loadingProgress = 0;
		}

		private void OnLoadingSceneLoaded()
		{
			var gameObj = new GameObject("Loading Screen Corutiner");
			var component = gameObj.AddComponent<LoadingScreenComponent>();

			component.StartCoroutine(AsyncLoadingRoutine());
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
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_targetScene);
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


		// -----------------------------------------------------------------

		/// <summary> Component to add to a game object to be able to start corroutines. </summary>
		private class LoadingScreenComponent : MonoBehaviour { }

	}
}
