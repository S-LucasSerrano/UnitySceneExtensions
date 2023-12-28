using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.SceneManagement
{
	/// <summary> Custom inspector for the <see cref="LoadingScreen"/> class. </summary>
	[CustomEditor(typeof(LoadingScreen))]
	[CanEditMultipleObjects]
	public class LoadingScreenEditor : Editor
	{
		// -----------------------------------------------------------------
		#region Open On Double Click

		// When the user double clicks on the asset, open the scene.

		[UnityEditor.Callbacks.OnOpenAsset]
		private static bool OnDoubleClick(int instanceId, int line)
		{
			var obj = EditorUtility.InstanceIDToObject(instanceId);
			if (obj is LoadingScreen)
			{
				var loadingScreen = (LoadingScreen)obj;

				if (string.IsNullOrEmpty(loadingScreen.LoadingSceneName))
					return false;

				var scenePath = loadingScreen.LoadingScenePath;
				SceneManagerExtensions.SaveAndLoadScene(scenePath);

				return true;
			}

			return false;
		}

		#endregion


		// -----------------------------------------------------------------

		SerializedProperty sceneAssetProperty;


		private void OnEnable()
		{
			sceneAssetProperty = serializedObject.FindProperty("loadingSceneAsset");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(sceneAssetProperty);

			serializedObject.ApplyModifiedProperties();

			if (sceneAssetProperty.objectReferenceValue == null)
				return;

			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Open", GUILayout.Width(100)))
			{
				string scenePath = AssetDatabase.GetAssetPath(sceneAssetProperty.objectReferenceValue);
				SceneManagerExtensions.SaveAndLoadScene(scenePath);
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}
	}
}
