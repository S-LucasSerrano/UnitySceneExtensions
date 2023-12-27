using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace SceneManagementExtensions.Editor
{
	/// <summary> Functions to manage scenes in the editor. </summary>
	public static class EditorSceneFunctions
	{
		/// <summary> Open the target scene, asking to save if there are changes. </summary>
		/// <returns> True if the scene has been loaded. </returns>
		public static bool SaveAndLoadScene(string scene)
		{
			if (Application.isPlaying)
			{
				SceneManager.LoadScene(scene);
			}
			else
			{
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
					EditorSceneManager.OpenScene(scene);
				else
					return false;
			}

			return true;
		}

		/// <summary> Open target scene without closing the rest. </summary>
		public static void AddScene(string scene)
		{
			if (Application.isPlaying)
			{
				SceneManager.LoadScene(scene, LoadSceneMode.Additive);
			}
			else
			{
				EditorSceneManager.OpenScene(scene, OpenSceneMode.Additive);
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
