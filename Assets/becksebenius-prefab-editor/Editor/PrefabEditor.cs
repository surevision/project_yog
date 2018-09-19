/*
 	PrefabEditor.cs
 	Author: Beck Sebenius
 	Contact: rseben at gmail dot com
 	
 	Description:
 		PrefabEditor.cs creates a hotkey to edit a prefab in an empty scene, and allows
 	you to use Ctrl+S to save changes to that prefab.
 	
 	* Opens the PrefabEditor scene when CTRL+E is pressed on a prefab,
 		then creates an instance of the prefab.
 		
 	* If the PrefabEditor scene doesn't exist next to PrefabEditor.cs, one is created.
 	
 	* Watches for asset saves on the PrefabEditor scene, and saves changes 
 		to the currently edited prefab.
*/

using UnityEngine.SceneManagement;

namespace PrefabEditor
{
	using UnityEngine;
	using UnityEditor;
	using System.IO;
	using UnityEditor.SceneManagement;

// Scriptable Object used to find monoscript, which aids us in finding
//	the PrefabEditor scene
// This lets us put the PrefabEditor folder wherever we like :)
	public class PrefabEditor : EditorWindow
	{
		const string PE_OnOpenAction = "PrefabEditor-OnOpenAction";
		const string PE_WindowPosition = "PrefabEditor-OnOpenAction";
		const string PE_WindowPositionX = PE_WindowPosition + "-X";
		const string PE_WindowPositionY = PE_WindowPosition + "-Y";

		public string PreviousScenePath;
		public string PreviousSceneName;
		public bool firstFrame; // Used to trigger automatically actions

		private void OnEnable()
		{
			position = new Rect(
				EditorPrefs.GetInt(PE_WindowPositionX, 7),
				EditorPrefs.GetInt(PE_WindowPositionY, 136),
				200,
				250);
			minSize = maxSize = position.size;
			titleContent = new GUIContent("Prefab Editor");
			firstFrame = true;
		}

		private void OnGUI()
		{
			EditorPrefs.SetInt(PE_WindowPositionX, (int) position.x);
			EditorPrefs.SetInt(PE_WindowPositionY, (int) position.y);
			
			EditorGUILayout.BeginVertical();

			EditorGUILayout.Space();

			GUILayout.Label("You are in a temporary scene. Saving this scene will save the prefab instead.", EditorStyles.helpBox);

			EditorGUILayout.Space();

			GUILayout.Label("Reset position, rotation and zoom", EditorStyles.centeredGreyMiniLabel);

			GUILayout.BeginHorizontal();
			if (ButtonAction("Reset View to Origin"))
			{
				ResetViewToOrigin();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (ButtonAction("Reset View to Prefab"))
			{
				ResetViewToPrefab();
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.Label("Reset only position", EditorStyles.centeredGreyMiniLabel);

			GUILayout.BeginHorizontal();
			if (ButtonAction("Move View to Origin"))
			{
				MoveViewToOrigin();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (ButtonAction("Move View to Prefab"))
			{
				MoveViewToPrefab();
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.Label("Return to  " + PreviousSceneName, EditorStyles.centeredGreyMiniLabel);

			if (GUILayout.Button("Finish Edit", EditorStyles.miniButton))
			{
				EditorGUILayout.EndVertical();

				if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

				EditorSceneManager.OpenScene(PreviousScenePath, OpenSceneMode.Single);
				Close();

				SceneView.lastActiveSceneView.ApplyBookmark(PrefabEditorAssetProcessor.SceneViewToRestore);
				SceneView.RepaintAll();
			}

			if (GUILayout.Button("Save and Finish Edit", EditorStyles.miniButton))
			{
				EditorSceneManager.SaveOpenScenes();

				EditorGUILayout.EndVertical();

				if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

				EditorSceneManager.OpenScene(PreviousScenePath, OpenSceneMode.Single);
				Close();

				SceneView.lastActiveSceneView.ApplyBookmark(PrefabEditorAssetProcessor.SceneViewToRestore);
				SceneView.RepaintAll();
			}

			firstFrame = false;
		}

		private bool ButtonAction(string key)
		{
			string onOpenAction = EditorPrefs.GetString(PE_OnOpenAction, "");
			bool status = onOpenAction == key;

			if (GUILayout.Button(key, EditorStyles.miniButton)) return true;

			bool newStatus = GUILayout.Toggle(status, new GUIContent("Auto", "Execute automatically after start editing a prefab"), GUILayout.Width(50));
			if (newStatus != status)
			{
				if (newStatus) EditorPrefs.SetString(PE_OnOpenAction, key);
				else EditorPrefs.SetString(PE_OnOpenAction, "");
			}
			else if (firstFrame && status)
			{
				return true;
			}
			return false;
		}

		private static void MoveViewToPrefab()
		{
			SceneView.lastActiveSceneView.ApplyBookmark(
				PrefabEditorAssetProcessor.SceneViewToMove(
					PrefabEditorAssetProcessor.CurrentEditingPrefab.transform.position));

			SceneView.RepaintAll();
		}

		private static void MoveViewToOrigin()
		{
			SceneView.lastActiveSceneView.ApplyBookmark(
				PrefabEditorAssetProcessor.SceneViewToMove(
					Vector3.zero));

			SceneView.RepaintAll();
		}

		private static void ResetViewToPrefab()
		{
			SceneView.lastActiveSceneView.ApplyBookmark(
				PrefabEditorAssetProcessor.SceneViewToReset(
					PrefabEditorAssetProcessor.CurrentEditingPrefab.transform.position));

			SceneView.RepaintAll();
		}

		private static void ResetViewToOrigin()
		{
			SceneView.lastActiveSceneView.ApplyBookmark(
				PrefabEditorAssetProcessor.SceneViewToReset(
					Vector3.zero));

			SceneView.RepaintAll();
		}

		private void Update()
		{
			if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path == PrefabEditorAssetProcessor.ScenePath) return;

			Close();
		}

		public void SetPreviousScene(string path)
		{
			var fileinfo = new FileInfo(path);
			PreviousSceneName = fileinfo.Name;
			PreviousScenePath = path;
		}
	}
}
