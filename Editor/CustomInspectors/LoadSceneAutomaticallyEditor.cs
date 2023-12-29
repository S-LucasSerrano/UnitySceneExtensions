using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.SceneManagement
{
	/// <summary> Custom inspector for the <see cref="LoadSceneAutomatically"/> component. </summary>

    [CustomEditor(typeof(LoadSceneAutomatically))]
	[CanEditMultipleObjects]
    public class LoadSceneAutomaticallyEditor : Editor
    {
		private LoadSceneAutomatically component = null;

		private string[] transitionOptions = { "None", "Automatic", "Select" };
		int selectedTransitionOption = 0;


		// -----------------------------------------------------------------

		private void OnEnable()
		{
			component = target as LoadSceneAutomatically;

			if (component.useTransition == false)
				selectedTransitionOption = 0;
			else if (component.transitionEffect == null)
				selectedTransitionOption = 1;
			else
				selectedTransitionOption = 2;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.Space();
			DrawProperty(nameof(component.targetScene));
			DrawProperty(nameof(component.delay));

			EditorGUILayout.Space();
			DrawTransitionSelection();

			EditorGUILayout.Space();
			DrawLoadingScreenSelection();

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawProperty(string name)
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
		}

		private void DrawTransitionSelection()
		{ 
			selectedTransitionOption = EditorGUILayout.Popup("Use Transition", selectedTransitionOption, transitionOptions);

			if (selectedTransitionOption == 0)
			{
				component.useTransition = false;
			}
			else
			{
				component.useTransition = true;

				if (selectedTransitionOption == 1)
				{
					component.transitionEffect = null;
				}
				else
				{
					DrawProperty(nameof(component.transitionEffect));
				}
			}
		}

		private void DrawLoadingScreenSelection()
		{
			DrawProperty(nameof(component.useLoadingScreen));
			if (component.useLoadingScreen)
			{
				DrawProperty(nameof(component.loadingScreen));
			}
		}
	}
}
