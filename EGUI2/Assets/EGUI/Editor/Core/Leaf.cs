using System;

namespace EGUI
{
    [Persistence]
    public abstract class Leaf : Object
    {
        [PersistentField]
        private bool mIsStart;

        [PersistentField]
        private Node mNode;

        public Node node { get { return mNode; } set { if (mNode != value) { mNode = value; if (mNode == null) mIsStart = false; RebuildActivation(); } } }

        [PersistentField]
        private bool mEnabled = true;

        public bool enabled { get { return mEnabled; } set { if (mEnabled != value) { mEnabled = value; RebuildActivation(); } } }

        [PersistentField]
        private bool mActive;

        public bool active { get { return mActive; } protected set { if (mActive != value) { mActive = value; if (mActive && !mIsStart) { mIsStart = true; OnStart(); } if (mActive) OnEnable(); else OnDisable(); } } }

        internal override void MarkInternalDisposed(bool value)
        {
            if (mInternalDisposed != value)
            {
                mInternalDisposed = value;
                RebuildActivation();
            }
        }
        
        public override void Dispose()
        {
            if (active)
            {
                OnDisable();
            }
            base.Dispose();
            if (node != null)
            {
                node.RemoveLeaf(this);
                node = null;
            }
        }

        public virtual void OnStart() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnNodeParentChanged() { }

        public virtual void OnNodeSiblingIndexChanged() { }

        public void RebuildActivation()
        {
            active = enabled && (node != null && node.active);
        }

        public T GetLeaf<T>(bool includeInactive = true)
        {
            return node.GetLeaf<T>(includeInactive);
        }

        public Leaf GetLeaf(Type type, bool includeInactive = true)
        {
            return node.GetLeaf(type, includeInactive);
        }

        public T[] GetLeaves<T>(bool includeInactive = true)
        {
            return node.GetLeaves<T>(includeInactive);
        }

        public Leaf[] GetLeaves(Type type, bool includeInactive = true)
        {
            return node.GetLeaves(type, includeInactive);
        }

        public Leaf[] GetAllLeaves(bool includeInactive = true)
        {
            return node.GetAllLeaves(includeInactive);
        }

        public T GetLeafInAncestors<T>(bool includeInactive = true)
        {
            return node.GetLeafInAncestors<T>(includeInactive);
        }

        public Leaf GetLeafInAncestors(Type type, bool includeInactive = true)
        {
            return node.GetLeafInAncestors(type, includeInactive);
        }

        public T[] GetLeavesInAncestors<T>(bool includeInactive)
        {
            return node.GetLeavesInAncestors<T>(includeInactive);
        }

        public Leaf[] GetLeavesInAncestors(Type type, bool includeInactive = true)
        {
            return node.GetLeavesInAncestors(type, includeInactive);
        }

        public T GetLeafInChildren<T>(bool includeInactive)
        {
            return node.GetLeafInChildren<T>(includeInactive);
        }

        public Leaf GetLeafInChildren(Type type, bool includeInactive)
        {
            return node.GetLeafInChildren(type, includeInactive);
        }

        public T[] GetLeavesInChildren<T>(bool includeInactive)
        {
            return node.GetLeavesInChildren<T>(includeInactive);
        }

        public Leaf[] GetLeavesInChildren(Type type, bool includeInactive)
        {
            return node.GetLeavesInChildren(type, includeInactive);
        }
    }
}
