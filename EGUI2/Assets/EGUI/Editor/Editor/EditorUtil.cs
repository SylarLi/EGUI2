using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace EGUI.Editor
{
    public sealed class EditorUtil
    {
        public static string GetNiceDisplayName(string name)
        {
            if (name.Length <= 2)
                return name;
            name = Regex.Replace(name, @"_(\w)", match => match.Groups[1].Value.ToUpper());
            name = Regex.Replace(name, @"^m([A-Z])", "$1");
            name = name.Substring(0, 1).ToUpper() + 
                Regex.Replace(name.Substring(1), @"[A-Z]", " $0");
            return name;
        }

        #region EditorGUI

        public static void PropertyField(GUIContent label, PersistentProperty persistentProperty, bool includeChildren, params GUILayoutOption[] options)
        {
            var propertyType = persistentProperty.type;
            var propertyDrawer = Caches.GetDrawer(propertyType);
            if (propertyDrawer != null)
            {
                var foldoutLabel = new GUIContent(GetNiceDisplayName(persistentProperty.memberInfo.Name));
                var foldout = EditorGUILayout.Foldout(Caches.GetFoldout(persistentProperty), foldoutLabel);
                Caches.SetFoldout(persistentProperty, foldout);
                if (includeChildren && foldout)
                {
                    var propertyHeight = propertyDrawer.GetPropertyHeight(persistentProperty, label);
                    var propertyRect = EditorGUILayout.GetControlRect(false, propertyHeight);
                    propertyDrawer.OnGUI(propertyRect, persistentProperty, label);
                }
            }
            else
            {
                bool rawShowMixedValue = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = persistentProperty.hasMultipleDifferentValues;
                object updateValue = null;
                object propertyValue = persistentProperty.GetValue<object>();
                if (propertyType == typeof(bool))
                {
                    updateValue = EditorGUILayout.Toggle(label, (bool)propertyValue, options);
                }
                else if (propertyType == typeof(int))
                {
                    updateValue = EditorGUILayout.IntField(label, (int)propertyValue, options);
                }
                else if (propertyType == typeof(long))
                {
                    updateValue = EditorGUILayout.LongField(label, (long)propertyValue, options);
                }
                else if (propertyType == typeof(float))
                {
                    updateValue = EditorGUILayout.FloatField(label, (float)propertyValue, options);
                }
                else if (propertyType == typeof(double))
                {
                    updateValue = EditorGUILayout.DoubleField(label, (double)propertyValue, options);
                }
                else if (propertyType == typeof(string))
                {
                    updateValue = EditorGUILayout.TextField(label, (string)propertyValue, options);
                }
                else if (propertyType == typeof(Vector2))
                {
                    updateValue = EditorGUILayout.Vector2Field(label, (Vector2)propertyValue, options);
                }
                else if (propertyType == typeof(Vector3))
                {
                    updateValue = EditorGUILayout.Vector3Field(label, (Vector3)propertyValue, options);
                }
                else if (propertyType == typeof(Vector4))
                {
                    updateValue = EditorGUILayout.Vector4Field(label, (Vector4)propertyValue, options);
                }
                else if (propertyType == typeof(Color))
                {
                    updateValue = EditorGUILayout.ColorField(label, (Color)propertyValue, options);
                }
                else if (propertyType == typeof(Rect))
                {
                    updateValue = EditorGUILayout.RectField(label, (Rect)propertyValue, options);
                }
                else if (propertyType.IsEnum)
                {
                    updateValue = EditorGUILayout.EnumPopup(label, (Enum)propertyValue, options);
                }
                else if (propertyType == typeof(UnityEngine.Object) || propertyType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    updateValue = EditorGUILayout.ObjectField(label, (UnityEngine.Object)propertyValue, propertyType, false, options);
                }
                else if (propertyType == typeof(AnimationCurve))
                {
                    EditorGUI.BeginChangeCheck();
                    updateValue = EditorGUILayout.CurveField(label, (AnimationCurve)propertyValue, options);
                    updateValue = new AnimationCurve(((AnimationCurve)updateValue).keys);
                }
                if (!Equals(propertyValue, updateValue))
                {
                    var objects = persistentProperty.persistentObject.GetValues<object>();
                    var commands = objects.Select(obj => new UpdateMemberCommand(obj, persistentProperty.propertyPath, updateValue)).ToArray();
                    Command.Execute(new CombinedCommand(commands));
                }
                EditorGUI.showMixedValue = rawShowMixedValue;
            }
        }

        private static CacheData Caches = new CacheData();

        private class CacheData
        {
            private Dictionary<Type, PropertyDrawer> mDrawers = new Dictionary<Type, PropertyDrawer>();

            private List<FoldData> mFoldouts = new List<FoldData>();

            public PropertyDrawer GetDrawer(Type propertyType)
            {
                PropertyDrawer drawer = null;
                var attributes = propertyType.GetCustomAttributes(typeof(CustomDrawerAttribute), true);
                var drawerType = attributes.Length > 0 ? (attributes[0] as CustomDrawerAttribute).type : null;
                if (drawerType != null)
                {
                    if (!mDrawers.ContainsKey(drawerType))
                    {
                        mDrawers.Add(drawerType, (PropertyDrawer)Activator.CreateInstance(drawerType));
                    }
                    drawer = mDrawers[drawerType];
                }
                return drawer;
            }

            public bool GetFoldout(PersistentProperty persistentProperty)
            {
                var ret = mFoldouts.Find(i => i.persistentObject == persistentProperty.persistentObject && i.propertyPath == persistentProperty.propertyPath);
                if (ret == null)
                {
                    ret = new FoldData()
                    {
                        persistentObject = persistentProperty.persistentObject,
                        propertyPath = persistentProperty.propertyPath
                    };
                    mFoldouts.Add(ret);
                }
                return ret.foldout;
            }

            public void SetFoldout(PersistentProperty persistentProperty, bool foldout)
            {
                var ret = mFoldouts.Find(i => i.persistentObject == persistentProperty.persistentObject && i.propertyPath == persistentProperty.propertyPath);
                if (ret != null)
                {
                    ret.foldout = foldout;
                }
            }

            private class FoldData
            {
                public PersistentObject persistentObject;

                public string propertyPath;

                public bool foldout = false;
            }
        }

        #endregion
    }
}
