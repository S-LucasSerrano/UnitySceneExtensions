using UnityEngine.UI;

namespace UnityEngine.SceneManagement
{
	/// <summary> Component that modifies the fill amount of a UI image to show the progress of a loading screen. </summary>

	[AddComponentMenu("Scene Management/Loading Progress Bar")]
	public class LoadingProgressBar : MonoBehaviour
	{
		[Space][SerializeField] public Image LoadingBar = null;


		private void Reset()
		{
			LoadingBar = GetComponent<Image>();
		}

		private void Update()
		{
			if (LoadingBar == null)
				return;

			LoadingBar.fillAmount = LoadingScreen.LoadingProgress;
		}
	}
}
