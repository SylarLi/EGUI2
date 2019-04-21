using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace EGUI
{
    [Persistence]
    public class Node : Object, IEnumerable<Node>
    {
        [PersistentField]
        private string mName = "";

        public string name { get { return mName; } set { mName = value; } }

        [PersistentField]
        private Vector2 mLocalPosition = Vector2.zero;

        /// <summary>
        /// Local position.
        /// </summary>
        public Vector2 localPosition { get { return mLocalPosition; } set { if (mLocalPosition != value) { mLocalPosition = value; SetLayoutDirty(); } } }

        /// <summary>
        /// World position.
        /// </summary>
        public Vector2 worldPosition { get { Vector3 worldPos = localPosition; if (parent != null) worldPos = parent.local2WorldMatrix.MultiplyPoint(worldPos); return worldPos; } }

        [PersistentField]
        private float mLocalAngle = 0;

        /// <summary>
        /// Local z-Axis angle.
        /// </summary>
        public float localAngle { get { return mLocalAngle; } set { if (mLocalAngle != value) { mLocalAngle = value; SetLayoutDirty(); } } }

        [PersistentField]
        private Vector2 mLocalScale = Vector2.one;

        /// <summary>
        /// Local scale.
        /// </summary>
        public Vector2 localScale { get { return mLocalScale; } set { if (mLocalScale != value) { mLocalScale = value; SetLayoutDirty(); } } }

        [PersistentField]
        private Vector2 mPivot;

        /// <summary>
        /// Pivot.
        /// </summary>
        public Vector2 pivot
        {
            get
            {
                return mPivot;
            }
            set
            {
                if (mPivot != value)
                {
                    localPosition += new Vector2(
                        localScale.x * size.x * (value.x - mPivot.x),
                        localScale.y * size.y * (value.y - mPivot.y)
                    );
                    mPivot = value;
                    SetLayoutDirty();
                }
            }
        }

        [PersistentField]
        private Vector2 mSize;

        /// <summary>
        /// Node's Size.
        /// </summary>
        public Vector2 size
        {
            get
            {
                return mSize;
            }
            set
            {
                if (mSize != value)
                {
                    mSize = value;
                    SyncSize2StretchSize(stretchWidth, stretchHeight);
                    SetLayoutDirty();
                }
            }
        }

        [PersistentField]
        private Vector2 mStretchSize;

        /// <summary>
        /// Stretched node size
        /// </summary>
        public Vector2 stretchSize
        {
            get
            {
                return mStretchSize;
            }
            set
            {
                if (mStretchSize != value)
                {
                    mStretchSize = value;
                    SyncStretchSize2Size(stretchWidth, stretchHeight);
                    SetLayoutDirty();
                }
            }
        }

        [PersistentField]
        private bool mStretchWidth;

        /// <summary>
        /// is node width stretched ?
        /// </summary>
        public bool stretchWidth
        {
            get
            {
                return mStretchWidth;
            }
            set
            {
                if (mStretchWidth != value)
                {
                    mStretchWidth = value;
                    if (mStretchWidth)
                    {
                        SyncSize2StretchSize(true, false);
                    }
                    else
                    {
                        SyncStretchSize2Size(true, false);
                    }
                    SetLayoutDirty();
                }
            }
        }

        [PersistentField]
        private bool mStretchHeight;

        /// <summary>
        /// is node height stretched ?
        /// </summary>
        public bool stretchHeight
        {
            get
            {
                return mStretchHeight;
            }
            set
            {
                if (mStretchHeight != value)
                {
                    mStretchHeight = value;
                    if (mStretchHeight)
                    {
                        SyncSize2StretchSize(false, true);
                    }
                    else
                    {
                        SyncStretchSize2Size(false, true);
                    }
                    SetLayoutDirty();
                }
            }
        }

        [PersistentField]
        private Vector4 mPadding = Vector4.zero;

        /// <summary>
        /// padding of node. (x: left, y: top, z: right, w: bottom)
        /// </summary>
        public Vector4 padding { get { return mPadding; } set { if (mPadding != value) { mPadding = value; SetLayoutDirty(); } } }

        private Rect mLocalRect;

        /// <summary>
        /// local rect.
        /// </summary>
        public Rect localRect { get { TryRebuildLayout(); return mLocalRect; } }

        /// <summary>
        /// GUI layout matrix.
        /// </summary>
        private Matrix4x4 mTRS = Matrix4x4.identity;

        public Matrix4x4 TRS { get { TryRebuildLayout(); return mTRS; } }

        private Matrix4x4 mLocal2WorldMatrix = Matrix4x4.identity;

        /// <summary>
        /// Local to world matrix.
        /// </summary>
        public Matrix4x4 local2WorldMatrix { get { TryRebuildMatrix(); return mLocal2WorldMatrix; } }

        private Matrix4x4 mWorld2LocalMatrix = Matrix4x4.identity;

        /// <summary>
        /// World to local matrix.
        /// </summary>
        public Matrix4x4 world2LocalMatrix { get { TryRebuildMatrix(); return mWorld2LocalMatrix; } }

        [PersistentField]
        private bool mEnabled = true;

        public bool enabled { get { return mEnabled; } set { if (mEnabled != value) { mEnabled = value; RebuildActivation(); } } }

        [PersistentField]
        private bool mActive = true;

        public bool active { get { return mActive; } protected set { if (mActive = value) { mActive = value; OnActivationUpdate(); } } }

        private bool mLayoutDirty = true;

        private bool mMatrixDirty = true;

        private List<Leaf> mLeavesUpdate = new List<Leaf>();

        private List<Node> mChildrenUpdate = new List<Node>();

        public override void Update()
        {
            mLeavesUpdate.Clear();
            mLeavesUpdate.AddRange(mLeaves);
            for (int i = 0; i < mLeavesUpdate.Count; i++)
            {
                var leaf = mLeavesUpdate[i];
                if (leaf != null && leaf.active)
                {
                    leaf.Update();
                }
            }
            mChildrenUpdate.Clear();
            mChildrenUpdate.AddRange(mChildren);
            for (int i = 0; i < mChildrenUpdate.Count; i++)
            {
                var child = mChildrenUpdate[i];
                if (child != null && child.active)
                {
                    child.Update();
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            RemoveAllLeaves();
            mLeaves = null;
            RemoveAllChildren();
            mChildren = null;
            parent = null;
        }

        public void SetLayoutDirty()
        {
            mLayoutDirty = true;
            SetMatrixDirty();
        }

        public virtual void RebuildLayout()
        {
            mTRS = BuildTRSMatrix(this);
            mLocalRect = new Rect(0, 0,
                size.x - padding.x - padding.z,
                size.y - padding.y - padding.w);
        }

        protected void TryRebuildLayout()
        {
            if (mLayoutDirty)
            {
                RebuildLayout();
                mLayoutDirty = false;
            }
        }

        public void SetMatrixDirty()
        {
            mMatrixDirty = true;
            mChildren.ForEach(i => i.SetMatrixDirty());
        }

        public virtual void RebuildMatrix()
        {
            mLocal2WorldMatrix = BuildLocal2WorldMatrix(this);
            mWorld2LocalMatrix = mLocal2WorldMatrix.inverse;
        }

        protected void TryRebuildMatrix()
        {
            if (mMatrixDirty)
            {
                RebuildMatrix();
                mMatrixDirty = false;
            }
        }

        public void RebuildActivation()
        {
            var i = parent;
            while (i != null && i.active) i = i.parent;
            active = i == null && enabled;
        }

        protected void OnActivationUpdate()
        {
            mLeaves.ForEach(i =>
            {
                i.RebuildActivation();
            });
            mChildren.ForEach(i =>
            {
                i.RebuildActivation();
            });
        }

        protected void SyncSize2StretchSize(bool syncWidth, bool syncHeight)
        {
            if (parent != null)
            {
                if (syncWidth)
                {
                    mStretchSize.x = mSize.x / parent.size.x;
                }
                if (syncHeight)
                {
                    mStretchSize.y = mSize.y / parent.size.y;
                }
                mChildren.ForEach(i =>
                {
                    i.SyncStretchSize2Size(i.stretchWidth, i.stretchHeight);
                    i.SetLayoutDirty();
                });
            }
        }

        protected void SyncStretchSize2Size(bool syncWidth, bool syncHeight)
        {
            if (parent != null)
            {
                if (syncWidth)
                {
                    mSize.x = mStretchSize.x * parent.size.x;
                }
                if (syncHeight)
                {
                    mSize.y = mStretchSize.y * parent.size.y;
                }
                mChildren.ForEach(i =>
                {
                    i.SyncStretchSize2Size(i.stretchWidth, i.stretchHeight);
                    i.SetLayoutDirty();
                });
            }
        }

        #region Transform

        [PersistentField]
        private Node mParent;

        /// <summary>
        /// Node's parnet.
        /// </summary>
        public Node parent
        {
            get
            {
                return mParent;
            }
            set
            {
                if (mParent != value)
                {
                    if (mParent == this)
                    {
                        throw new InvalidOperationException();
                    }
                    if (mParent != null)
                    {
                        mParent.RemoveChild(this);
                    }
                    mParent = value;
                    if (mParent != null)
                    {
                        mParent.AddChild(this);
                    }
                    if (!disposed)
                    {
                        SetMatrixDirty();
                        RebuildActivation();
                        OnParentChanged();
                    }
                }
            }
        }

        [PersistentField]
        private List<Node> mChildren = new List<Node>();

        public IEnumerator<Node> GetEnumerator()
        {
            return mChildren.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mChildren.GetEnumerator();
        }

        /// <summary>
        /// Child count of this node.
        /// </summary>
        public int childCount
        {
            get
            {
                return mChildren.Count;
            }
        }

        public Node Find(string path)
        {
            Debug.Assert(path != null);
            var names = path.Split(new char[] { '\\', '/' });
            var node = this;
            int index = 0;
            while (node != null && 
                index < names.Length)
            {
                foreach (var child in node)
                {
                    if (child.name.Equals(names[index]))
                    {
                        node = child;
                        break;
                    }
                }
                index += 1;
            }
            return node;
        }

        public Node GetChild(int index)
        {
            return mChildren[index];
        }

        public Node GetChild(string name)
        {
            return mChildren.Find(node => node.name == name);
        }

        public bool ContainsChild(Node child)
        {
            return mChildren.Contains(child);
        }

        public void AddChild(Node child, int index = -1)
        {
            if (!ContainsChild(child))
            {
                if (index < 0) index = childCount;
                mChildren.Insert(index, child);
                child.parent = this;
            }
        }

        internal void RemoveChild(int index)
        {
            var child = mChildren[index];
            mChildren.RemoveAt(index);
            child.parent = null;
        }

        internal void RemoveChild(Node child)
        {
            if (ContainsChild(child))
            {
                mChildren.Remove(child);
                child.parent = null;
            }
        }

        internal void RemoveAllChildren()
        {
            for (int i = mChildren.Count - 1; i >= 0; i--)
            {
                mChildren[i].Dispose();
                mChildren.RemoveAt(i);
            }
        }

        public int GetSiblingIndex()
        {
            var index = -1;
            if (parent != null)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    if (parent.GetChild(i) == this)
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        public void SetSiblingIndex(int index)
        {
            if (parent != null)
            {
                index = index < 0 ? parent.childCount - 1 : index;
                index = Mathf.Clamp(index, 0, parent.childCount - 1);
                int current = GetSiblingIndex();
                if (current != index)
                {
                    for (int i = current; i < index; i++)
                    {
                        parent.mChildren[i] = parent.mChildren[i + 1];
                    }
                    for (int i = current; i > index; i--)
                    {
                        parent.mChildren[i] = parent.mChildren[i - 1];
                    }
                    parent.mChildren[index] = this;
                    OnSiblingIndexChanged();
                }
            }
        }

        public void SetAsFirstSibling()
        {
            SetSiblingIndex(0);
        }

        public void SetAsLastSibling()
        {
            SetSiblingIndex(-1);
        }

        public bool IsAncestorOf(Node node)
        {
            node = node.parent;
            while (node != null && node != this) { node = node.parent; }
            return node != null;
        }

        protected void OnParentChanged()
        {
            mLeaves.ForEach(i => i.OnNodeParentChanged());
        }

        protected void OnSiblingIndexChanged()
        {
            mLeaves.ForEach(i => i.OnNodeSiblingIndexChanged());
        }

        #endregion

        #region Component

        [PersistentField]
        private List<Leaf> mLeaves = new List<Leaf>();

        public T AddLeaf<T>() where T : Leaf
        {
            return (T)AddLeaf(typeof(T));
        }

        public Leaf AddLeaf(Type type)
        {
            Debug.Assert(GetLeaf(type) == null);
            var leaf = (Leaf)Activator.CreateInstance(type);
            mLeaves.Add(leaf);
            leaf.node = this;
            return leaf;
        }

        public void AddLeaf(Leaf leaf)
        {
            Debug.Assert(GetLeaf(leaf.GetType()) == null);
            Debug.Assert(!mLeaves.Contains(leaf));
            if (leaf.node != null)
            {
                leaf.node.RemoveLeaf(leaf);
                leaf.node = null;
            }
            mLeaves.Add(leaf);
            leaf.node = this;
        }

        public T GetLeaf<T>(bool includeInactive = true)
        {
            return (T)(object)GetLeaf(typeof(T), includeInactive);
        }

        public Leaf GetLeaf(Type type, bool includeInactive = true)
        {
            return mLeaves.Find(leaf => type.IsAssignableFrom(leaf.GetType()) && (includeInactive || leaf.active));
        }

        public T[] GetLeaves<T>(bool includeInactive = true)
        {
            return GetLeaves(typeof(T)).Select(leaf => (T)(object)leaf).ToArray();
        }

        public Leaf[] GetLeaves(Type type, bool includeInactive = true)
        {
            return mLeaves.Where(leaf => type.IsAssignableFrom(leaf.GetType()) && (includeInactive || leaf.active)).ToArray();
        }

        public Leaf[] GetAllLeaves(bool includeInactive = true)
        {
            return mLeaves.Where(leaf => includeInactive || leaf.active).ToArray();
        }

        public T GetLeafInAncestors<T>(bool includeInactive = true)
        {
            return (T)(object)GetLeafInAncestors(typeof(T), includeInactive);
        }

        public Leaf GetLeafInAncestors(Type type, bool includeInactive = true)
        {
            Leaf leaf = null;
            var node = parent;
            while (node != null && leaf == null)
            {
                leaf = node.GetLeaf(type, includeInactive);
                node = node.parent;
            }
            return leaf;
        }

        public T[] GetLeavesInAncestors<T>(bool includeInactive)
        {
            return Array.ConvertAll(GetLeavesInAncestors(typeof(T), includeInactive), i => (T)(object)i);
        }

        public Leaf[] GetLeavesInAncestors(Type type, bool includeInactive = true)
        {
            List<Leaf> leaves = new List<Leaf>();
            var node = parent;
            while (node != null)
            {
                var leaf = parent.GetLeaf(type, includeInactive);
                if (leaf != null)
                {
                    leaves.Add(leaf);
                }
                node = node.parent;
            }
            return leaves.ToArray();
        }

        public T GetLeafInChildren<T>(bool includeInactive)
        {
            return (T)(object)GetLeafInChildren(typeof(T), includeInactive);
        }

        public Leaf GetLeafInChildren(Type type, bool includeInactive)
        {
            Leaf leaf = null;
            foreach (var child in mChildren)
            {
                leaf = child.GetLeaf(type, includeInactive);
                if (leaf == null)
                {
                    leaf = child.GetLeafInChildren(type, includeInactive);
                }
                if (leaf != null)
                {
                    break;
                }
            }
            return leaf;
        }

        public T[] GetLeavesInChildren<T>(bool includeInactive)
        {
            return Array.ConvertAll(GetLeavesInChildren(typeof(T), includeInactive), i => (T)(object)i);
        }

        public Leaf[] GetLeavesInChildren(Type type, bool includeInactive)
        {
            List<Leaf> leaves = new List<Leaf>();
            foreach (var child in mChildren)
            {
                var leaf = child.GetLeaf(type, includeInactive);
                if (leaf != null)
                {
                    leaves.Add(leaf);
                }
                leaves.AddRange(child.GetLeavesInChildren(type, includeInactive));
            }
            return leaves.ToArray();
        }

        internal void RemoveLeaf<T>() where T : Leaf
        {
            RemoveLeaf(typeof(T));
        }

        internal void RemoveLeaf(Type type)
        {
            for (int i = mLeaves.Count - 1; i >= 0; i--)
            {
                var leaf = mLeaves[i];
                if (leaf.GetType() == type)
                {
                    leaf.node = null;
                    mLeaves.RemoveAt(i);
                }
            }
        }

        internal void RemoveLeaf(Leaf leaf)
        {
            Debug.Assert(mLeaves.Contains(leaf));
            leaf.node = null;
            mLeaves.Remove(leaf);
        }

        internal void RemoveAllLeaves()
        {
            for (int i = mLeaves.Count - 1; i >= 0; i--)
            {
                mLeaves[i].Dispose();
                mLeaves.RemoveAt(i);
            }
        }

        #endregion

        #region Utility

        public static Matrix4x4 BuildTRSMatrix(Node node)
        {
            var matrix = GUI.matrix;
            var offset = new Vector2(node.pivot.x * node.size.x - node.padding.x, node.pivot.y * node.size.y - node.padding.y);
            var newMatrix = Matrix4x4.Translate(node.localPosition - offset);
            GUI.matrix = newMatrix;
            var scale = node.localScale;
            if (scale.x == 0) scale.x = float.MinValue;
            if (scale.y == 0) scale.y = float.MinValue;
            GUIUtility.ScaleAroundPivot(scale, offset);
            GUIUtility.RotateAroundPivot(node.localAngle, node.localPosition);
            var ret = GUI.matrix;
            GUI.matrix = matrix;
            return ret;
        }

        public static Matrix4x4 BuildLocal2WorldMatrix(Node node)
        {
            var stack = new Stack<Node>();
            var current = node;
            while (current != null)
            {
                stack.Push(current);
                current = current.parent;
            }
            var matrix = Matrix4x4.identity;
            while (stack.Count > 0)
            {
                var pop = stack.Pop();
                matrix = matrix * pop.TRS;
            }
            return matrix;
        }

        public static Matrix4x4 BuildWorld2LocalMatrix(Node node)
        {
            return BuildLocal2WorldMatrix(node).inverse;
        }

        public static Matrix4x4 BuildLocal2WorldRotateMatrix(Node node)
        {
            var stack = new Stack<Node>();
            var current = node;
            while (current != null)
            {
                stack.Push(current);
                current = current.parent;
            }
            var matrix = Matrix4x4.identity;
            while (stack.Count > 0)
            {
                var item = stack.Pop();
                matrix = matrix * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, item.localAngle), Vector3.one);
            }
            return matrix;
        }

        #endregion
    }
}