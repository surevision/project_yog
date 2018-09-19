using PrefabEditor;
using UnityEditor;
using UnityEngine;

namespace PrefabEditor
{
    public struct ViewBookmark
    {
        public readonly Vector3 Pivot;
        public readonly Quaternion Rotation;
        public readonly float Size;

        public ViewBookmark(SceneView view) : this()
        {
            this.Size = view.size;
            this.Rotation = view.rotation;
            this.Pivot = view.pivot;
        }

        public ViewBookmark(float size, Quaternion rotation, Vector3 pivot) : this()
        {
            this.Size = size;
            this.Rotation = rotation;
            this.Pivot = pivot;
        }
    }
}

public static class SceneViewExtensions
{
    public static void ApplyBookmark(this SceneView view, ViewBookmark bookmark)
    {
        view.pivot = bookmark.Pivot;
        view.rotation = bookmark.Rotation;
        view.size = bookmark.Size;
    }
}
