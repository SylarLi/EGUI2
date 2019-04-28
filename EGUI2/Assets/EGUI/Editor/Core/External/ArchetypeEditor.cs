using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EGUI
{
    [CustomEditor(typeof(Archetype))]
    public class ArchetypeEditor : UnityEditor.Editor
    {
//        private Node root;
//
//        private EditorWindow inspector;
//
//        private PropertyInfo position;
//
//        private void OnEnable()
//        {
//            var type = CoreUtil.FindType("UnityEditor.InspectorWindow");
//            if (type != null)
//            {
//                var objects = Resources.FindObjectsOfTypeAll(type);
//                if (objects.Length > 0)
//                    inspector = (EditorWindow) objects[0];
//                position = type.GetProperty("position");
//            }
//
//            if (inspector == null || position == null)
//                Debug.Log("Can not get window size, root will be set to 100x100 size.");
//        }

        public override void OnInspectorGUI()
        {
              // 在Scrollview(inspector自带)中预览出现一些偏移，原因未知
//            var archetype = target as Archetype;
//            if (root == null && archetype.data != null)
//            {
//                root = new Persistence().Deserialize<Node>(archetype.data);
//            }
//
//            var size = new Vector2(400, 400);
//            if (inspector != null && position != null)
//            {
//                var pos = (Rect) position.GetValue(inspector, null);
//                size = new Vector2(pos.width, pos.height);
//            }
//
//            if (root != null)
//            {
//                root.size = size;
//                root.Update();
//                Cursor.Update();
//            }
        }
    }
}