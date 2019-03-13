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

        private void OnGUI()
        {
            if (GUILayout.Button("Test"))
            {
                
            }
            if (pobj == null)
            {
                var node = new Node();
                node.localPosition = new Vector2(1, 2);
                node.name = "A";
                var node1 = new Node();
                node1.localPosition = new Vector2(1, 4);
                node1.name = "B";
                nodes = new Node[] { node, node1 };
                pobj = new PersistentObject(nodes);
            }
            var prop = pobj.Find("localPosition");
            PersistentGUILayout.PropertyField(prop);
            var prop1 = pobj.Find("name");
            PersistentGUILayout.PropertyField(prop1);
            if (moduleBytes != null && moduleBytes.Length > 0)
            {
                mRoot = new Persistence().Deserialize<Node>(moduleBytes);
                moduleBytes = null;
            }
            else if (mRoot == null)
            {
                mRoot = new Node();
                mRoot.AddLeaf<EGUI.UI.Canvas>();
                mRoot.AddLeaf<EventSystem>();
                var textfield_1 = DefaultControl.CreateTextField(mRoot);
                textfield_1.onInputValueChanged += value => Debug.Log(value);
                var button_2 = DefaultControl.CreateButton(mRoot);
                button_2.node.localPosition = new Vector2(0, 100);
            }
            mRoot.Update();
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
