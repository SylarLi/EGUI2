using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace EGUI
{
    [Persistence]
    public class Node : Object, IEnumerable<Node>
    {
        [PersistentField] private string mName = "";

        public string name
        {
            get { return mName; }
            set { mName = value; }
        }

        [PersistentField] private Vector2 mLocalPosition = Vector2.zero;

        /// <summary>
        /// local position.
        /// </summary>
        public Vector2 localPosition
        {
            get { return mLocalPosition; }
            set
            {
                if (mLocalPosition != value)
                {
                    mLocalPosition = value;
                    SetLayoutDirty();
                }
            }
        }

        /// <summary>
        /// World position.
        /// </summary>
        public Vector2 worldPosition
        {
            get
            {
                Vector3 worldPos = localPosition;
                if (parent != null)
                    worldPos = parent.local2WorldMatrix.MultiplyPoint(worldPos);
                return worldPos;
            }
            set
            {
                var localPos = value;
                if (parent != null)
                    localPos = parent.world2LocalMatrix.MultiplyPoint(localPos);
                localPosition = localPos;
            }
        }

        [PersistentField] private float mLocalAngle = 0;

        /// <summary>
        /// Local Z-Axis angle.
        /// </summary>
        public float localAngle
        {
            get { return mLocalAngle; }
            set
            {
                if (Mathf.Abs(mLocalAngle - value) > float.Epsilon)
                {
                    mLocalAngle = value;
                    SetLayoutDirty();
                }
            }
        }

        /// <summary>
        /// World Z-Axis angle.
        /// </summary>
        public float worldAngle
        {
            get
            {
                var angle = localAngle;
                var node = parent;
                while (node != null)
                {
                    angle += node.localAngle;
                    node = node.parent;
                }

                return FormatRotation(angle);
            }
            set
            {
                var node = parent;
                while (node != null)
                {
                    value -= node.localAngle;
                    node = node.parent;
                }

                localAngle = FormatRotation(value);
            }
        }

        [PersistentField] private Vector2 mLocalScale = Vector2.one;

        /// <summary>
        /// Local scale.
        /// </summary>
        public Vector2 localScale
        {
            get { return mLocalScale; }
            set
            {
                if (mLocalScale != value)
                {
                    mLocalScale = value;
                    SetLayoutDirty();
                }
            }
        }

        /// <summary>
        /// World scale.
        /// </summary>
        public Vector2 worldScale
        {
            get
            {
                var scale = localScale;
                if (parent != null)
                {
                    var pScale = parent.worldScale;
                    scale.x *= pScale.x;
                    scale.y *= pScale.y;
                }

                return scale;
            }
            set
            {
                if (parent != null)
                {
                    var pScale = parent.worldScale;
                    if (pScale.x > 0)
                        value.x /= pScale.x;
                    if (pScale.y > 0)
                        value.y /= pScale.y;
                }

                localScale = value;
            }
        }

        [PersistentField] private Vector2 mPivot;

        /// <summary>
        /// Pivot.
        /// </summary>
        public Vector2 pivot
        {
            get { return mPivot; }
            set
            {
                if (mPivot != value)
                {
                    mPivot = value;
                    SetLayoutDirty();
                }
            }
        }

        [PersistentField] private Vector2 mSize;

        /// <summary>
        /// Size.
        /// </summary>
        public Vector2 size
        {
            get { return mSize; }
            set
            {
                if (mSize != value)
                {
                    var delta = value - mSize;
                    mSize = value;
                    foreach (var child in this)
                    {
                        var min = child.anchorMin;
                        var max = child.anchorMax;
                        child.anchoredPosition += new Vector2(min.x * delta.x, min.y * delta.y);
                        child.size += new Vector2((max.x - min.x) * delta.x, (max.y - min.y) * delta.y);
                    }

                    SetLayoutDirty();
                }
            }
        }

        [PersistentField] private Vector2 mAnchorMin = Vector2.zero;

        /// <summary>
        /// Minimum anchor(top left).
        /// </summary>
        public Vector2 anchorMin
        {
            get { return mAnchorMin; }
            set
            {
                if (mAnchorMin != value)
                {
                    mAnchorMin = value;
                    SetLayoutDirty();
                }
            }
        }

        [PersistentField] private Vector2 mAnchorMax = Vector2.zero;

        /// <summary>
        /// Maximum anchor(bottom right).
        /// </summary>
        public Vector2 anchorMax
        {
            get { return mAnchorMax; }
            set
            {
                if (mAnchorMax != value)
                {
                    mAnchorMax = value;
                    SetLayoutDirty();
                }
            }
        }

        /// <summary>
        /// The pivot's local position relative to the referenced anchor.
        /// </summary>
        public Vector2 anchoredPosition
        {
            get { return localPosition + GetPivotOffset() - GetMinAnchorOffset(); }
            set { localPosition = value - GetPivotOffset() + GetMinAnchorOffset(); }
        }

        /// <summary>
        /// The offset of the top left corner of the rectangle relative to the top left anchor.
        /// </summary>
        public Vector2 offsetMin
        {
            get { return localPosition - GetMinAnchorOffset(); }
            set
            {
                var newPos = value + GetMinAnchorOffset();
                size += (localPosition - newPos);
                localPosition = newPos;
            }
        }

        /// <summary>
        /// The offset of the bottom right corner of the rectangle relative to the bottom right anchor.
        /// </summary>
        public Vector2 offsetMax
        {
            get { return GetMaxAnchorOffset() - localPosition - size; }
            set { size = GetMaxAnchorOffset() - localPosition - value; }
        }

        private Rect mLocalRect;

        /// <summary>
        /// local rect.
        /// </summary>
        public Rect localRect
        {
            get
            {
                TryRebuildLayout();
                return mLocalRect;
            }
        }

        private Matrix4x4 mTRS = Matrix4x4.identity;

        /// <summary>
        /// TRS matrix.
        /// </summary>
        public Matrix4x4 TRS
        {
            get
            {
                TryRebuildLayout();
                return mTRS;
            }
        }

        private Matrix4x4 mLocal2WorldMatrix = Matrix4x4.identity;

        /// <summary>
        /// Local to world matrix.
        /// </summary>
        public Matrix4x4 local2WorldMatrix
        {
            get
            {
                TryRebuildMatrix();
                return mLocal2WorldMatrix;
            }
        }

        private Matrix4x4 mWorld2LocalMatrix = Matrix4x4.identity;

        /// <summary>
        /// World to local matrix.
        /// </summary>
        public Matrix4x4 world2LocalMatrix
        {
            get
            {
                TryRebuildMatrix();
                return mWorld2LocalMatrix;
            }
        }

        /// <summary>
        /// GUI TRS matrix
        /// </summary>
        private Matrix4x4 mGTRS = Matrix4x4.identity;

        public Matrix4x4 GTRS
        {
            get
            {
                TryRebuildLayout();
                return mGTRS;
            }
        }

        private Matrix4x4 mGUIMatrix = Matrix4x4.identity;

        /// <summary>
        /// GUI matrix
        /// </summary>
        public Matrix4x4 GUIMatrix
        {
            get
            {
                TryRebuildMatrix();
                return mGUIMatrix;
            }
        }

        [PersistentField] private bool mEnabled = true;

        public bool enabled
        {
            get { return mEnabled; }
            set
            {
                if (mEnabled != value)
                {
                    mEnabled = value;
                    RebuildActivation();
                }
            }
        }

        [PersistentField] private bool mActive = true;

        public bool active
        {
            get { return mActive; }
            protected set
            {
                if (mActive != value)
                {
                    mActive = value;
                    OnActivationUpdate();
                }
            }
        }

        private bool mLayoutDirty = true;

        private bool mMatrixDirty = true;

        private List<Leaf> mLeavesUpdate = new List<Leaf>();

        private List<Node> mChildrenUpdate = new List<Node>();

        public override void Update()
        {
            mLeavesUpdate.Clear();
            mLeavesUpdate.AddRange(mLeaves);
            var i = 0;
            for (; i < mLeavesUpdate.Count; i++)
            {
                var leaf = mLeavesUpdate[i];
                if (leaf != null && leaf.active)
                {
                    leaf.Update();
                }
            }

            mChildrenUpdate.Clear();
            mChildrenUpdate.AddRange(mChildren);
            for (i = 0; i < mChildrenUpdate.Count; i++)
            {
                var child = mChildrenUpdate[i];
                if (child != null && child.active)
                {
                    child.Update();
                }
            }
        }

        internal override void MarkInternalDisposed(bool value)
        {
            if (mInternalDisposed != value)
            {
                mInternalDisposed = value;
                mChildren.ForEach(i => i.MarkInternalDisposed(value));
                mLeaves.ForEach(i => i.MarkInternalDisposed(value));
                RebuildActivation();
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
            mGTRS = BuildGTRSMatrix(this);
            mLocalRect = new Rect(0, 0, size.x, size.y);
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
            mGUIMatrix = BuildGUIMatrix(this);
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
            if (IsNull())
            {
                active = false;
                return;
            }

            var i = parent;
            while (i != null && i.active) i = i.parent;
            active = i == null && enabled;
        }

        protected void OnActivationUpdate()
        {
            mLeaves.ForEach(i => { i.RebuildActivation(); });
            mChildren.ForEach(i => { i.RebuildActivation(); });
        }

        #region Transform

        [PersistentField] private Node mParent;

        /// <summary>
        /// Node's parent.
        /// </summary>
        public Node parent
        {
            get { return mParent; }
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

                    if (!IsNull())
                    {
                        SetMatrixDirty();
                        RebuildActivation();
                        OnParentChanged();
                    }
                }
            }
        }

        public void SetParent(Node parent, bool worldPositionStays = true)
        {
            if (this.parent != parent)
            {
                if (worldPositionStays)
                {
                    var pos = worldPosition;
                    var angle = worldAngle;
                    var scale = worldScale;
                    this.parent = parent;
                    worldAngle = angle;
                    worldScale = scale;
                    worldPosition = pos;
                }
                else
                {
                    this.parent = parent;
                }
            }
        }

        [PersistentField] private List<Node> mChildren = new List<Node>();

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
            get { return mChildren.Count; }
        }

        public Node Find(string path)
        {
            Debug.Assert(path != null);
            var names = path.Split('\\', '/');
            var node = this;
            var index = 0;
            while (index < names.Length)
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

        public string GetPath()
        {
            var sb = new StringBuilder();
            var node = this;
            while (node.parent != null)
            {
                sb.Insert(0, node.name);
                sb.Insert(0, "/");
                node = node.parent;
            }

            if (sb.Length > 0)
                sb.Remove(0, 1);
            return sb.ToString();
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
                var current = GetSiblingIndex();
                if (current != index)
                {
                    for (var i = current; i < index; i++)
                    {
                        parent.mChildren[i] = parent.mChildren[i + 1];
                    }

                    for (var i = current; i > index; i--)
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
            while (node != null && node != this)
            {
                node = node.parent;
            }

            return node != null;
        }

        public Vector2 TransformPoint(Vector2 position)
        {
            return local2WorldMatrix.MultiplyPoint(position);
        }

        public Vector2 InverseTransformPoint(Vector2 position)
        {
            return world2LocalMatrix.MultiplyPoint(position);
        }

        public bool ContainsLocalPosition(Vector2 localPosition)
        {
            return localRect.Contains(localPosition);
        }

        public bool ContainsWorldPosition(Vector2 worldPosition)
        {
            return localRect.Contains(InverseTransformPoint(worldPosition));
        }

        private void OnParentChanged()
        {
            mLeaves.ForEach(i => i.OnNodeParentChanged());
        }

        private void OnSiblingIndexChanged()
        {
            mLeaves.ForEach(i => i.OnNodeSiblingIndexChanged());
        }

        private Vector2 GetPivotOffset()
        {
            return new Vector2(pivot.x * size.x, pivot.y * size.y);
        }

        private Vector2 GetMinAnchorOffset()
        {
            return parent != null
                ? new Vector2(anchorMin.x * parent.size.x, anchorMin.y * parent.size.y)
                : Vector2.zero;
        }

        private Vector2 GetMaxAnchorOffset()
        {
            return parent != null
                ? new Vector2(anchorMax.x * parent.size.x, anchorMax.y * parent.size.y)
                : Vector2.zero;
        }

        #endregion

        #region Component

        [PersistentField] private List<Leaf> mLeaves = new List<Leaf>();

        public T AddLeaf<T>() where T : Leaf
        {
            return (T) AddLeaf(typeof(T));
        }

        public Leaf AddLeaf(Type type)
        {
            Debug.Assert(GetLeaf(type) == null);
            var leaf = (Leaf) Activator.CreateInstance(type);
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
            return (T) (object) GetLeaf(typeof(T), includeInactive);
        }

        public Leaf GetLeaf(Type type, bool includeInactive = true)
        {
            return mLeaves.Find(leaf => type.IsAssignableFrom(leaf.GetType()) && (includeInactive || leaf.active));
        }

        public T[] GetLeaves<T>(bool includeInactive = true)
        {
            return GetLeaves(typeof(T)).Select(leaf => (T) (object) leaf).ToArray();
        }

        public Leaf[] GetLeaves(Type type, bool includeInactive = true)
        {
            return mLeaves.Where(leaf => type.IsAssignableFrom(leaf.GetType()) && (includeInactive || leaf.active))
                .ToArray();
        }

        public Leaf[] GetAllLeaves(bool includeInactive = true)
        {
            return mLeaves.Where(leaf => includeInactive || leaf.active).ToArray();
        }

        public T GetLeafInAncestors<T>(bool includeInactive = true)
        {
            return (T) (object) GetLeafInAncestors(typeof(T), includeInactive);
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
            return Array.ConvertAll(GetLeavesInAncestors(typeof(T), includeInactive), i => (T) (object) i);
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
            return (T) (object) GetLeafInChildren(typeof(T), includeInactive);
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
            return Array.ConvertAll(GetLeavesInChildren(typeof(T), includeInactive), i => (T) (object) i);
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

        public static Matrix4x4 BuildGTRSMatrix(Node node)
        {
            var matrix = GUI.matrix;
            var offset = node.GetPivotOffset();
            var newMatrix = Matrix4x4.Translate(node.localPosition);
            GUI.matrix = newMatrix;
            var scale = node.localScale;
            if (scale.x == 0) scale.x = float.MinValue;
            if (scale.y == 0) scale.x = float.MinValue;
            GUIUtility.ScaleAroundPivot(scale, offset);
            GUIUtility.RotateAroundPivot(node.localAngle, node.localPosition + offset);
            var ret = GUI.matrix;
            GUI.matrix = matrix;
            return ret;
        }

        public static Matrix4x4 BuildGUIMatrix(Node node)
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
                matrix = matrix * pop.GTRS;
            }

            return matrix;
        }

        public static Matrix4x4 BuildTRSMatrix(Node node)
        {
            return Matrix4x4.TRS(node.localPosition, Quaternion.Euler(0, 0, node.localAngle),
                new Vector3(node.localScale.x, node.localScale.y, 1));
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

        public static Matrix4x4 BuildGUIRotationMatrix(Node node)
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
                var rawMatrix = GUI.matrix;
                GUI.matrix = Matrix4x4.identity;
                GUIUtility.RotateAroundPivot(item.localAngle, item.localPosition);
                matrix = matrix * GUI.matrix;
                GUI.matrix = rawMatrix;
            }

            return matrix;
        }

        public static float FormatRotation(float angle)
        {
            angle = angle % 360;
            if (angle < 0)
                angle += 360;
            return angle;
        }

        #endregion
    }
}