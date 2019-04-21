using UnityEditor;
using UnityEngine;

namespace EGUI.Editor
{
    internal class MainFrameContainer : UserFrameContainer
    {
        private Node mRoot;

        private SceneFrame mSceneFrame;

        private HierarchyFrame mHierarchyFrame;

        private NodeFrame mNodeFrame;

        public void Setup(Node root)
        {
            mRoot = root;
            InitFrames();
        }

        protected override void OnGUI()
        {
            OnDrawMenuBar();
            OnDrawSeparatorLine();
            base.OnGUI();
        }

        protected override void OnResize()
        {
            if (mSceneFrame != null && mNodeFrame != null && mHierarchyFrame != null)
            {
                var rawHeight = mSceneFrame.rect.height + mHierarchyFrame.rect.height + UserSetting.FrameIntervalSpace;
                var rawWidth = mSceneFrame.rect.width;
                mSceneFrame.rect = new Rect(0, 0, rect.width, mSceneFrame.rect.height * rect.height / rawHeight);
                mHierarchyFrame.rect = new Rect(0, mSceneFrame.rect.yMax + UserSetting.FrameIntervalSpace, mHierarchyFrame.rect.width * rect.width / rawWidth, rect.height - mSceneFrame.rect.yMax - UserSetting.FrameIntervalSpace);
                mNodeFrame.rect = new Rect(mHierarchyFrame.rect.width + UserSetting.FrameIntervalSpace, mHierarchyFrame.rect.y, rect.width - mHierarchyFrame.rect.width - UserSetting.FrameIntervalSpace, mHierarchyFrame.rect.height);
            }
        }

        private void InitFrames()
        {
            mSceneFrame = new SceneFrame() { root = mRoot };
            mHierarchyFrame = new HierarchyFrame() { root = mRoot };
            mNodeFrame = new NodeFrame();
            frames = new UserFrame[] { mSceneFrame, mHierarchyFrame, mNodeFrame };

            mSceneFrame.rect = new Rect(0, UserSetting.FrameMenuBarHeight, rect.width, rect.height * 2 / 3);
            mHierarchyFrame.rect = new Rect(0, mSceneFrame.rect.yMax + UserSetting.FrameIntervalSpace, (rect.width - UserSetting.FrameIntervalSpace) / 2, rect.height - mSceneFrame.rect.yMax - UserSetting.FrameIntervalSpace);
            mNodeFrame.rect = new Rect(mHierarchyFrame.rect.width + UserSetting.FrameIntervalSpace, mHierarchyFrame.rect.y, rect.width - mHierarchyFrame.rect.width - UserSetting.FrameIntervalSpace, mHierarchyFrame.rect.height);

            UserSelection.onChange += () => 
            {
                var nodes = UserSelection.nodes;
                mNodeFrame.target = nodes != null && nodes.Length > 0 ? 
                    new PersistentObject(CoreUtil.CopyArray(nodes)) : null;
            };
        }

        private void OnDrawMenuBar()
        {
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = Event.current.GetTypeForControl(controlId);
            if (eventType == EventType.Repaint)
            {
                EditorStyles.toolbar.Draw(new Rect(0, 0, rect.width, UserSetting.FrameMenuBarHeight), GUIContent.none, controlId);
            }
            if (GUI.Button(new Rect(0, 0, 60, UserSetting.FrameMenuBarHeight), "File", EditorStyles.toolbarButton))
            {

            }
            mSceneFrame.preview = GUI.Toggle(new Rect(rect.width - 60, 0, 60, UserSetting.FrameMenuBarHeight), mSceneFrame.preview, "Preview", EditorStyles.toolbarButton);
        }

        private void OnDrawSeparatorLine()
        {
            var hlineRect = new Rect(mSceneFrame.rect.x, mSceneFrame.rect.yMax, mSceneFrame.rect.width, UserSetting.FrameIntervalSpace);
            var vlineRect = new Rect(mHierarchyFrame.rect.xMax, mHierarchyFrame.rect.y, UserSetting.FrameIntervalSpace, mHierarchyFrame.rect.height);
            var signNumber = 987654321;
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = Event.current.GetTypeForControl(controlId);
            switch (eventType)
            {
                case EventType.MouseMove:
                    {
                        if (rect.Contains(Event.current.mousePosition))
                        {
                            if (hlineRect.Contains(Event.current.mousePosition))
                            {
                                UserCursor.SetState(UserCursor.State.Vertical);
                                Event.current.Use();
                            }
                            else
                            {
                                if (UserCursor.GetState() == UserCursor.State.Vertical)
                                {
                                    UserCursor.SetState(UserCursor.State.Default);
                                    Event.current.Use();
                                }
                            }
                            if (vlineRect.Contains(Event.current.mousePosition))
                            {
                                UserCursor.SetState(UserCursor.State.Horizontal);
                                Event.current.Use();
                            }
                            else
                            {
                                if (UserCursor.GetState() == UserCursor.State.Horizontal)
                                {
                                    UserCursor.SetState(UserCursor.State.Default);
                                    Event.current.Use();
                                }
                            }
                        }
                        break;
                    }
                case EventType.MouseDown:
                    {
                        if (hlineRect.Contains(Event.current.mousePosition))
                        {
                            GUIUtility.hotControl = controlId - signNumber;
                            Event.current.Use();
                        }
                        else if (vlineRect.Contains(Event.current.mousePosition))
                        {
                            GUIUtility.hotControl = controlId + signNumber;
                            Event.current.Use();
                        }
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        if (GUIUtility.hotControl == controlId - signNumber)
                        {
                            var y = Event.current.mousePosition.y;
                            y = Mathf.Max(y, mSceneFrame.rect.y);
                            mSceneFrame.rect = new Rect(0, UserSetting.FrameMenuBarHeight, mSceneFrame.rect.width, y - UserSetting.FrameMenuBarHeight);
                            mHierarchyFrame.rect = new Rect(0, mSceneFrame.rect.yMax + UserSetting.FrameIntervalSpace, mHierarchyFrame.rect.width, rect.height - mSceneFrame.rect.yMax - UserSetting.FrameIntervalSpace);
                            mNodeFrame.rect = new Rect(mNodeFrame.rect.x, mHierarchyFrame.rect.y, mNodeFrame.rect.width, mHierarchyFrame.rect.height);
                            Event.current.Use();
                        }
                        if (GUIUtility.hotControl == controlId + signNumber)
                        {
                            var x = Event.current.mousePosition.x;
                            x = Mathf.Max(x, mHierarchyFrame.rect.x);
                            mHierarchyFrame.rect = new Rect(mHierarchyFrame.rect.x, mHierarchyFrame.rect.y, x, mHierarchyFrame.rect.height);
                            mNodeFrame.rect = new Rect(mHierarchyFrame.rect.width + UserSetting.FrameIntervalSpace, mHierarchyFrame.rect.y, rect.width - mHierarchyFrame.rect.width - UserSetting.FrameIntervalSpace, mHierarchyFrame.rect.height);
                            Event.current.Use();
                        }
                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (GUIUtility.hotControl == controlId - signNumber ||
                            GUIUtility.hotControl == controlId + signNumber)
                        {
                            GUIUtility.hotControl = 0;
                        }
                        break;
                    }
            }
        }
    }
}
