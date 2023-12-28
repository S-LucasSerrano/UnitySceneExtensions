using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.SceneManagement
{
	/// <summary> Scriptable singleton that saves the data that the <see cref="SceneListWindow"/> needs to persist between sessions. </summary>
	
	[FilePath("ProjectSettings/SceneLoaderWindow.asset", FilePathAttribute.Location.ProjectFolder)]
	public class SceneListPreferences : ScriptableSingleton<SceneListPreferences>
	{
		[SerializeField]
		List<string> directories = new List<string>();
		[SerializeField]
		List<bool> foldedValues = new List<bool>();

		public bool IsDirOpen (string dir)
		{
			int i = GetIndexOfDir(dir);
			return foldedValues[i];
		}

		public void SetDirFolded (string dir, bool value)
		{
			int i = GetIndexOfDir(dir);
			foldedValues[i] = value;

			Save(true);
		}

		private int GetIndexOfDir(string dir)
		{
			if (!directories.Contains(dir))
				directories.Add(dir);

			if (foldedValues.Count < directories.Count)
			{
				for (int i = foldedValues.Count; i < directories.Count; i++)
				{
					foldedValues.Add(true);
				}
			}

			return directories.IndexOf(dir);
		}
	}
}
