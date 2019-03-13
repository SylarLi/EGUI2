using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EGUI.Editor
{
    public sealed class PersistentGUI
    {
        internal static int s_FoldoutHash = "Foldout".GetHashCode();

        private static Stack<bool> s_EnabledStack = new Stack<bool>();

        private static Stack<bool> s_MixedValueStack = new Stack<bool>();

        public static void LabelField(Rect position, GUIContent label, GUIContent label2, GUIStyle style)
        {
            EditorGUI.LabelField(position, label, label2, style);
        }

        public static bool Toggle(Rect position, GUIContent label, bool value, GUIStyle style)
        {
            return EditorGUI.Toggle(position, label, value, style);
        }

        public static int IntField(Rect position, GUIContent label, int value, GUIStyle style)
        {
            return EditorGUI.IntField(position, label, value, style);
        }

        public static long LongField(Rect position, GUIContent label, long value, GUIStyle style)
        {
            return EditorGUI.LongField(position, label, value, style);
        }

        public static float FloatField(Rect position, GUIContent label, float value, GUIStyle style)
        {
            return EditorGUI.FloatField(position, label, value, style);
        }

        public static double DoubleField(Rect position, GUIContent label, double value, GUIStyle style)
        {
            return EditorGUI.DoubleField(position, label, value, style);
        }

        public static string TextField(Rect position, GUIContent label, string value, GUIStyle style)
        {
            return EditorGUI.TextField(position, label, value, style);
        }

        public static Enum EnumPopup(Rect position, GUIContent label, Enum value, GUIStyle style)
        {
            return EditorGUI.EnumPopup(position, label, value, style);
        }

        public static UnityEngine.Object ObjectField(Rect position, GUIContent label, UnityEngine.Object value, Type type, bool allowSceneObjects)
        {
            return EditorGUI.ObjectField(position, label, value, type, allowSceneObjects);
        }

        public static Color ColorField(Rect position, GUIContent label, Color value)
        {
            return EditorGUI.ColorField(position, label, value);
        }

        public static AnimationCurve CurveField(Rect position, GUIContent label, AnimationCurve value)
        {
            return EditorGUI.CurveField(position, label, value);
        }

        public static bool Foldout(Rect position, GUIContent label, bool foldout)
        {
            return EditorGUI.Foldout(position, foldout, label, true);
        }

        public static Vector2 Vector2Field(Rect position, GUIContent label, Vector2 value)
        {
            return EditorGUI.Vector2Field(position, label, value);
        }

        public static Vector3 Vector3Field(Rect position, GUIContent label, Vector3 value)
        {
            return EditorGUI.Vector3Field(position, label, value);
        }

        public static Vector4 Vector4Field(Rect position, GUIContent label, Vector4 value)
        {
            return EditorGUI.Vector4Field(position, label, value);
        }

        public static Rect RectField(Rect position, GUIContent label, Rect value)
        {
            return EditorGUI.RectField(position, label, value);
        }

        public static Bounds BoundsField(Rect position, GUIContent label, Bounds value)
        {
            return EditorGUI.BoundsField(position, label, value);
        }

        // 注：暂不支持默认递归属性显示
        public static void PropertyField(Rect position, GUIContent label, PersistentProperty property, bool includeChildren)
        {
            var propertyType = property.type;
            var propertyDrawer = Caches.GetDrawer(propertyType);
            if (propertyDrawer != null)
            {
                var foldoutLabel = new GUIContent(property.displayName);
                var foldout = Foldout(position, foldoutLabel, Caches.GetFoldout(property));
                Caches.SetFoldout(property, foldout);
                if (includeChildren && foldout)
                {
                    var propertyRect = position;
                    propertyRect.y += EditorGUIUtility.singleLineHeight;
                    propertyRect.height = propertyDrawer.GetPropertyHeight(property, label);
                    propertyDrawer.OnGUI(propertyRect, property, label);
                }
            }
            else
            {
                DefaultPropertyField(position, label, property);
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
            MultiPropertyField(position, new GUIContent[] { new GUIContent("Z"), new GUIContent("W") }, property.Find(new string[] { "z", "w" }));
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
                    updateValue = Toggle(position, label, (bool)propertyValue, EditorStyles.toggle);
                }
                else if (propertyType == typeof(int))
                {
                    updateValue = IntField(position, label, (int)propertyValue, EditorStyles.numberField);
                }
                else if (propertyType == typeof(long))
                {
                    updateValue = LongField(position, label, (long)propertyValue, EditorStyles.numberField);
                }
                else if (propertyType == typeof(float))
                {
                    updateValue = FloatField(position, label, (float)propertyValue, EditorStyles.numberField);
                }
                else if (propertyType == typeof(double))
                {
                    updateValue = DoubleField(position, label, (double)propertyValue, EditorStyles.numberField);
                }
                else if (propertyType == typeof(string))
                {
                    updateValue = TextField(position, label, (string)propertyValue, EditorStyles.textField);
                }
                else if (propertyType == typeof(Color))
                {
                    updateValue = ColorField(position, label, (Color)propertyValue);
                }
                else if (propertyType == typeof(UnityEngine.Object) || propertyType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    updateValue = ObjectField(position, label, (UnityEngine.Object)propertyValue, propertyType, false);
                }
                else if (propertyType == typeof(AnimationCurve))
                {
                    updateValue = CurveField(position, label, (AnimationCurve)propertyValue);
                }
                else if (propertyType.IsEnum)
                {
                    updateValue = EnumPopup(position, label, (Enum)propertyValue, EditorStyles.popup);
                }
                else
                {
                    LabelField(position, label, GUIContent.none, EditorStyles.label);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    var objects = property.persistentObject.GetValues<object>();
                    var commands = objects.Select(obj => new UpdateMemberCommand(obj, property.propertyPath, updateValue)).ToArray();
                    Command.Execute(new CombinedCommand(commands));
                }
                EndShowMixedValue();
            }
        }

        internal static float GetPropertyHeight(GUIContent label, PersistentProperty property, bool includeChildren)
        {
            var height = 0f;
            var propertyType = property.type;
            var propertyDrawer = Caches.GetDrawer(propertyType);
            if (propertyDrawer != null)
            {
                height += EditorGUIUtility.singleLineHeight;
                if (includeChildren && Caches.GetFoldout(property))
                {
                    height += propertyDrawer.GetPropertyHeight(property, label);
                }
            }
            else if (propertyType == typeof(Vector2) ||
                propertyType == typeof(Vector3) ||
                propertyType == typeof(Vector4))
            {
                height += ((LabelHasContent(label) && !EditorGUIUtility.wideMode) ? EditorGUIUtility.singleLineHeight : 0f) + EditorGUIUtility.singleLineHeight;
            }
            else if (propertyType == typeof(Rect))
            {
                height += ((LabelHasContent(label) && !EditorGUIUtility.wideMode) ? EditorGUIUtility.singleLineHeight : 0f) + EditorGUIUtility.singleLineHeight * 2;
            }
            else if (propertyType == typeof(Bounds))
            {
                height += (LabelHasContent(label) ? EditorGUIUtility.singleLineHeight : 0f) + EditorGUIUtility.singleLineHeight * 2;
            }
            else
            {
                height += EditorGUIUtility.singleLineHeight;
            }
            return height;
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

        internal static void BeginShowMixedValue(bool showMixedValue)
        {
            s_MixedValueStack.Push(EditorGUI.showMixedValue);
            EditorGUI.showMixedValue = showMixedValue;
        }

        internal static void EndShowMixedValue()
        {
            EditorGUI.showMixedValue = s_MixedValueStack.Pop();
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

        internal static CacheData Caches = new CacheData();

        internal class CacheData
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
    }
}
