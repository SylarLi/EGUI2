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

        private bool mPreview;

        protected override bool scrollEnabled
        {
            get { return false; }
        }

        public bool preview
        {
            get { return mPreview; }
            set
            {
                mPreview = value;
                if (mSceneFrame != null) mSceneFrame.preview = mPreview;
            }
        }

        public void SetRoot(Node root)
        {
            mRoot = root;
            if (mSceneFrame == null || mHierarchyFrame == null || mNodeFrame == null)
            {
                mSceneFrame = new SceneFrame {preview = preview};
                mHierarchyFrame = new HierarchyFrame();
                mNodeFrame = new NodeFrame();
                frames = new UserFrame[] {mSceneFrame, mHierarchyFrame, mNodeFrame};
                mSceneFrame.rect = new Rect(0, 0, rect.width,
                    Mathf.RoundToInt(rect.height * 2 / 3));
                mHierarchyFrame.rect = new Rect(0, mSceneFrame.rect.yMax + UserSetting.FrameIntervalSpace,
                    Mathf.RoundToInt((rect.width - UserSetting.FrameIntervalSpace) / 2),
                    rect.height - mSceneFrame.rect.yMax - UserSetting.FrameIntervalSpace);
                mNodeFrame.rect = new Rect(mHierarchyFrame.rect.width + UserSetting.FrameIntervalSpace,
                    mHierarchyFrame.rect.y, rect.width - mHierarchyFrame.rect.width - UserSetting.FrameIntervalSpace,
                    mHierarchyFrame.rect.height);
                UserDatabase.selection.onChange += () =>
                {
                    var nodes = UserDatabase.selection.nodes;
                    mNodeFrame.target = nodes != null && nodes.Length > 0
                        ? new PersistentObject(CoreUtil.CopyArray(nodes))
                        : null;
                    if (nodes != null && nodes.Length > 0)
                    {
                        foreach (var node in nodes)
                        {
                            var current = node.parent;
                            while (current.parent != null)
                            {
                                UserDatabase.caches.SetHierarchyFoldout(current, true);
                                current = current.parent;
                            }
                        }

                        mHierarchyFrame.ScrollTo(UserDatabase.selection.node);
                    }
                };
            }

            mSceneFrame.root = mRoot;
            mHierarchyFrame.root = mRoot;
            UserDatabase.selection.onChange();
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            OnDrawSeparatorLine();
        }

        protected override void OnResize()
        {
            if (mSceneFrame != null && mNodeFrame != null && mHierarchyFrame != null)
            {
                var rawHeight = mSceneFrame.rect.height + mHierarchyFrame.rect.height +
                                UserSetting.FrameIntervalSpace;
                var rawWidth = mSceneFrame.rect.width;
                mSceneFrame.rect = new Rect(0, 0, rect.width,
                    Mathf.RoundToInt(mSceneFrame.rect.height * rect.height / rawHeight));
                mHierarchyFrame.rect = new Rect(0, mSceneFrame.rect.yMax + UserSetting.FrameIntervalSpace,
                    Mathf.RoundToInt(mHierarchyFrame.rect.width * rect.width / rawWidth),
                    rect.height - mSceneFrame.rect.yMax - UserSetting.FrameIntervalSpace);
                mNodeFrame.rect = new Rect(mHierarchyFrame.rect.width + UserSetting.FrameIntervalSpace,
                    mHierarchyFrame.rect.y, rect.width - mHierarchyFrame.rect.width - UserSetting.FrameIntervalSpace,
                    mHierarchyFrame.rect.height);
            }
        }

        private void OnDrawSeparatorLine()
        {
            HorizontalSeparatorLine(new Rect(mSceneFrame.rect.x, mSceneFrame.rect.yMax, mSceneFrame.rect.width,
                UserSetting.FrameIntervalSpace));
            VerticalSeparatorLine(new Rect(mHierarchyFrame.rect.xMax, mHierarchyFrame.rect.y,
                UserSetting.FrameIntervalSpace,
                mHierarchyFrame.rect.height));
        }

        private void HorizontalSeparatorLine(Rect lineRect)
        {
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = Event.current.GetTypeForControl(controlId);
            switch (eventType)
            {
                case EventType.MouseMove:
                {
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        if (lineRect.Contains(Event.current.mousePosition))
                        {
                            Cursor.SetState(Cursor.State.Vertical);
                            Event.current.Use();
                        }
                        else
                        {
                            if (Cursor.GetState() == Cursor.State.Vertical)
                            {
                                Cursor.SetState(Cursor.State.Default);
                                Event.current.Use();
                            }
                        }
                    }

                    break;
                }
                case EventType.MouseDown:
                {
                    if (Event.current.button == 0 &&
                        lineRect.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                    }

                    break;
                }
                case EventType.MouseDrag:
                {
                    if (GUIUtility.hotControl == controlId)
                    {
                        var y = Event.current.mousePosition.y;
                        y = Mathf.Max(y, mSceneFrame.rect.y);
                        mSceneFrame.rect = new Rect(0, 0, mSceneFrame.rect.width, y);
                        mHierarchyFrame.rect = new Rect(0, mSceneFrame.rect.yMax + UserSetting.FrameIntervalSpace,
                            mHierarchyFrame.rect.width,
                            rect.height - mSceneFrame.rect.yMax - UserSetting.FrameIntervalSpace);
                        mNodeFrame.rect = new Rect(mNodeFrame.rect.x, mHierarchyFrame.rect.y, mNodeFrame.rect.width,
                            mHierarchyFrame.rect.height);
                        Event.current.Use();
                    }


                    break;
                }
                case EventType.MouseUp:
                {
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }

                    break;
                }
            }
        }

        private void VerticalSeparatorLine(Rect lineRect)
        {
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = Event.current.GetTypeForControl(controlId);
            switch (eventType)
            {
                case EventType.MouseMove:
                {
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        if (lineRect.Contains(Event.current.mousePosition))
                        {
                            Cursor.SetState(Cursor.State.Horizontal);
                            Event.current.Use();
                        }
                        else
                        {
                            if (Cursor.GetState() == Cursor.State.Horizontal)
                            {
                                Cursor.SetState(Cursor.State.Default);
                                Event.current.Use();
                            }
                        }
                    }

                    break;
                }
                case EventType.MouseDown:
                {
                    if (lineRect.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.hotControl = controlId;
                        Event.current.Use();
                    }

                    break;
                }
                case EventType.MouseDrag:
                {
                    if (GUIUtility.hotControl == controlId)
                    {
                        var x = Event.current.mousePosition.x;
                        x = Mathf.Max(x, mHierarchyFrame.rect.x);
                        mHierarchyFrame.rect = new Rect(mHierarchyFrame.rect.x, mHierarchyFrame.rect.y, x,
                            mHierarchyFrame.rect.height);
                        mNodeFrame.rect = new Rect(mHierarchyFrame.rect.width + UserSetting.FrameIntervalSpace,
                            mHierarchyFrame.rect.y,
                            rect.width - mHierarchyFrame.rect.width - UserSetting.FrameIntervalSpace,
                            mHierarchyFrame.rect.height);
                        Event.current.Use();
                    }

                    break;
                }
                case EventType.MouseUp:
                {
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        Event.current.Use();
                    }

                    break;
                }
            }
        }
    }
}