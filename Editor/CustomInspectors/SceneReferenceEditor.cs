using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.SceneManagement
{
	/// <summary> Custom inspector for the <see cref="SceneReference"/> class. </summary>
	[CustomEditor(typeof(SceneReference))]
	public class SceneReferenceEditor : Editor 
	{
		// -----------------------------------------------------------------
		#region Validation

		// When something changes in the project, validate all scenes references.

		[InitializeOnLoadMethod]
		private static void AddListeners()
		{
			EditorApplication.projectChanged += OnProjectChanged;
		}

		private static void OnProjectChanged()
		{
			string[] searchingPaths = { "Assets" };
			string[] guids = AssetDatabase.FindAssets("t:" + typeof(SceneReference), searchingPaths);

			foreach(string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				SceneReference scene = AssetDatabase.LoadAssetAtPath<SceneReference>(path);

				MethodInfo onValidate = scene.GetType().GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);
				onValidate.Invoke(scene, new object[] { });
			}
		}

		#endregion

		#region Open On Double Click

		// When the user double clicks on the asset, open the scene.

		[UnityEditor.Callbacks.OnOpenAsset]
		private static bool OnDoubleClick(int instanceId, int line)
		{
			var obj = EditorUtility.InstanceIDToObject(instanceId);
			if (obj is SceneReference)
			{
				var sceneReference = (SceneReference)obj;

				if (string.IsNullOrEmpty(sceneReference.SceneName))
					return false;

				var scenePath = sceneReference.ScenePath;
				SceneManagerExtensions.SaveAndLoadScene(scenePath);

				return true;
			}

			return false;
		}

		#endregion


		// -----------------------------------------------------------------
		#region Custom Inspector

		SceneReference sceneReference;
		SerializedProperty sceneAssetProperty;

		private void OnEnable()
		{
			sceneReference = (SceneReference)target;
			sceneAssetProperty = serializedObject.FindProperty("sceneAsset");
		}

		// ----------

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(sceneAssetProperty);

			serializedObject.ApplyModifiedProperties();

			if (sceneAssetProperty.objectReferenceValue == null)
				return;

			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			DrawLoadingButtons();
		}

		private void DrawLoadingButtons()
		{
			var scene = sceneReference.GetScene();
			string scenePath = sceneReference.ScenePath;

			EditorGUILayout.BeginHorizontal();

			// LOAD SCENE
			if (EditorSceneManager.sceneCount == 1 && scene.isLoaded)
				GUI.enabled = false;
			if (GUILayout.Button("Open", GUILayout.Width(100)))
			{
				SceneManagerExtensions.SaveAndLoadScene(scenePath);
			}
			GUI.enabled = true;

			// ADD/REMOVE SCENE
			if (scene.isLoaded && SceneManager.sceneCount == 1)
				GUI.enabled = false;

			if (!scene.isLoaded)
			{
				if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus")))
				{
					SceneManagerExtensions.AddScene(scenePath);
				}
			}
			else
			{
				if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus")))
				{
					SceneManagerExtensions.SaveAndCloseScene(scene);
				}
			}
			GUI.enabled = true;

			GUILayout.Space(20);

			// PLAY SCENE
			if (Application.isPlaying)
				GUI.enabled = false;
			if (GUILayout.Button(EditorGUIUtility.IconContent("PlayButton On")))
			{
				if (SceneManagerExtensions.SaveAndLoadScene(scenePath))
					EditorApplication.EnterPlaymode();
			}
			GUI.enabled = true;


			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		#endregion
	}
}
