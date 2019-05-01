using System;
using System.IO;
using System.Net;
using EGUI.UI;
using UnityEditor;
using UnityEngine;

namespace EGUI.Editor
{
    public class PersistenceTestWindow : ArchetypeWindow
    {
        [MenuItem("EGUI/Test Persistence")]
        private static void Test()
        {
            GetWindow<PersistenceTestWindow>().Show();
        }

        public override void OnBeforeSerialize()
        {
        }

        private bool isInit;
        private Vector2 m_ScrollPosition = Vector2.zero;
        private Vector2 m_ScrollPosition1 = Vector2.zero;
        private UndoRedoMarker marker;

        private void OnEnable()
        {
            marker = CreateInstance<UndoRedoMarker>();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Group+"))
                Undo.IncrementCurrentGroup();
            if (GUILayout.Button("Record"))
                marker.Mark();
            EditorGUILayout.LabelField(Undo.GetCurrentGroup().ToString());
        }

//        private void OnGUI()
//        {
////            this.m_ScrollPosition = GUILayout.BeginScrollView(this.m_ScrollPosition);
//            GUI.BeginGroup(new Rect(0, 0, position.width, position.height));
//            this.m_ScrollPosition1 = GUILayout.BeginScrollView(this.m_ScrollPosition);
//            if (Event.current.type == EventType.repaint)
//            {
//                var style = new GUIStyle(GUI.skin.button);
//                style.overflow = new RectOffset(0, 0, 0, 0);
//                style.Draw(new Rect(0, 0, 100, 50), GUIContent.none, false, false, false, false);
//            }
//            GUILayout.EndScrollView();
//            GUI.EndGroup();
////            GUILayout.EndScrollView();
//        }

//        protected override void OnRender()
//        {
//            if (GUILayout.Button("Export"))
//            {
//                var raw = GUI.skin.horizontalScrollbarThumb.normal.background;
//                var bytes = raw.GetRawTextureData();
//                var t = new Texture2D(raw.width, raw.height, TextureFormat.RGBA32, false);
//                t.LoadRawTextureData(bytes);
//                var b = t.EncodeToPNG();
//                File.WriteAllBytes("Assets/EGUI/Editor/Core/Resource/scrollbar_thumb.png", b);
//            }
//            if (!isInit)
//            {
//                isInit = true;
//                var button = DefaultControl.CreateButton(root);
//            }
//            
//            root.Update();
//            Cursor.Update();
//        }
    }
}