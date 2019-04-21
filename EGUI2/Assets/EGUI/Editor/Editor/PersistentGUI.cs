using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace EGUI.Editor
{
    public sealed class PersistentGUI
    {
        internal static int s_FoldoutHash = "Foldout".GetHashCode();

        private static Stack<bool> s_EnabledStack = new Stack<bool>();

        private static Stack<bool> s_MixedValueStack = new Stack<bool>();

        private static Stack<Color> s_ColorStack = new Stack<Color>();

        private static Stack<bool> s_WideModeStack = new Stack<bool>();

        private static Stack<Color> h_ColorStack = new Stack<Color>();

        private static Stack<Matrix4x4> s_MatrixStack = new Stack<Matrix4x4>();

        public static void DrawAAPolyLine(Rect rect, float width, Color color)
        {
            BeginHandlesColor(color);
            Handles.DrawAAPolyLine(width,
                new Vector3(rect.x, rect.y, 0),
                new Vector3(rect.xMax, rect.y, 0),
                new Vector3(rect.xMax, rect.yMax, 0),
                new Vector3(rect.x, rect.yMax, 0),
                new Vector3(rect.x, rect.y, 0));
            EndHandlesColor();
        }

        public static bool ToggleBar(Rect position, GUIContent label, bool value)
        {
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = Event.current.GetTypeForControl(controlId);
            switch (eventType)
            {
                case EventType.Repaint:
                    {
                        EditorStyles.toolbarButton.Draw(position, GUIContent.none, controlId, value);
                        var height = EditorStyles.foldout.CalcHeight(label, position.width);
                        EditorStyles.foldout.Draw(new Rect(position.x + 5, position.y + (position.height - height) * 0.5f, position.width, height), label, controlId, value);
                        break;
                    }
                case EventType.MouseDown:
                    {
                        if (position.Contains(Event.current.mousePosition))
                        {
                            value = !value;
                            Event.current.Use();
                        }
                        break;
                    }
            }
            return value;
        }

        public static void PropertyField(Rect position, GUIContent label, PersistentProperty property)
        {
            PropertyField(position, label, property, false);
        }

        public static void PropertyField(Rect position, GUIContent label, PersistentProperty property, bool includeChildren)
        {
            var propertyType = property.type;
            if (Caches.IsDefaultPropertyType(propertyType))
            {
                DefaultPropertyField(position, label, property);
            }
            else
            {
                var propertyRect = position;
                propertyRect.height = EditorGUIUtility.singleLineHeight;
                if (!property.exist)
                {
                    BeginColor(Color.yellow);
                    EditorGUI.LabelField(propertyRect, label, new GUIContent("NULL"));
                    EndColor();
                }
                else
                {
                    var propertyDrawer = Caches.GetPropertyDrawer(propertyType);
                    if (propertyDrawer != null)
                    {
                        propertyDrawer.OnGUI(position, property, label);
                    }
                    else
                    {
                        var foldout = EditorGUI.Foldout(propertyRect, Caches.GetFoldout(property), label);
                        Caches.SetFoldout(property, foldout);
                        if (includeChildren && foldout)
                        {
                            propertyRect.y += EditorGUIUtility.singleLineHeight;
                            var children = property.ListChildren();
                            EditorGUI.indentLevel += 1;
                            foreach (var child in children)
                            {
                                var childLabel = new GUIContent(child.displayName);
                                propertyRect.y += EditorGUIUtility.standardVerticalSpacing;
                                PropertyField(propertyRect, childLabel, child, includeChildren);
                                propertyRect.y += GetPropertyHeight(childLabel, child, includeChildren);
                            }
                            EditorGUI.indentLevel -= 1;
                        }
                    }
                }
                
            }
        }

        public static void MultiPropertyField(Rect position, GUIContent[] subLabels, PersistentProperty[] properties, float labelWidth = 13f, bool[] disabledMask = null)
        {
            float num = subLabels.Length;
            float num2 = (position.width - (num - 1) * 2f) / num;
            Rect position2 = new Rect(position);
            position2.width = num2;
            float labelWidth2 = EditorGUIUtility.labelWidth;
            int indentLevel = EditorGUI.indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = 0;
            for (int i = 0; i < subLabels.Length; i++)
            {
                if (disabledMask != null) BeginDisabled(disabledMask[i]);
                PropertyField(position2, subLabels[i], properties[i], false);
                if (disabledMask != null) EndDisabled();
                position2.x += num2 + 2f;
            }
            EditorGUIUtility.labelWidth = labelWidth2;
            EditorGUI.indentLevel = indentLevel;
        }

        internal static void Vector2Field(Rect position, GUIContent label, PersistentProperty property)
        {
            int controlID = GUIUtility.GetControlID(s_FoldoutHash, FocusType.Keyboard, position);
            position = MultiFieldPrefixLabel(position, controlID, label, 2);
            position.height = EditorGUIUtility.singleLineHeight;
            MultiPropertyField(position, new GUIContent[] { new GUIContent("X"), new GUIContent("Y") }, property.Find(new string[] { "x", "y" }));
        }

        internal static void Vector3Field(Rect position, GUIContent label, PersistentProperty property)
        {
            int controlID = GUIUtility.GetControlID(s_FoldoutHash, FocusType.Keyboard, position);
            position = MultiFieldPrefixLabel(position, controlID, label, 3);
            position.height = EditorGUIUtility.singleLineHeight;
            MultiPropertyField(position, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") }, property.Find(new string[] { "x", "y", "z" }));
        }

        internal static void Vector4Field(Rect position, GUIContent label, PersistentProperty property)
        {
            int controlID = GUIUtility.GetControlID(s_FoldoutHash, FocusType.Keyboard, position);
            position = MultiFieldPrefixLabel(position, controlID, label, 4);
            position.height = EditorGUIUtility.singleLineHeight;
            MultiPropertyField(position, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z"), new GUIContent("W") }, property.Find(new string[] { "x", "y", "z", "w" }));
        }

        internal static void RectField(Rect position, GUIContent label, PersistentProperty property)
        {
            int controlID = GUIUtility.GetControlID(s_FoldoutHash, FocusType.Keyboard, position);
            position = MultiFieldPrefixLabel(position, controlID, label, 2);
            position.height = EditorGUIUtility.singleLineHeight;
            MultiPropertyField(position, new GUIContent[] { new GUIContent("X"), new GUIContent("Y") }, property.Find(new string[] { "x", "y" }));
            position.y += EditorGUIUtility.singleLineHeight;
            MultiPropertyField(position, new GUIContent[] { new GUIContent("W"), new GUIContent("H") }, property.Find(new string[] { "width", "height" }));
        }

        internal static void BoundsField(Rect position, GUIContent label, PersistentProperty property)
        {
            bool flag = LabelHasContent(label);
            if (flag)
            {
                int controlID = GUIUtility.GetControlID(s_FoldoutHash, FocusType.Keyboard, position);
                position = MultiFieldPrefixLabel(position, controlID, label, 3);
                if (EditorGUIUtility.wideMode)
                {
                    position.y += EditorGUIUtility.singleLineHeight;
                }
            }
            position.height = EditorGUIUtility.singleLineHeight;
            position = DrawBoundsFieldLabelsAndAdjustPositionForValues(position, EditorGUIUtility.wideMode && flag);
            MultiPropertyField(position, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") }, property.Find(new string[] { "center.x", "center.y", "center.z" }));
            position.y += EditorGUIUtility.singleLineHeight;
            MultiPropertyField(position, new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") }, property.Find(new string[] { "extents.x", "extents.y", "extents.z" }));
        }

        internal static Rect DrawBoundsFieldLabelsAndAdjustPositionForValues(Rect position, bool drawOutside)
        {
            if (drawOutside)
            {
                position.xMin -= 53f;
            }
            GUI.Label(position, "Center:", EditorStyles.label);
            position.y += EditorGUIUtility.singleLineHeight;
            GUI.Label(position, "Extents:", EditorStyles.label);
            position.y -= EditorGUIUtility.singleLineHeight;
            position.xMin += 53f;
            return position;
        }

        internal static Rect MultiFieldPrefixLabel(Rect totalPosition, int id, GUIContent label, int columns)
        {
            Rect result;
            if (!LabelHasContent(label))
            {
                result = EditorGUI.IndentedRect(totalPosition);
            }
            else if (EditorGUIUtility.wideMode)
            {
                Rect labelPosition = new Rect(totalPosition.x + indent, totalPosition.y, EditorGUIUtility.labelWidth - indent, EditorGUIUtility.singleLineHeight);
                Rect rect = totalPosition;
                rect.xMin += EditorGUIUtility.labelWidth;
                if (columns > 1)
                {
                    labelPosition.width -= 1f;
                    rect.xMin -= 1f;
                }
                if (columns == 2)
                {
                    float num = (rect.width - 4f) / 3f;
                    rect.xMax -= num + 2f;
                }
                EditorGUI.HandlePrefixLabel(totalPosition, labelPosition, label, id);
                result = rect;
            }
            else
            {
                Rect labelPosition2 = new Rect(totalPosition.x + indent, totalPosition.y, totalPosition.width - indent, EditorGUIUtility.singleLineHeight);
                Rect rect2 = totalPosition;
                rect2.xMin += indent + 15f;
                rect2.yMin += EditorGUIUtility.singleLineHeight;
                EditorGUI.HandlePrefixLabel(totalPosition, labelPosition2, label, id);
                result = rect2;
            }
            return result;
        }

        internal static object DefaultTypeField(Rect position, GUIContent label, object value)
        {
            object updateValue = null;
            var propertyType = value.GetType();
            if (propertyType == typeof(Vector2))
            {
                updateValue = EditorGUI.Vector2Field(position, label, (Vector2)value);
            }
            else if (propertyType == typeof(Vector3))
            {
                updateValue = EditorGUI.Vector3Field(position, label, (Vector3)value);
            }
            else if (propertyType == typeof(Vector4))
            {
                updateValue = EditorGUI.Vector4Field(position, label, (Vector4)value);
            }
            else if (propertyType == typeof(Rect))
            {
                updateValue = EditorGUI.RectField(position, label, (Rect)value);
            }
            else if (propertyType == typeof(Bounds))
            {
                updateValue = EditorGUI.BoundsField(position, label, (Bounds)value);
            }
            else if(propertyType == typeof(bool))
            {
                updateValue = EditorGUI.Toggle(position, label, (bool)value);
            }
            else if (propertyType == typeof(int))
            {
                updateValue = EditorGUI.IntField(position, label, (int)value);
            }
            else if (propertyType == typeof(long))
            {
                updateValue = EditorGUI.LongField(position, label, (long)value);
            }
            else if (propertyType == typeof(float))
            {
                updateValue = EditorGUI.FloatField(position, label, (float)value);
            }
            else if (propertyType == typeof(double))
            {
                updateValue = EditorGUI.DoubleField(position, label, (double)value);
            }
            else if (propertyType == typeof(string))
            {
                updateValue = EditorGUI.TextField(position, label, (string)value);
            }
            else if (propertyType == typeof(Color) ||
                propertyType == typeof(Color32))
            {
                updateValue = EditorGUI.ColorField(position, label, (Color)value);
            }
            else if (propertyType == typeof(UnityEngine.Object) || propertyType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                updateValue = EditorGUI.ObjectField(position, label, (UnityEngine.Object)value, propertyType, false);
            }
            else if (propertyType == typeof(AnimationCurve))
            {
                updateValue = EditorGUI.CurveField(position, label, (AnimationCurve)value);
            }
            else if (propertyType.IsEnum)
            {
                updateValue = EditorGUI.EnumPopup(position, label, (Enum)value);
            }
            else
            {
                throw new NotSupportedException("Invalide default type: " + propertyType.FullName);
            }
            return updateValue;
        }

        internal static void DefaultPropertyField(Rect position, GUIContent label, PersistentProperty property)
        {
            var propertyType = property.type;
            if (propertyType == typeof(Vector2))
            {
                Vector2Field(position, label, property);
            }
            else if (propertyType == typeof(Vector3))
            {
                Vector3Field(position, label, property);
            }
            else if (propertyType == typeof(Vector4))
            {
                Vector4Field(position, label, property);
            }
            else if (propertyType == typeof(Rect))
            {
                RectField(position, label, property);
            }
            else if (propertyType == typeof(Bounds))
            {
                BoundsField(position, label, property);
            }
            else
            {
                object updateValue = null;
                object propertyValue = property.GetValue<object>();
                BeginShowMixedValue(property.hasMultipleDifferentValues);
                EditorGUI.BeginChangeCheck();
                if (propertyType == typeof(bool))
                {
                    updateValue = EditorGUI.Toggle(position, label, (bool)propertyValue);
                }
                else if (propertyType == typeof(int))
                {
                    updateValue = EditorGUI.IntField(position, label, (int)propertyValue);
                }
                else if (propertyType == typeof(long))
                {
                    updateValue = EditorGUI.LongField(position, label, (long)propertyValue);
                }
                else if (propertyType == typeof(float))
                {
                    updateValue = EditorGUI.FloatField(position, label, (float)propertyValue);
                }
                else if (propertyType == typeof(double))
                {
                    updateValue = EditorGUI.DoubleField(position, label, (double)propertyValue);
                }
                else if (propertyType == typeof(string))
                {
                    updateValue = EditorGUI.TextField(position, label, (string)propertyValue);
                }
                else if (propertyType == typeof(Color) ||
                    propertyType == typeof(Color32))
                {
                    updateValue = EditorGUI.ColorField(position, label, (Color)propertyValue);
                }
                else if (propertyType == typeof(UnityEngine.Object) || propertyType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    updateValue = EditorGUI.ObjectField(position, label, (UnityEngine.Object)propertyValue, propertyType, false);
                }
                else if (propertyType == typeof(AnimationCurve))
                {
                    updateValue = EditorGUI.CurveField(position, label, (AnimationCurve)propertyValue);
                }
                else if (propertyType.IsEnum)
                {
                    updateValue = EditorGUI.EnumPopup(position, label, (Enum)propertyValue);
                }
                else
                {
                    throw new NotSupportedException("Invalide default type: " + propertyType.FullName);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    property.SetValue(updateValue);
                }
                EndShowMixedValue();
            }
        }

        internal static float GetPropertyHeight(GUIContent label, PersistentProperty property, bool includeChildren)
        {
            var height = 0f;
            var propertyType = property.type;
            var propertyDrawer = Caches.GetPropertyDrawer(propertyType);
            if (propertyDrawer != null)
            {
                height += propertyDrawer.GetHeight(property, label);
            }
            else if (Caches.IsDefaultPropertyType(propertyType))
            {
                return GetDefaultTypeHeight(label, propertyType);
            }
            else
            {
                height += EditorGUIUtility.singleLineHeight;
                var foldout = Caches.GetFoldout(property);
                if (property.exist && includeChildren && foldout)
                {
                    var children = property.ListChildren();
                    foreach (var child in children)
                    {
                        var childLabel = new GUIContent(child.displayName);
                        height += EditorGUIUtility.standardVerticalSpacing;
                        height += GetPropertyHeight(childLabel, child, includeChildren);
                    }
                }
            }
            return height;
        }

        internal static float GetDefaultTypeHeight(GUIContent label, Type propertyType)
        {
            var height = 0f;
            if (propertyType == typeof(Vector2) ||
                propertyType == typeof(Vector3) ||
                propertyType == typeof(Vector4))
            {
                height = ((LabelHasContent(label) && !EditorGUIUtility.wideMode) ? EditorGUIUtility.singleLineHeight : 0f) + EditorGUIUtility.singleLineHeight;
            }
            else if (propertyType == typeof(Rect))
            {
                height = ((LabelHasContent(label) && !EditorGUIUtility.wideMode) ? EditorGUIUtility.singleLineHeight : 0f) + EditorGUIUtility.singleLineHeight * 2;
            }
            else if (propertyType == typeof(Bounds))
            {
                height = (LabelHasContent(label) ? EditorGUIUtility.singleLineHeight : 0f) + EditorGUIUtility.singleLineHeight * 2;
            }
            else
            {
                height = EditorGUIUtility.singleLineHeight;
            }
            return height;
        }

        internal static void BeginMatrix(Matrix4x4 matrix)
        {
            s_MatrixStack.Push(GUI.matrix);
            GUI.matrix = matrix;
        }

        internal static void EndMatrix()
        {
            if (s_MatrixStack.Count > 0)
            {
                GUI.matrix = s_MatrixStack.Pop();
            }
        }

        internal static void BeginColor(Color color)
        {
            s_ColorStack.Push(GUI.color);
            GUI.color = color;
        }

        internal static void EndColor()
        {
            if (s_ColorStack.Count > 0)
            {
                GUI.color = s_ColorStack.Pop();
            }
        }

        internal static void BeginDisabled(bool disabled)
        {
            s_EnabledStack.Push(GUI.enabled);
            GUI.enabled &= !disabled;
        }

        internal static void EndDisabled()
        {
            if (s_EnabledStack.Count > 0)
            {
                GUI.enabled = s_EnabledStack.Pop();
            }
        }

        internal static void BeginWideMode(bool wideMode)
        {
            s_WideModeStack.Push(EditorGUIUtility.wideMode);
            EditorGUIUtility.wideMode = wideMode;
        }

        internal static void EndWideMode()
        {
            if (s_WideModeStack.Count > 0)
            {
                EditorGUIUtility.wideMode = s_WideModeStack.Pop();
            }
        }

        internal static void BeginShowMixedValue(bool showMixedValue)
        {
            s_MixedValueStack.Push(EditorGUI.showMixedValue);
            EditorGUI.showMixedValue = showMixedValue;
        }

        internal static void EndShowMixedValue()
        {
            EditorGUI.showMixedValue = s_MixedValueStack.Pop();
        }

        internal static void BeginHandlesColor(Color color)
        {
            h_ColorStack.Push(Handles.color);
            Handles.color = color;
        }

        internal static void EndHandlesColor()
        {
            if (h_ColorStack.Count > 0)
            {
                Handles.color = h_ColorStack.Pop();
            }
        }

        internal static bool LabelHasContent(GUIContent label)
        {
            return label == null || label.text != string.Empty || label.image != null;
        }

        internal static float indent
        {
            get
            {
                return EditorGUI.indentLevel * 15f;
            }
        }

        internal static bool IsDefaultPropertyType(Type propertyType)
        {
            return Caches.IsDefaultPropertyType(propertyType);
        }

        internal static CacheData Caches = new CacheData();

        internal class CacheData
        {
            private HashSet<Type> mDefaultPropertyType = new HashSet<Type>();

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
                    mDefaultPropertyType.Add(typeof(bool));
                    mDefaultPropertyType.Add(typeof(int));
                    mDefaultPropertyType.Add(typeof(long));
                    mDefaultPropertyType.Add(typeof(float));
                    mDefaultPropertyType.Add(typeof(double));
                    mDefaultPropertyType.Add(typeof(string));
                }
                return mDefaultPropertyType.Contains(propertyType) ||
                    propertyType.IsEnum ||
                    propertyType.IsSubclassOf(typeof(UnityEngine.Object));
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
                        var drawerType = attributes.Length > 0 ? (attributes[0] as UserPropertyDrawerAttribute).type : null;
                        if (drawerType != null)
                        {
                            var instance = (UserPropertyDrawer)CoreUtil.CreateInstance(type, null);
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
                        var drawerType = attributes.Length > 0 ? (attributes[0] as UserDrawerAttribute).type : null;
                        if (drawerType != null)
                        {
                            var instance = (UserDrawer)CoreUtil.CreateInstance(type, null);
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

            public bool GetFoldout(PersistentProperty persistentProperty)
            {
                var ret = mFoldouts.Find(i => i.objectType == persistentProperty.persistentObject.type && i.propertyPath == persistentProperty.propertyPath);
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

            public void SetFoldout(PersistentProperty persistentProperty, bool foldout)
            {
                var ret = mFoldouts.Find(i => i.objectType == persistentProperty.persistentObject.type && i.propertyPath == persistentProperty.propertyPath);
                if (ret != null)
                {
                    ret.foldout = foldout;
                }
            }

            public bool GetHierarchyFoldout(Node node)
            {
                return mHierarchyFoldouts.ContainsKey(node) ? mHierarchyFoldouts[node] : false;
            }

            public void SetHierarchyFoldout(Node node, bool foldout)
            {
                mHierarchyFoldouts[node] = foldout;
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
