using System;
using System.Net;
using EGUI.UI;
using UnityEngine;
using UnityEditor;
using Canvas = UnityEngine.Canvas;

namespace EGUI.Editor
{
    public abstract class CustomWindow : EditorWindow, ISerializationCallbackReceiver
    {
        [NonSerialized] private Data mData;

        [SerializeField] private byte[] mDataBytes;

        [SerializeField] private Vector2 mScrollPos = Vector2.zero;

        private Data data
        {
            get
            {
                if (mDataBytes != null && mDataBytes.Length > 0)
                {
                    mData = new Persistence().Deserialize<Data>(mDataBytes);
                    mDataBytes = null;
                }

                mData = mData ?? NewData();
                return mData;
            }
        }

        protected Node root
        {
            get { return data.root; }
            set { data.root = value; }
        }

        protected virtual Data NewData()
        {
            return new Data();
        }

        protected virtual bool undoRedoEnabled
        {
            get { return true; }
        }

        protected virtual void Awake()
        {
            ClearCache();
        }

        protected virtual void OnEnable()
        {
            wantsMouseMove = true;
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnFocus()
        {
        }

        protected virtual void OnLostFocus()
        {
        }

        protected virtual void OnResize()
        {
            root.size = new Vector2(position.width, position.height);
        }

        public virtual void OnAfterDeserialize()
        {
        }

        public virtual void OnBeforeSerialize()
        {
            mDataBytes = new Persistence().Serialize(data);
        }

        protected virtual void OnPreRender()
        {
        }

        protected virtual void OnRender()
        {
            root.Update();
            Cursor.Update();
        }

        protected virtual void OnPostRender()
        {
        }

        private void OnGUI()
        {
            if (Vector2.Distance(data.size, position.size) > UserSetting.DistanceComparisionTolerance)
            {
                data.size = position.size;
                OnResize();
            }

            OnPreRender();
            OnRender();
            OnPostRender();

            if (undoRedoEnabled &&
                Event.current.type == EventType.KeyDown &&
                Event.current.shift)
            {
                if (Event.current.keyCode == KeyCode.Z)
                {
                    Command.PerformUndo();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Y)
                {
                    Command.PerformRedo();
                    Event.current.Use();
                }
            }
        }

        protected virtual void ClearCache()
        {
            if (mData != null)
            {
                mData.Clear();
                mData = null;
            }
        }

        [Persistence]
        protected class Data
        {
            [PersistentField] private Node mRoot;

            public Node root
            {
                get { return mRoot; }
                set { mRoot = value; }
            }

            [PersistentField] private Vector2 mSize = Vector2.zero;

            public Vector2 size
            {
                get { return mSize; }
                set { mSize = value; }
            }

            /// <summary>
            /// Declare for persistence.
            /// </summary>
            [PersistentField] private Command mCommand;

            /// <summary>
            /// Declare for persistence.
            /// </summary>
            [PersistentField] private Database mDatabase;

            public Data()
            {
                mRoot = new Node() {name = "Root"};
                mRoot.AddLeaf<EGUI.UI.Canvas>();
                mRoot.AddLeaf<EGUI.UI.EventSystem>();
            }

            public void Clear()
            {
                Command.Clear();
                Database.Clear();
                mRoot = null;
            }
        }
    }
}