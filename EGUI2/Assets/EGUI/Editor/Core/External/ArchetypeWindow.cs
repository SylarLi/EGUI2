using System;
using System.Net;
using EGUI.UI;
using UnityEngine;
using UnityEditor;
using Canvas = UnityEngine.Canvas;

namespace EGUI.Editor
{
    public abstract class ArchetypeWindow : EditorWindow, ISerializationCallbackReceiver
    {
        [NonSerialized] private Data mData;

        [SerializeField] private byte[] mDataBytes;

        [SerializeField] private Vector2 mScrollPos = Vector2.zero;

        [SerializeField] private UndoRedoMarker mUndoRedoMarker;

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
            if (mUndoRedoMarker == null) 
                mUndoRedoMarker = CreateInstance<UndoRedoMarker>();
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            Command.onCommandPushed -= OnCommandPushed;
            Command.onCommandPushed += OnCommandPushed;
        }

        protected virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Command.onCommandPushed -= OnCommandPushed;
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

//            if (undoRedoEnabled &&
//                Event.current.type == EventType.KeyDown &&
//                Event.current.shift)
//            {
//                if (Event.current.keyCode == KeyCode.Z)
//                {
//                    Command.PerformUndo();
//                    Event.current.Use();
//                }
//                else if (Event.current.keyCode == KeyCode.Y)
//                {
//                    Command.PerformRedo();
//                    Event.current.Use();
//                }
//            }
        }

        private void OnUndoRedoPerformed()
        {
            if (undoRedoEnabled)
            {
                switch (mUndoRedoMarker.last)
                {
                    case UndoRedoMarker.UndoRedo.Undo:
                        Command.PerformUndo();
                        break;
                    case UndoRedoMarker.UndoRedo.Redo:
                        Command.PerformRedo();
                        break;
                }

                mUndoRedoMarker.last = UndoRedoMarker.UndoRedo.None;
                Repaint();
            }
        }

        private void OnCommandPushed(Command command)
        {
            Undo.RecordObject(mUndoRedoMarker, "UndoRedoProxyObject");
            mUndoRedoMarker.Mark();
        }

        protected virtual void ClearCache()
        {
            if (mData != null)
            {
                mData.Clear();
                mData = null;
            }

            if (mUndoRedoMarker != null)
                mUndoRedoMarker.Clear();
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