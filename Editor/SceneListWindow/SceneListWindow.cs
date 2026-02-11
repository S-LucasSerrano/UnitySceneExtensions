using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace LucasSerrano.SceneManagement.Editor
{
	/// <summary>
	/// Editor window displaying a list of all the scenes in the assets folder, and buttons to easly load them.
	/// </summary>

	public class SceneListWindow : EditorWindow
	{
		/// <summary> Folder hierarchy with all the scenes in the project. </summary>
		Folder sceneFolderTree = null;

		/// Current position of the window's scroll view.
		Vector2 _scrollPosition = new Vector2();

		/// <summary> Direct acces to the preferences scriptable singleton. </summary>
		SceneListPreferences Preferences => SceneListPreferences.instance;


		// -----------------------------------------------------------------
		#region Initialization

		/// <summary> Open the Scene List window. </summary>
		[MenuItem("Window/Scene List", priority = 5000)]
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

		/// <summary> Fill the scene tree with all the scenes in the assets folder. </summary>
		private void FindScenes()
		{
			sceneFolderTree = new Folder() { name = "Assets", path="Assets" };

			string[] searchingPaths = { "Assets" };
			string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", searchingPaths);
			foreach (string guid in sceneGuids)
			{
				SceneData sceneData = new SceneData(guid);
				AddScene(sceneData);
			}

			FlattenFolderTree(sceneFolderTree);

			Repaint();
		}

		/// <summary> Add target scene to the folder tree. </summary>
		private void AddScene(SceneData scene)
		{
			string scenePath = System.IO.Path.GetDirectoryName(scene.path);
			string[] folderNames = scenePath.Split(System.IO.Path.DirectorySeparatorChar);

			// Go deeper into the tree, adding each folder in the scene path.
			Folder currentFolder = sceneFolderTree;
			for (int i = 1; i < folderNames.Length; i++)
			{
				var folderName = folderNames[i];

				if (currentFolder.ContainsChild(folderName) == false)
				{
					var newFolder = new Folder();
					newFolder.name = folderName;
					newFolder.path = currentFolder.path + "/" + newFolder.name;

					currentFolder.childFolders.Add(newFolder);
				}
				currentFolder = currentFolder.GetChild(folderName);
			}
			// Add the scene to the last folder.
			if (currentFolder.scenes.Contains(scene) == false)
			{
				currentFolder.scenes.Add(scene);
			}
		}

		/// <summary> Ensure that there are no folders that only contain another single folder. </summary>
		private void FlattenFolderTree(Folder parentFolder)
		{
			foreach(var folder in parentFolder.childFolders)
			{
				FlattenFolderTree(folder);

				if (folder.childFolders.Count == 1 && folder.scenes.Count == 0)
				{
					var onlyChild = folder.childFolders[0];

					folder.name = folder.name + "/" + onlyChild.name;
					folder.path = onlyChild.path;
					folder.scenes = onlyChild.scenes;
					folder.childFolders = onlyChild.childFolders;
				}
			}
		}

		#endregion


		// -----------------------------------------------------------------
		#region Draw Window

		private void OnGUI()
		{
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
			{
				DrawHorizontalLine();
				DrawFolderTree(sceneFolderTree);
			}
			EditorGUILayout.EndScrollView();
		}

		private void DrawFolderTree(Folder folder, int indentLevel = 0)
		{
			if (Preferences.GetDirectory(folder.path) == false)
				return;

			float indentPixels = indentLevel * 40;

			// Draw child folders.
			foreach (Folder childFolder in folder.childFolders)
			{
				// Draw folder name.
				DrawFolderHeader(childFolder, indentPixels);
				// Draw the contents of the folder recursively.
				DrawFolderTree(childFolder, indentLevel + 1);

				DrawHorizontalLine(indentPixels);
			}

			// Draw buttons for each scene.
			foreach (SceneData sceneData in folder.scenes)
			{
				DrawButtonsForScene(sceneData, indentPixels);
			}
		}

		private void DrawFolderHeader(Folder folder, float indent = 0)
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(indent);

				// Draw a button to show the directoty in the project window.
				if (GUILayout.Button(FolderIcon, FolderButtonStyle))
				{
					var dirObj = AssetDatabase.LoadAssetAtPath(folder.path, typeof(UnityEngine.Object));
					EditorGUIUtility.PingObject(dirObj);
					Selection.activeObject = dirObj;
				}

				// Draw a button to fold the directory.
				bool wasOpen = Preferences.GetDirectory(folder.path);
				bool isOpen = EditorGUILayout.Foldout(wasOpen, folder.name);
				if (isOpen != wasOpen)
				{
					Preferences.SetDirectory(folder.path, isOpen);
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawHorizontalLine(float indent = 0)
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(indent);
				EditorGUILayout.LabelField("---", SeparatorStyle);
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawButtonsForScene(SceneData sceneData, float indent = 0)
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
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(indent);
				GUILayout.Label("     " + sceneName, EditorStyles.whiteLargeLabel);
			}
			GUILayout.EndHorizontal();

			// BUTTONS
			GUILayout.BeginHorizontal();
			{
				GUILayout.Space(indent);

				// LOAD BUTTON
				if (scene.isLoaded && SceneManager.sceneCount == 1)
					GUI.enabled = false;

				var buttonStyle = LoadButtonStyle;
				buttonStyle.fixedWidth = Screen.width - (90 + indent + PlayButtonStyle.margin.horizontal * 2);
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
		}

		#endregion

		#region Styles & Icons

		// ---
		static GUIStyle _loadButtonStyle = null;
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
				return _loadButtonStyle;
			}
		}

		// ---
		static GUIStyle _addButtonStyle = null;
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

		static GUIContent _plusIcon = null;
		GUIContent PlusIcon
		{
			get
			{
				if (_plusIcon == null)
					_plusIcon = EditorGUIUtility.IconContent("Toolbar Plus");
				return _plusIcon;
			}
		}

		static GUIContent _minusIcon = null;
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
		static GUIStyle _playButtonStyle = null;
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

		static GUIContent _playIcon = null;
		GUIContent PlayIcon
		{
			get
			{
				if (_playIcon == null)
					_playIcon = EditorGUIUtility.IconContent("PlayButton");
				return _playIcon;
			}
		}

		// ---
		static GUIStyle _folderButtonStyle = null;
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

		static GUIContent _folderIcon = null;
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
		static GUIStyle _separatorStyle = null;
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
		#region Definitions

		/// <summary> Data from a folder in the project. </summary>
		private class Folder
		{
			public string path = "";
			public string name = "";
			public List<Folder> childFolders = new();
			public List<SceneData> scenes = new();

			public bool ContainsChild(string folderName)
			{
				foreach(var folder in childFolders)
				{
					if (folder.name == folderName)
						return true;
				}
				return false;
			}
			public Folder GetChild(string folderName)
			{
				foreach(var folder in childFolders)
				{
					if (folder.name == folderName)
						return folder;
				}
				return null;
			}
		}

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
				name = System.IO.Path.GetFileNameWithoutExtension(path);
			}
		}

		#endregion
	}
}
