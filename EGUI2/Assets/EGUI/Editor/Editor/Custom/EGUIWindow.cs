using System;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEditor;
using EGUI.UI;
using UnityEngine.SocialPlatforms;

namespace EGUI.Editor
{
    public class EGUIWindow : CustomWindow
    {
        [MenuItem("EGUI/Editor")]
        private static void Test()
        {
            GetWindow<EGUIWindow>().Show();
        }

        private MainFrameContainer container;

        private string file;

        protected override void OnEnable()
        {
            base.OnEnable();
            wantsMouseEnterLeaveWindow = true;
        }

        protected override void OnFocus()
        {
            base.OnFocus();
            if (container != null)
                container.focused = true;
        }

        protected override void OnLostFocus()
        {
            base.OnLostFocus();
            if (container != null)
                container.focused = false;
        }

        protected override void OnResize()
        {
            if (container != null)
                container.rect = new Rect(0, UserSetting.FrameMenuBarHeight, position.width,
                    position.height - UserSetting.FrameMenuBarHeight);
        }

        protected override void OnRender()
        {
            if (container == null)
            {
                container = new MainFrameContainer
                {
                    focused = focusedWindow == this,
                    rect = new Rect(0, UserSetting.FrameMenuBarHeight, position.width,
                        position.height - UserSetting.FrameMenuBarHeight)
                };
                container.SetRoot(root);
            }

            OnDrawMenuBar();
            container.OnDraw();
            Cursor.Update();
            OnEvent();
        }

        private void OnDrawMenuBar()
        {
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = Event.current.GetTypeForControl(controlId);
            if (eventType == EventType.Repaint)
            {
                EditorStyles.toolbar.Draw(new Rect(0, 0, position.width, UserSetting.FrameMenuBarHeight),
                    GUIContent.none,
                    controlId);
            }

            var displayName = string.IsNullOrEmpty(file) ? Locale.L_Untitled : Path.GetFileNameWithoutExtension(file);
            if (string.IsNullOrEmpty(file) || Command.Floating()) displayName += "*";
            EditorGUI.LabelField(new Rect(70, 1, position.width - 60 - 60, position.height), displayName,
                EditorStyles.miniLabel);
            if (GUI.Button(new Rect(0, 0, 60, UserSetting.FrameMenuBarHeight), "File", EditorStyles.toolbarButton))
                ShowFileMenu();

            container.preview = GUI.Toggle(new Rect(position.width - 60, 0, 60, UserSetting.FrameMenuBarHeight),
                container.preview, "Preview", EditorStyles.toolbarButton);
        }

        private void OnEvent()
        {
            switch (Event.current.type)
            {
                case EventType.MouseLeaveWindow:
                {
                    if (Cursor.GetState() != Cursor.State.Default)
                    {
                        Cursor.SetState(Cursor.State.Default);
                        Event.current.Use();
                    }
                    break;
                }
                case EventType.MouseMove:
                {
                    if (container == null || !container.rect.Contains(Event.current.mousePosition))
                    {
                        if (Cursor.GetState() != Cursor.State.Default)
                        {
                            Cursor.SetState(Cursor.State.Default);
                            Event.current.Use();
                        }
                    }

                    break;
                }
                case EventType.MouseDown:
                {
                    Event.current.Use();
                    break;
                }
                case EventType.MouseUp:
                {
                    if (UserDragDrop.dragging)
                    {
                        UserDragDrop.StopDrag();
                        Cursor.ResetState();
                        Event.current.Use();
                    }

                    break;
                }
                case EventType.MouseDrag:
                {
                    if (UserDragDrop.dragging)
                    {
                        if (!new Rect(0, 0, position.width, position.height).Contains(Event.current.mousePosition))
                        {
                            UserDragDrop.StopDrag();
                            Cursor.ResetState();
                        }

                        Event.current.Use();
                    }

                    break;
                }
            }
        }

        private void ShowFileMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(Locale.L_New), false, () =>
            {
                if (string.IsNullOrEmpty(file) || !File.Exists(file) || Command.Floating())
                {
                    var ret = EditorUtility.DisplayDialogComplex(Locale.L_SceneModifiedTitle,
                        Locale.L_SceneModifiedContent,
                        Locale.L_Save, Locale.L_DontSave, Locale.L_Cancel);
                    if (ret == 0)
                        Save();
                    else if (ret == 2) return;
                }

                New();
            });
            menu.AddSeparator("");
            if (string.IsNullOrEmpty(file) || !File.Exists(file) || Command.Floating())
                menu.AddItem(new GUIContent(Locale.L_Save), false, Save);
            else
                menu.AddDisabledItem(new GUIContent(Locale.L_Save));

            menu.AddItem(new GUIContent(Locale.L_SaveAs), false, SaveAs);

            menu.AddSeparator("");
            menu.AddItem(new GUIContent(Locale.L_Open), false, () =>
            {
                if (string.IsNullOrEmpty(file) || !File.Exists(file) || Command.Floating())
                {
                    var ret = EditorUtility.DisplayDialogComplex(Locale.L_SceneModifiedTitle,
                        Locale.L_SceneModifiedContent,
                        Locale.L_Save, Locale.L_DontSave, Locale.L_Cancel);
                    if (ret == 0)
                        Save();
                    else if (ret == 2) return; 
                }

                Open();
            });
            menu.DropDown(new Rect(0, UserSetting.FrameMenuBarHeight, 0, 0));
        }

        private void New()
        {
            ClearCache();
            if (container != null)
                container.SetRoot(root);
        }

        private void Save()
        {
            if (!string.IsNullOrEmpty(file) && File.Exists(file))
            {
                UserUtil.SaveFile(root, file);
                Command.Anchor();
            }
            else
            {
                SaveAs();
            }
        }

        private void SaveAs()
        {
            var path = UserUtil.SaveFileAs(root);
            if (!string.IsNullOrEmpty(path))
            {
                Command.Anchor();
                file = path;
            }
        }

        private void Open()
        {
            Node node;
            string path;
            UserUtil.LoadFile(out node, out path);
            if (node != null)
            {
                ClearCache();
                root = node;
                file = path;
                if (container != null)
                    container.SetRoot(root);
            }
        }

        protected override Data NewData()
        {
            return new EGUIData();
        }

        protected override void ClearCache()
        {
            base.ClearCache();
            UserDatabase.Clear();
            file = null;
        }

        [Persistence]
        private class EGUIData : Data
        {
            [PersistentField] private UserDatabase mUserDatabase;

            public EGUIData() : base()
            {
            }
        }
    }
}