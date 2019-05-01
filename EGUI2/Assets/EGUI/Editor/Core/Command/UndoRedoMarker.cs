using System;
using UnityEngine;

namespace EGUI
{
    [Serializable]
    public class UndoRedoMarker : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private int id = 0;

        [NonSerialized] private int bid = int.MinValue;

        private UndoRedo mLast = UndoRedo.None;

        public UndoRedo last
        {
            get { return mLast; }
            set { mLast = value; }
        }

        public void Mark()
        {
            id += 1;
        }

        public void Clear()
        {
            UnityEditor.Undo.ClearUndo(this);
            id = 0;
            bid = int.MinValue;
        }

        public void OnBeforeSerialize()
        {
            bid = id;
        }

        public void OnAfterDeserialize()
        {
            if (id == bid - 1)
                last = UndoRedo.Undo;
            else if (id == bid + 1)
                last = UndoRedo.Redo;
        }

        public enum UndoRedo
        {
            None,
            Undo,
            Redo,
        }
    }
}