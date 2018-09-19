using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrefabEditor
{
    [InitializeOnLoad]
    public class PrefabEditorAssetProcessor :
#if !UNITY_3_5
        UnityEditor.
#endif
        AssetModificationProcessor
    {
        private const string MenuItemPath = "Assets/Edit Prefab %e";

        public static readonly string ScenePath;
        private static readonly string ScenePathTemplate;

        public static GameObject CurrentEditingPrefab { get; private set; }

        public static ViewBookmark SceneViewToRestore { get; private set; }

        static PrefabEditorAssetProcessor()
        {
            // Create prefab editor instance so that we can find it's monoscript location
            //	that way we can find its sister path for the PrefabEditor scene.
            var instance = ScriptableObject.CreateInstance<PrefabEditor>();
            var monoscript = MonoScript.FromScriptableObject(instance);
            var path = AssetDatabase.GetAssetPath(monoscript);

            //Derive scene path from asset path
            ScenePath = path.Substring(0, path.Length - 3) + ".unity";
            ScenePathTemplate = path.Substring(0, path.Length - 3) + "Template.unity";

            //Cleanup instance
            Object.DestroyImmediate(instance);

            //Register Double-click listener
            EditorApplication.projectWindowItemOnGUI += (guid, area) =>
            {
                if (Event.current.type != EventType.MouseDown
                    || Event.current.button != 0
                    || Event.current.clickCount != 2
                    || !area.Contains(Event.current.mousePosition)) return;

                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var fileInfo = new FileInfo(assetPath);

                if (fileInfo.Extension != ".prefab") return;

                Event.current.Use();
                var go = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                EditPrefab(go);
            };
        }

        [MenuItem(MenuItemPath, true)]
        public static bool ValidateEditPrefab()
        {
            var prefab = Selection.activeGameObject;

            // No gameobject selected
            if (!prefab) return false;

            // Selected game object is not a prefab
            return PrefabUtility.GetPrefabType(prefab) == PrefabType.Prefab;
        }

        [MenuItem(MenuItemPath)]
        public static void EditPrefab()
        {
            EditPrefab(Selection.activeGameObject);
        }
        
        public static ViewBookmark SceneViewToReset(Vector3 pivot)
        {
            return new ViewBookmark(
                5,
                Quaternion.LookRotation(new Vector3(-0.2f, -0.1f, -0.2f)),
                pivot);
        }

        public static ViewBookmark SceneViewToMove(Vector3 pivot)
        {
            return new ViewBookmark(
                SceneView.lastActiveSceneView.size,
                SceneView.lastActiveSceneView.rotation,
                pivot);
        }

        private static void EditPrefab(GameObject prefab)
        {
            //Passed a null reference
            if (!prefab)
            {
                return;
            }

            // Is game object a prefab?
            if (PrefabUtility.GetPrefabType(prefab) != PrefabType.Prefab)
            {
                return;
            }

            // Save currently open scene
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            CurrentEditingPrefab = prefab;

            // Open the backtrack window
            var currentScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            if (currentScenePath != ScenePath)
            {
                SceneViewToRestore = new ViewBookmark(SceneView.lastActiveSceneView);
                var pEditor = EditorWindow.GetWindow<PrefabEditor>(true);
                pEditor.SetPreviousScene(currentScenePath);
            }

            //Open the PrefabEditor scene
            EditorSceneManager.OpenScene(ScenePathTemplate, OpenSceneMode.Single);

            //Overwrite existing scene
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);

            //Instantiate the prefab and select it
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Selection.activeGameObject = instance;

            //Focus our scene view camera to aid in editing
            if (SceneView.lastActiveSceneView)
            {
                SceneView.lastActiveSceneView.Focus();
            }
        }

        public static string[] OnWillSaveAssets(string[] paths)
        {
            //Only perform prefab saving if we're in the PrefabEditor scene
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != ScenePath)
            {
                return paths;
            }

            //Make sure this save pass is actually trying to save the PrefabEditor scene
            if (!paths.Contains(ScenePath))
            {
                return paths;
            }

            //If we hit this point, it means we have the prefab editor open
            //	and it is in the process of being saved.
            var allGameObjects = Object.FindObjectsOfType(typeof(GameObject));
            foreach (var obj in allGameObjects)
            {
                var go = obj as GameObject;

                //Is this gameobject a prefab instance?
                if (PrefabUtility.GetPrefabType(go) != PrefabType.PrefabInstance)
                {
                    continue;
                }

                //Is this the root of the prefab?
                if (PrefabUtility.FindPrefabRoot(go) != go)
                {
                    continue;
                }

                //Apply changes
                PrefabUtility.ReplacePrefab(go, PrefabUtility.GetPrefabParent(go), ReplacePrefabOptions.ConnectToPrefab);
            }

            return paths;
        }
    }
}
