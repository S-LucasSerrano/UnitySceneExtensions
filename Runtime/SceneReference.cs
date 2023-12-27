using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagementExtensions
{
	/// <summary> Scriptable object that allows loading a specific scene. </summary>

	[CreateAssetMenu(order =201, menuName="Scene Reference", fileName="NewSceneReference")]
	public class SceneReference : ScriptableObject
	{
		/// <summary> Name of the scene referenced by this object. </summary>
		[SerializeField, HideInInspector] public string SceneName = "";


		// -----------------------------------------------------------------
		#region Editor

#if UNITY_EDITOR

		[Space][SerializeField] UnityEditor.SceneAsset sceneAsset = null;

		private void OnValidate()
		{
			SceneName = sceneAsset != null ? sceneAsset.name : "";
		}

#endif

		#endregion


		// -----------------------------------------------------------------
		#region Load Scene

		/// <summary> Load the scene this object is referencing. </summary>
		public void LoadScene()
		{
			this.LoadScene(LoadSceneMode.Single);
		}

		public void LoadScene(LoadSceneMode mode)
		{
			SceneManager.LoadScene(SceneName, mode);
		}

		public void LoadScene(LoadSceneParameters parameters)
		{
			SceneManager.LoadScene(SceneName, parameters);
		}

		#endregion

		#region Fade To Scene

		private bool _usingTransition = false;
		private SceneTransitionEffect _transitionEffect = null;

		/// <summary> Load a scene after playing a transition effect found in the scene. </summary>
		public void FadeToScene()
		{
			SceneTransitionEffect transitionEffect = FindObjectOfType<SceneTransitionEffect>(false);
			if (transitionEffect == null)
			{
				Debug.Log("Couldn't find a " + typeof(SceneTransitionEffect).Name + " while unloading " + SceneManager.GetActiveScene().name + ".");
			}

			FadeToScene(transitionEffect);
		}

		/// <summary> Load a scene after playing a transition efffect. </summary>
		public void FadeToScene(SceneTransitionEffect transitionEffect)
		{
			_usingTransition = true;
			_transitionEffect = transitionEffect;
			SceneManager.sceneLoaded += OnSceneLoaded;

			if (transitionEffect == null)
			{
				LoadScene();
			}
			else
			{ 
				transitionEffect.AnimateTransitionTo(true, LoadScene);
			}
		}

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

		/// <summary> If loaded, returns the <see cref="Scene"/> referenced by this object. <br/>
		/// If not, returs an invalid <see cref="Scene"/>. </summary>
		public Scene GetScene()
		{
			return SceneManager.GetSceneByName(SceneName);
		}

		#endregion
	}
}

/*
 TO DO:

	LOADING SCREEN
	Y luego tenemos el sciptable object loading screen
		que tiene un scene asset que es la escena de carga
		el tiempo minimo
		y funciones para decirle e cargame estas escena, y te la carga pasando primero por la escena de carga.
			y para cargar con transicion
			a ver cuanta duplicidad hay y si deberian tener una clase padre comun SceneLoader.

		quiza haya que cambiar el nombre de FadeTo a TransitionTo en los dos scriptable object
		asegurarse de que aparece debajo de scene reference en el CreateAssetMenu.

		la variable Progress tiene que ser estatica para que cualqueira pueda checkearla con LoadingScreen.Progress.

	El custom inspector que añada un boton open y ya esta.
	Mirar si podemos cambiar el icono de los scritable object

	SCENE LOADING BAR
		que muestra en el fill amount the una imagen de la ui el progreso de carga de la escena.

	El scene loader ya no existirá

	Componente LoadSceneAutomatically para cargar escenas en el start.		

	(Usar [AddComponentMenu] en todos lados)

	Preparar las Samples
 */
