using UnityEngine;
using UnityEditor;
using EGUI.UI;

namespace EGUI.Editor
{
    public class TestWindow : EditorWindow, ISerializationCallbackReceiver
    {
        [MenuItem("Test/1")]
        private static void Test()
        {
            GetWindow<TestWindow>().Show();
        }

        private Node mRoot;

        [SerializeField]
        byte[] moduleBytes;

        Node[] nodes;

        PersistentObject pobj;

        MainFrameContainer container;

        Rect size;

        private void OnGUI()
        {
            //if (GUILayout.Button("Test"))
            //{
                
            //}
            if (pobj == null)
            {
                var node = new Node();
                node.localPosition = new Vector2(1, 2);
                node.name = "A";
                node.AddLeaf<Drawer>();
                node.AddLeaf<Text>();
                var node1 = new Node();
                node1.localPosition = new Vector2(1, 4);
                node1.name = "B";
                nodes = new Node[] { node };
                pobj = new PersistentObject(nodes);
            }
            
            if (moduleBytes != null && moduleBytes.Length > 0)
            {
                mRoot = new Persistence().Deserialize<Node>(moduleBytes);
                moduleBytes = null;
            }
            else if (mRoot == null)
            {
                mRoot = new Node();
                mRoot.size = new Vector2(position.width, position.height);
                mRoot.AddLeaf<EGUI.UI.Canvas>();
                mRoot.AddLeaf<EventSystem>();
                var button_2 = DefaultControl.CreateButton(mRoot);
                button_2.node.localPosition = new Vector2(0, 0);
            }
            if (container == null)
            {
                container = new MainFrameContainer();
                container.focused = focusedWindow == this;
                container.rect = new Rect(0, 0, position.width, position.height);
                container.Setup(mRoot);
            }
            if (size.width != position.width ||
                size.height != position.height)
            {
                mRoot.size = new Vector2(position.width, position.height);
                container.rect = new Rect(0, 0, position.width, position.height);
                size = position;
            }
            container.OnDraw();
            OnCommonEvent();
            UserCursor.Update();
        }

        private void OnCommonEvent()
        {
            switch (Event.current.type)
            {
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
                            UserCursor.ResetState();
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
                                UserCursor.ResetState();
                            }
                            Event.current.Use();
                        }
                        break;
                    }
            }
        }

        private void OnFocus()
        {
            if (container != null)
                container.focused = true;
        }

        private void OnLostFocus()
        {
            if (container != null)
                container.focused = false;
        }

        private void OnEnable()
        {
            wantsMouseMove = true;
        }

        public void OnAfterDeserialize() { }

        public void OnBeforeSerialize()
        {
            if (mRoot != null)
                moduleBytes = new Persistence().Serialize(mRoot);
        }
    }
}
