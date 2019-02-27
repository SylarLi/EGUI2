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

        private void OnGUI()
        {
            if (GUILayout.Button("Test"))
            {
                var node = new Node();
                node.localPosition = new Vector2(100, 50);
                var sobj = new SerializedObject(new object[] { node });
                Debug.Assert(sobj.type == typeof(Node));
                Debug.Assert(sobj.GetValue<Node>() == node);
                var sprop = sobj.Find("localPosition");
                Debug.Assert(sprop.GetValue<Vector2>() == node.localPosition);
                sprop.SetValue(new Vector2(1, 2));
                Debug.Assert(sprop.GetValue<Vector2>() == new Vector2(1, 2));
                Debug.Assert(sprop.GetValue<Vector2>() == node.localPosition);
                var spropx = sprop.Find("x");
                spropx.SetValue(3f);
                Debug.Assert(sprop.GetValue<Vector2>() == new Vector2(3, 2));
                Debug.Assert(sprop.GetValue<Vector2>() == node.localPosition);
            }
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
