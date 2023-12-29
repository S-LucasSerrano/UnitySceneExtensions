using System.Collections;

namespace UnityEngine.SceneManagement
{
	/// <summary> Component that loads a scene on start. </summary>

	[AddComponentMenu("Scene Management/Load Scene Automatically")]
	public class LoadSceneAutomatically : MonoBehaviour
	{
		[Tooltip("Time to wait before loading the scene.")]
		[SerializeField][Min(0)] public float delay = 0;

		[Tooltip("Scene to load.")]
		[SerializeField] public SceneReference targetScene = null;

		[Tooltip("Whether or not to use a transition effect to change the scene.")]
		[SerializeField] public bool useTransition = true;
		[Tooltip("Transition effect that will be used.")]
		[SerializeField] public SceneTransitionEffect transitionEffect = null;

		[Tooltip("Whether or not to load the scene using a loading screen.")]
		[SerializeField] public bool useLoadingScreen = false;
		[Tooltip("Loading screen that will be used.")]
		[SerializeField] public LoadingScreen loadingScreen = null;


		// -----------------------------------------------------------------

		private IEnumerator Start()
		{
			if (targetScene == null)
				yield break;

			if (delay > 0)
				yield return new WaitForSecondsRealtime(delay);

			if (useLoadingScreen && loadingScreen != null)
			{
				if (useTransition)
				{
					if (transitionEffect != null)
						loadingScreen.TransitionToScene(targetScene, transitionEffect);
					else
						loadingScreen.TransitionToScene(targetScene);
				}
				else
				{
					loadingScreen.LoadScene(targetScene);
				}
			}
			else
			{
				if (useTransition)
				{
					if (transitionEffect != null)
						targetScene.TransitionToScene(transitionEffect);
					else
						targetScene.TransitionToScene();
				}
				else
				{
					targetScene.LoadScene();
				}
			}
		}
	}
}
