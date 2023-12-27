using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SceneManagementExtensions.Editor
{
	/// <summary> Custom inspector for the <see cref="SceneReference"/> class. </summary>
	[CustomEditor(typeof(SceneReference))]
	public class SceneReferenceEditor : UnityEditor.Editor 
	{
		// -----------------------------------------------------------------
		#region Project Changed

		// When something changes in the project, validate all scenes references.

		[InitializeOnLoadMethod]
		private static void AddListeners()
		{
			EditorApplication.projectChanged += OnProjectChanged;
		}

		public static void OnProjectChanged()
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
			string scenePath = AssetDatabase.GetAssetPath(sceneAssetProperty.objectReferenceValue);

			EditorGUILayout.BeginHorizontal();

			// LAOD SCENE
			if (EditorSceneManager.sceneCount == 1 && scene.isLoaded)
				GUI.enabled = false;
			if (GUILayout.Button("Open", GUILayout.Width(100)))
			{
				EditorSceneFunctions.SaveAndLoadScene(scenePath);
			}
			GUI.enabled = true;

			// ADD/REMOVE SCENE
			if (scene.isLoaded && SceneManager.sceneCount == 1)
				GUI.enabled = false;

			if (!scene.isLoaded)
			{
				if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus")))
				{
					EditorSceneFunctions.AddScene(scenePath);
				}
			}
			else
			{
				if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus")))
				{
					EditorSceneFunctions.SaveAndCloseScene(scene);
				}
			}
			GUI.enabled = true;

			GUILayout.Space(20);

			// PLAY SCENE
			if (Application.isPlaying)
				GUI.enabled = false;
			if (GUILayout.Button(EditorGUIUtility.IconContent("PlayButton On")))
			{
				if (EditorSceneFunctions.SaveAndLoadScene(scenePath))
					EditorApplication.EnterPlaymode();
			}
			GUI.enabled = true;


			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		#endregion
	}
}
