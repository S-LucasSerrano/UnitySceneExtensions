
namespace UnityEngine.SceneManagement
{
	/// <summary> Scriptable object that allows loading a specific scene. </summary>

	[CreateAssetMenu(order =201, menuName="Scene Reference", fileName="NewSceneReference")]
	public class SceneReference : ScriptableObject
	{
		// -----------------------------------------------------------------
		#region Editor

#if UNITY_EDITOR

		[Space][SerializeField] UnityEditor.SceneAsset sceneAsset = null;

		private void OnValidate()
		{
			_sceneName = sceneAsset != null ? sceneAsset.name : "";
			_scenePath = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);
		}

#endif

		#endregion

		// ----------

		/// <summary> Name of the scene referenced by this object. </summary>
		public string SceneName => _sceneName;
		[SerializeField, HideInInspector] public string _sceneName = "";
		/// <summary> Path of the scene referenced by this object relative to the project folder. </summary>
		public string ScenePath => _scenePath;
		[SerializeField, HideInInspector] public string _scenePath = "";

		/// If we are using or not a transition effect to change between scenes.
		private bool _usingTransition = false;
		private SceneTransitionEffect _transitionEffect = null;


		// -----------------------------------------------------------------
		#region Load Scene

		/// <summary> Load the scene this object is referencing. </summary>
		public void LoadScene()
		{
			LoadScene(LoadSceneMode.Single);
		}

		public void LoadScene(LoadSceneMode mode)
		{
			_usingTransition = false;
			_transitionEffect = null;

			SceneManager.LoadScene(SceneName, mode);
		}

		public void LoadScene(LoadSceneParameters parameters)
		{
			_usingTransition = false;
			_transitionEffect = null;
			SceneManager.LoadScene(SceneName, parameters);
		}

		#endregion

		// ----------
		#region Transition To Scene

		/// <summary> Load this scene after playing a transition effect found in the scene. </summary>
		public void TransitionToScene()
		{
			SceneTransitionEffect transitionEffect = FindObjectOfType<SceneTransitionEffect>(false);
			if (transitionEffect == null)
			{
				Debug.Log("Couldn't find a " + typeof(SceneTransitionEffect).Name + " while unloading " + SceneManager.GetActiveScene().name + ".");
			}

			TransitionToScene(transitionEffect);
		}

		/// <summary> Load this scene after playing a transition efffect. </summary>
		public void TransitionToScene(SceneTransitionEffect transitionEffect)
		{
			_usingTransition = true;
			_transitionEffect = transitionEffect;

			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneLoaded += OnSceneLoaded;

			if (transitionEffect == null)
			{
				SceneManager.LoadScene(SceneName);
			}
			else
			{ 
				transitionEffect.AnimateTransitionTo(true, ()=> SceneManager.LoadScene(SceneName));
			}
		}

		// ---------

		private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
		{
			if (_usingTransition)
			{
				if (_transitionEffect == null)
					_transitionEffect = FindObjectOfType<SceneTransitionEffect>(false);

				if (_transitionEffect != null)
					_transitionEffect.AnimateTransitionTo(false);
				else
					Debug.Log("Couldn't find a " + typeof(SceneTransitionEffect).Name + " after loading " + SceneName + ".", this);
			}

			_transitionEffect = null;
			_usingTransition = false;
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		#endregion


		// -----------------------------------------------------------------
		#region Utilities

		/// <summary> If it's loaded, returns the <see cref="Scene"/> referenced by this object. <br/>
		/// If not, returns an invalid <see cref="Scene"/>. </summary>
		public Scene GetScene()
		{
			return SceneManager.GetSceneByName(SceneName);
		}

		#endregion
	}
}
