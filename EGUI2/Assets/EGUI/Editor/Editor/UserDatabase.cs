using System;
using System.Collections.Generic;
using UnityEngine;

namespace EGUI.Editor
{
    internal class UserDatabase
    {
        public static void Clear()
        {
            caches.Clear();
            selection.nodes = null;
            highlight.node = null;
        }

        public static Selection selection = new Selection();
        
        public static Highlight highlight = new Highlight();

        public class Selection
        {
            public delegate void OnChange();

            public OnChange onChange = () => { };

            private Node[] mNodes;

            public Node[] nodes
            {
                get { return mNodes; }
                set
                {
                    mNodes = value;
                    onChange();
                }
            }

            public Node node
            {
                get { return nodes != null && nodes.Length > 0 ? nodes[nodes.Length - 1] : null; }
                set { nodes = value != null ? new Node[] {value} : null; }
            }
        }

        public class Highlight
        {
            public delegate void OnChange();

            public OnChange onChange = () => { };
            
            private Node mNode;

            public Node node
            {
                get { return mNode; }
                set
                {
                    mNode = value;
                    onChange();
                }
            }
        }

        public static CacheData caches = new CacheData();

        public class CacheData
        {
            private List<Type> mDefaultPropertyType = new List<Type>();

            private Dictionary<Type, UserPropertyDrawer> mPropertyDrawers = new Dictionary<Type, UserPropertyDrawer>();

            private Dictionary<Type, UserDrawer> mUserDrawers = new Dictionary<Type, UserDrawer>();

            private List<FoldData> mFoldouts = new List<FoldData>();

            private Dictionary<Node, bool> mHierarchyFoldouts = new Dictionary<Node, bool>();

            public bool IsDefaultPropertyType(Type propertyType)
            {
                if (mDefaultPropertyType.Count == 0)
                {
                    mDefaultPropertyType.Add(typeof(Vector2));
                    mDefaultPropertyType.Add(typeof(Vector3));
                    mDefaultPropertyType.Add(typeof(Vector4));
                    mDefaultPropertyType.Add(typeof(Rect));
                    mDefaultPropertyType.Add(typeof(Bounds));
                    mDefaultPropertyType.Add(typeof(Color));
                    mDefaultPropertyType.Add(typeof(UnityEngine.Object));
                    mDefaultPropertyType.Add(typeof(AnimationCurve));
                    mDefaultPropertyType.Add(typeof(Color32));
                    mDefaultPropertyType.Add(typeof(Object));
                    mDefaultPropertyType.Add(typeof(bool));
                    mDefaultPropertyType.Add(typeof(int)); 
                    mDefaultPropertyType.Add(typeof(long));
                    mDefaultPropertyType.Add(typeof(float));
                    mDefaultPropertyType.Add(typeof(double));
                    mDefaultPropertyType.Add(typeof(string));
                }
    
                return mDefaultPropertyType.Contains(propertyType) ||
                       propertyType.IsEnum ||
                       propertyType.IsSubclassOf(typeof(UnityEngine.Object)) ||
                       propertyType.IsSubclassOf(typeof(Object));
            }

            public UserPropertyDrawer GetPropertyDrawer(Type propertyType)
            {
                UserPropertyDrawer drawer = null;
                if (mPropertyDrawers.Count == 0)
                {
                    var customTypes = CoreUtil.FindSubTypes(typeof(UserPropertyDrawer));
                    foreach (var type in customTypes)
                    {
                        var attributes = type.GetCustomAttributes(typeof(UserPropertyDrawerAttribute), true);
                        var drawerType = attributes.Length > 0
                            ? ((UserPropertyDrawerAttribute) attributes[0]).type
                            : null;
                        if (drawerType != null)
                        {
                            var instance = (UserPropertyDrawer) CoreUtil.CreateInstance(type, null);
                            mPropertyDrawers.Add(drawerType, instance);
                        }
                    }
                }

                if (propertyType.IsArray)
                {
                    drawer = mPropertyDrawers[typeof(Array)];
                }
                else if (mPropertyDrawers.ContainsKey(propertyType))
                {
                    drawer = mPropertyDrawers[propertyType];
                }

                return drawer;
            }

            public UserDrawer GetUserDrawer(Type leafType)
            {
                if (mUserDrawers.Count == 0)
                {
                    var customTypes = CoreUtil.FindSubTypes(typeof(UserDrawer));
                    foreach (var type in customTypes)
                    {
                        var attributes = type.GetCustomAttributes(typeof(UserDrawerAttribute), true);
                        var drawerType = attributes.Length > 0 ? ((UserDrawerAttribute) attributes[0]).type : null;
                        if (drawerType != null)
                        {
                            var instance = (UserDrawer) CoreUtil.CreateInstance(type, null);
                            mUserDrawers.Add(drawerType, instance);
                        }
                    }
                }

                if (!mUserDrawers.ContainsKey(leafType))
                {
                    mUserDrawers.Add(leafType, new UserDrawer());
                }

                return mUserDrawers[leafType];
            }

            public bool GetPropertyFoldout(PersistentProperty persistentProperty)
            {
                var ret = mFoldouts.Find(i =>
                    i.objectType == persistentProperty.persistentObject.type &&
                    i.propertyPath == persistentProperty.propertyPath);
                if (ret == null)
                {
                    ret = new FoldData()
                    {
                        objectType = persistentProperty.persistentObject.type,
                        propertyPath = persistentProperty.propertyPath
                    };
                    mFoldouts.Add(ret);
                }

                return ret.foldout;
            }

            public void SetPropertyFoldout(PersistentProperty persistentProperty, bool foldout)
            {
                var ret = mFoldouts.Find(i =>
                    i.objectType == persistentProperty.persistentObject.type &&
                    i.propertyPath == persistentProperty.propertyPath);
                if (ret != null)
                {
                    ret.foldout = foldout;
                }
            }

            public bool GetHierarchyFoldout(Node node)
            {
                return mHierarchyFoldouts.ContainsKey(node) && mHierarchyFoldouts[node];
            }

            public void SetHierarchyFoldout(Node node, bool foldout)
            {
                mHierarchyFoldouts[node] = foldout;
            }

            public void Clear()
            {
                mDefaultPropertyType.Clear();
                mPropertyDrawers.Clear();
                mUserDrawers.Clear();
                mFoldouts.Clear();
                mHierarchyFoldouts.Clear();
            }

            private class FoldData
            {
                public Type objectType;

                public string propertyPath;

                public bool foldout = false;
            }
        }
    }
}