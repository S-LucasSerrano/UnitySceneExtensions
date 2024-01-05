using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.SceneManagement
{
	/// <summary>
	/// Editor window displaying a list of all the scenes in the assets folder, and buttons to easly load them.
	/// </summary>

	public class SceneListWindow : EditorWindow
	{
		/// <summary> Dictionary with all the scenes in the asset folder associated with their directory. </summary>
		Dictionary<string, List<SceneData>> scenesByDir = new Dictionary<string, List<SceneData>>();

		/// Current position of the window's scroll view.
		Vector2 _scrollPosition = new Vector2();

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
				SceneData newSceneData = new SceneData(guid);

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

		private void DrawDirectoryHeader(string dir)
		{
			// Draw a divider.
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(70);
				EditorGUILayout.LabelField("---", SeparatorStyle);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				// Draw a button to show the directoty in the project window.
				if (GUILayout.Button(FolderIcon, FolderButtonStyle))
				{
					var dirObj = AssetDatabase.LoadAssetAtPath(dir, typeof(UnityEngine.Object));
					EditorGUIUtility.PingObject(dirObj);
					Selection.activeObject = dirObj;
				}

				// Draw a button to fold the directory.
				string label = dir.Substring("Assets/".Length);
				bool isOpen = Preferences.IsDirOpen(dir);
				bool wasOpen = isOpen;

				isOpen = EditorGUILayout.BeginFoldoutHeaderGroup(isOpen, label);
				EditorGUILayout.EndFoldoutHeaderGroup();

				if (isOpen != wasOpen)
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

				if (GUILayout.Button(buttonLabel, LoadButtonStyle))
				{
					SceneManagerExtensions.SaveAndLoadScene(scenePath);
				}

				GUI.color = defaultColor;
				GUI.enabled = true;


				// ADD/REMOVE BUTTON
				if (scene.isLoaded && SceneManager.sceneCount == 1)
					GUI.enabled = false;

				if (!scene.isLoaded)
				{
					if (GUILayout.Button(PlusIcon, AddButtonStyle))
					{
						SceneManagerExtensions.AddScene(scenePath);
					}
				}
				else
				{
					if (GUILayout.Button(MinusIcon, AddButtonStyle))
					{
						SceneManagerExtensions.SaveAndCloseScene(scene);
					}
				}

				GUI.enabled = true;


				// PLAY BUTTON.
				if (Application.isPlaying)
					GUI.enabled = false;
				if (GUILayout.Button(PlayIcon, PlayButtonStyle))
				{
					if (SceneManagerExtensions.SaveAndLoadScene(scenePath))
						EditorApplication.EnterPlaymode();
				}
				GUI.enabled = true;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		#endregion

		#region Styles & Icons

		// ---
		GUIStyle _loadButtonStyle = null;
		GUIStyle LoadButtonStyle
		{
			get
			{
				if (_loadButtonStyle == null)
				{
					_loadButtonStyle = new GUIStyle(GUI.skin.button);
					_loadButtonStyle.alignment = TextAnchor.MiddleLeft;
					_loadButtonStyle.fontStyle = FontStyle.Italic;
				}

				_loadButtonStyle.fixedWidth = Screen.width - (80 + PlayButtonStyle.margin.horizontal * 2);
				return _loadButtonStyle;
			}
		}

		// ---
		GUIStyle _addButtonStyle = null;
		GUIStyle AddButtonStyle
		{
			get
			{
				if (_addButtonStyle == null)
				{
					_addButtonStyle = new GUIStyle(GUI.skin.button);
					_addButtonStyle.fixedWidth = 30;
				}

				return _addButtonStyle;
			}
		}

		GUIContent _plusIcon = null;
		GUIContent PlusIcon
		{
			get
			{
				if (_plusIcon == null)
					_plusIcon = EditorGUIUtility.IconContent("Toolbar Plus");
				return _plusIcon;
			}
		}

		GUIContent _minusIcon = null;
		GUIContent MinusIcon
		{
			get
			{
				if (_minusIcon == null)
					_minusIcon = EditorGUIUtility.IconContent("Toolbar Minus");
				return _minusIcon;
			}
		}

		// ---
		GUIStyle _playButtonStyle = null;
		GUIStyle PlayButtonStyle
		{
			get
			{
				if (_playButtonStyle == null)
				{
					_playButtonStyle = new GUIStyle(GUI.skin.button);
					_playButtonStyle.fixedWidth = 50;
				}
				return _playButtonStyle;
			}
		}

		GUIContent _playIcon = null;
		GUIContent PlayIcon
		{
			get
			{
				if (_playIcon == null)
					_playIcon = EditorGUIUtility.IconContent("PlayButton On");
				return _playIcon;
			}
		}

		// ---
		GUIStyle _folderButtonStyle = null;
		GUIStyle FolderButtonStyle
		{
			get
			{
				if (_folderButtonStyle == null)
				{
					_folderButtonStyle = new GUIStyle(AddButtonStyle);
					_folderButtonStyle.fixedHeight = 20;
				}
				return _folderButtonStyle;
			}
		}

		GUIContent _folderIcon = null;
		GUIContent FolderIcon
		{
			get
			{
				if (_folderIcon == null)
					_folderIcon = EditorGUIUtility.IconContent("FolderOpened On Icon");
				return _folderIcon;
			}
		}

		// ---
		GUIStyle _separatorStyle = null;
		GUIStyle SeparatorStyle
		{
			get
			{
				if (_separatorStyle == null)
				{
					_separatorStyle = new GUIStyle(GUI.skin.horizontalSlider);
				}
				_separatorStyle.fixedWidth = Screen.width - 80;
				return _separatorStyle;
			}
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
