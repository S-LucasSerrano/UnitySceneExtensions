using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SceneManagementExtensions.Editor
{
	/// <summary>
	/// Editor window displaying a list of all the scenes in the assets folder, and buttons to easly load them.
	/// </summary>

	public class SceneListWindow : EditorWindow
	{
		/// <summary> Dictionary with all the scenes in the asset folder associated with their directory. </summary>
		Dictionary<string, List<SceneData>> scenesByDir = new();

		/// Current position of the window's scroll view.
		Vector2 _scrollPosition = new();

		/// Different GUI styles used in the window.
		GUIStyle _loadButtonStyle = null;
		GUIStyle _addButtonStyle = null;
		GUIStyle _playButtonStyle = null;

		/// <summary> Direct acces to the preferences scriptable singleton. </summary>
		SceneListPreferences Preferences => SceneListPreferences.instance;


		// -----------------------------------------------------------------
		#region Initialization

		/// <summary> Open the Scene List window. </summary>
		[MenuItem("Window/Scenes", priority = 5000)]
		public static void Open()
		{
			var window = EditorWindow.GetWindow(typeof(SceneListWindow), false);
			window.titleContent = EditorGUIUtility.IconContent("d_UnityEditor.HierarchyWindow");
			window.titleContent.text = "Scenes";
		}

		private void OnEnable()
		{
			EditorApplication.projectChanged += FindScenes;
			FindScenes();
		}

		/// <summary> Fill the scenes dictorionary with all the scenes in the assets folder. </summary>
		private void FindScenes()
		{
			scenesByDir.Clear();

			string[] searchingPaths = { "Assets" };
			string[] guids = AssetDatabase.FindAssets("t:Scene", searchingPaths);

			foreach (string guid in guids)
			{
				SceneData newSceneData = new(guid);

				string dir = System.IO.Path.GetDirectoryName(newSceneData.path);
				if (!scenesByDir.ContainsKey(dir))
					scenesByDir.Add(dir, new List<SceneData>());

				scenesByDir[dir].Add(newSceneData);
			}

			Repaint();
		}

		#endregion


		// -----------------------------------------------------------------
		#region Draw Window

		private void OnGUI()
		{
			UpdateGUIStyles();

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

			foreach (string dir in scenesByDir.Keys)
			{
				List<SceneData> scenes = scenesByDir[dir];

				DrawDirectoryHeader(dir);

				if (Preferences.IsDirOpen(dir))
				{
					foreach (SceneData scene in scenes)
					{
						DrawButtonsForScene(scene);
					}
				}
			}

			EditorGUILayout.EndScrollView();
		}

		/// <summary> Make sure the different GUI styles draw everything as it is supposed to. </summary>
		private void UpdateGUIStyles()
		{
			if (_loadButtonStyle == null)
				_loadButtonStyle = new GUIStyle(GUI.skin.button)
				{
					alignment = TextAnchor.MiddleLeft,
					fontStyle = FontStyle.Italic
				};

			if (_addButtonStyle == null)
				_addButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fixedWidth = 30
				};

			if (_playButtonStyle == null)
				_playButtonStyle = new GUIStyle(GUI.skin.button)
				{
					fixedWidth = 50
				};

			_loadButtonStyle.fixedWidth = Screen.width - (80 + _playButtonStyle.margin.horizontal * 2);
		}

		private void DrawDirectoryHeader(string dir)
		{
			// Draw a divider.
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("---", GUI.skin.horizontalSlider, GUILayout.Width(Screen.width - 60));
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				// Draw a button to show the directoty in the project window.
				if (GUILayout.Button(EditorGUIUtility.IconContent("FolderOpened On Icon"), _addButtonStyle, GUILayout.Height(20)))
				{
					var dirObj = AssetDatabase.LoadAssetAtPath(dir, typeof(UnityEngine.Object));
					EditorGUIUtility.PingObject(dirObj);
					Selection.activeObject = dirObj;
				}

				// Draw a button to fold the directory.
				string label = dir.Substring("Assets/".Length);
				bool isOpen = Preferences.IsDirOpen(dir);

				isOpen = EditorGUILayout.BeginFoldoutHeaderGroup(isOpen, label);
				EditorGUILayout.EndFoldoutHeaderGroup();

				Preferences.SetDirFolded(dir, isOpen);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
		}

		private void DrawButtonsForScene(SceneData sceneData)
		{
			string scenePath = sceneData.path;
			string sceneName = sceneData.name;
			string buttonLabel = sceneName + "     (" + scenePath + ")";
			Scene scene = EditorSceneManager.GetSceneByPath(scenePath);

			if (scene.isDirty)
				sceneName += "*";

			Color defaultColor = GUI.color;
			if (scene.isLoaded)
				GUI.color = Color.yellow;

			// NAME OF THE SCENE
			GUILayout.Label("     " + sceneName, EditorStyles.whiteLargeLabel);

			GUILayout.BeginHorizontal();
			{
				// LOAD BUTTON
				if (scene.isLoaded && SceneManager.sceneCount == 1)
					GUI.enabled = false;

				if (GUILayout.Button(buttonLabel, _loadButtonStyle))
				{
					EditorSceneFunctions.SaveAndLoadScene(scenePath);
				}

				GUI.color = defaultColor;
				GUI.enabled = true;


				// ADD/REMOVE BUTTON
				if (scene.isLoaded && SceneManager.sceneCount == 1)
					GUI.enabled = false;

				if (!scene.isLoaded)
				{
					if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), _addButtonStyle))
					{
						EditorSceneFunctions.AddScene(scenePath);
					}
				}
				else
				{
					if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), _addButtonStyle))
					{
						EditorSceneFunctions.SaveAndCloseScene(scene);
					}
				}

				GUI.enabled = true;


				// PLAY BUTTON.
				if (Application.isPlaying)
					GUI.enabled = false;
				if (GUILayout.Button(EditorGUIUtility.IconContent("PlayButton On"), _playButtonStyle))
				{
					if (EditorSceneFunctions.SaveAndLoadScene(scenePath))
						EditorApplication.EnterPlaymode();
				}
				GUI.enabled = true;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		#endregion


		// -----------------------------------------------------------------
		#region Difinitions

		/// <summary> Data from a scene in the project. </summary>
		private class SceneData
		{
			public string guid;
			public string path;
			public string name;

			public SceneData(string guid)
			{
				this.guid = guid;

				path = AssetDatabase.GUIDToAssetPath(guid);
				name = ExtractFileNameFromPath(path);
			}

			private string ExtractFileNameFromPath(string path)
			{
				string[] strs = path.Split('/');
				string fileName = strs[strs.Length - 1];
				string extension = ".unity";
				fileName = fileName.Substring(0, fileName.Length - extension.Length);
				return fileName;
			}
		}

		#endregion
	}
}
