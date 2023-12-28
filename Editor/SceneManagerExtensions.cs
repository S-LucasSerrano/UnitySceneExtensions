using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.SceneManagement
{
	/// <summary> Added functions to manage scenes in the editor. </summary>
	public static class SceneManagerExtensions
	{
		/// <summary> Open the target scene, asking to save if there are changes. </summary>
		/// <returns> True if the scene has been loaded. </returns>
		public static bool SaveAndLoadScene(string scenePath)
		{
			if (Application.isPlaying)
			{
				SceneManager.LoadScene(scenePath);
			}
			else
			{
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
					EditorSceneManager.OpenScene(scenePath);
				else
					return false;
			}

			return true;
		}

		/// <summary> Open target scene without closing the rest. </summary>
		public static void AddScene(string scenePath)
		{
			if (Application.isPlaying)
			{
				SceneManager.LoadScene(scenePath, LoadSceneMode.Additive);
			}
			else
			{
				EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
			}
		}

		/// <summary> Close target scene, asking to save if there are changes. </summary>
		public static void SaveAndCloseScene(Scene scene)
		{
			if (!scene.isLoaded)
				return;

			if (Application.isPlaying)
			{
				SceneManager.UnloadSceneAsync(scene);
			}
			else
			{
				if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new Scene[] { scene }))
					EditorSceneManager.CloseScene(scene, true);
			}
		}
	}
}
