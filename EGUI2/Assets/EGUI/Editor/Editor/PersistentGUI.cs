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
        private static int s_PropertyHash = "Property".GetHashCode();

        private static Stack<bool> s_EnabledStack = new Stack<bool>();

        private static Stack<bool> s_MixedValueStack = new Stack<bool>();

        private static Stack<Color> s_ColorStack = new Stack<Color>();

        private static Stack<bool> s_WideModeStack = new Stack<bool>();

        private static Stack<Color> h_ColorStack = new Stack<Color>();

        private static Stack<Matrix4x4> s_MatrixStack = new Stack<Matrix4x4>();

        private static Stack<float> s_LabelWidth = new Stack<float>();

        public static void DrawAAPolyLine(Rect rect, float width, Color color)
        {
            var w2 = width * 0.5f;
            BeginHandlesColor(color);
            Handles.DrawAAPolyLine(width,
                new Vector3(rect.x + w2, rect.y + w2, 0),
                new Vector3(rect.xMax - w2, rect.y + w2, 0),
                new Vector3(rect.xMax - w2, rect.yMax - w2, 0),
                new Vector3(rect.x + w2, rect.yMax - w2, 0),
                new Vector3(rect.x + w2, rect.y + w2, 0));
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
                    EditorStyles.foldout.Draw(
                        new Rect(position.x + 5, position.y + (position.height - height) * 0.5f, position.width,
                            height), label, controlId, value);
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

        public static void IntSlider(Rect position, GUIContent label, PersistentProperty property, int leftValue,
            int rightValue)
        {
            Debug.Assert(property.type == typeof(int));
            var value = property.GetValue<int>();
            BeginShowMixedValue(property.hasMultipleDifferentValues);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.IntSlider(position, label, value, leftValue, rightValue);
            if (EditorGUI.EndChangeCheck())
                property.SetValue(newValue);
            EndShowMixedValue();
        }
        
        public static void FloatSlider(Rect position, GUIContent label, PersistentProperty property, float leftValue,
            float rightValue)
        {
            Debug.Assert(property.type == typeof(float));
            var value = property.GetValue<float>();
            BeginShowMixedValue(property.hasMultipleDifferentValues);
            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.Slider(position, label, value, leftValue, rightValue);
            if (EditorGUI.EndChangeCheck())
                property.SetValue(newValue);
            EndShowMixedValue();
        }

        public static void PropertyField(Rect position, GUIContent label, PersistentProperty property,
            bool includeChildren = false)
        {
            var propertyType = property.type;
            if (UserDatabase.caches.IsDefaultPropertyType(propertyType))
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
                    var propertyDrawer = UserDatabase.caches.GetPropertyDrawer(propertyType);
                    if (propertyDrawer != null)
                    {
                        propertyDrawer.OnGUI(position, property, label);
                    }
                    else
                    {
                        var foldout = EditorGUI.Foldout(propertyRect, UserDatabase.caches.GetPropertyFoldout(property),
                            label);
                        UserDatabase.caches.SetPropertyFoldout(property, foldout);
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

        public static void MultiPropertyField2x2(Rect position, GUIContent label, GUIContent[] subLabels,
            PersistentProperty[] properties, float labelWidth = 13f, bool[] disabledMask = null)
        {
            Debug.Assert(subLabels.Length == 4 && properties.Length == subLabels.Length);
            var controlID = GUIUtility.GetControlID(s_PropertyHash, FocusType.Keyboard, position);
            position = MultiFieldPrefixLabel(position, controlID, label, 2);
            position.height = EditorGUIUtility.singleLineHeight;
            MultiPropertyField(position, subLabels.Take(2).ToArray(), properties.Take(2).ToArray(), labelWidth,
                disabledMask);
            position.y += EditorGUIUtility.singleLineHeight;
            MultiPropertyField(position, subLabels.Skip(2).Take(2).ToArray(), properties.Skip(2).Take(2).ToArray(),
                labelWidth, disabledMask);
        }

        public static void MultiPropertyField(Rect position, GUIContent label, GUIContent[] subLabels,
            PersistentProperty[] properties, float labelWidth = 13f, bool[] disabledMask = null)
        {
            var controlID = GUIUtility.GetControlID(s_PropertyHash, FocusType.Keyboard, position);
            position = MultiFieldPrefixLabel(position, controlID, label, properties.Length);
            position.height = EditorGUIUtility.singleLineHeight;
            MultiPropertyField(position, subLabels, properties, labelWidth, disabledMask);
        }

        public static void MultiPropertyField(Rect position, GUIContent[] subLabels, PersistentProperty[] properties,
            float labelWidth = 13f, bool[] disabledMask = null)
        {
            var num = subLabels.Length;
            var num2 = (position.width - (num - 1) * 2f) / num;
            var position2 = new Rect(position);
            position2.width = num2;
            var labelWidth2 = EditorGUIUtility.labelWidth;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = 0;
            for (var i = 0; i < subLabels.Length; i++)
            {
                if (disabledMask != null) BeginDisabled(disabledMask[i]);
                PropertyField(position2, subLabels[i], properties[i], false);
                if (disabledMask != null) EndDisabled();
                position2.x += num2 + 2f;
            }

            EditorGUIUtility.labelWidth = labelWidth2;
            EditorGUI.indentLevel = indentLevel;
        }

        public static void MultiLabelField(Rect position, GUIContent[] subLabels)
        {
            var num = subLabels.Length;
            var num2 = (position.width - (num - 1) * 2f) / num;
            var position2 = new Rect(position);
            position2.width = num2;
            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            foreach (var subLabel in subLabels)
            {
                EditorGUI.HandlePrefixLabel(position2, position2, subLabel);
                EditorGUI.LabelField(position2, subLabel);
                position2.x += num2 + 2f;
            }

            EditorGUI.indentLevel = indentLevel;
        }

        public static void Vector2Field(Rect position, GUIContent label, PersistentProperty property)
        {
            MultiPropertyField(position, label, new[] {new GUIContent("X"), new GUIContent("Y")},
                property.Find(new[] {"x", "y"}));
        }

        public static void Vector3Field(Rect position, GUIContent label, PersistentProperty property)
        {
            MultiPropertyField(position, label, new[] {new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z")},
                property.Find(new[] {"x", "y", "z"}));
        }

        public static void Vector4Field(Rect position, GUIContent label, PersistentProperty property)
        {
            MultiPropertyField(position, label,
                new[] {new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z"), new GUIContent("W")},
                property.Find(new[] {"x", "y", "z", "w"}));
        }

        public static void RectField(Rect position, GUIContent label, PersistentProperty property)
        {
            MultiPropertyField2x2(position, label,
                new[] {new GUIContent("X"), new GUIContent("Y"), new GUIContent("W"), new GUIContent("H")},
                property.Find(new[] {"x", "y", "width", "height"}));
        }

        public static void BoundsField(Rect position, GUIContent label, PersistentProperty property)
        {
            bool flag = LabelHasContent(label);
            if (flag)
            {
                int controlID = GUIUtility.GetControlID(s_PropertyHash, FocusType.Keyboard, position);
                position = MultiFieldPrefixLabel(position, controlID, label, 3);
                if (EditorGUIUtility.wideMode)
                {
                    position.y += EditorGUIUtility.singleLineHeight;
                }
            }

            position.height = EditorGUIUtility.singleLineHeight;
            position = DrawBoundsFieldLabelsAndAdjustPositionForValues(position, EditorGUIUtility.wideMode && flag);
            MultiPropertyField(position,
                new GUIContent[] {new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z")},
                property.Find(new string[] {"center.x", "center.y", "center.z"}));
            position.y += EditorGUIUtility.singleLineHeight;
            MultiPropertyField(position,
                new GUIContent[] {new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z")},
                property.Find(new string[] {"extents.x", "extents.y", "extents.z"}));
        }

        public static Rect DrawBoundsFieldLabelsAndAdjustPositionForValues(Rect position, bool drawOutside)
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

        public static Rect MultiFieldPrefixLabel(Rect totalPosition, int id, GUIContent label, int columns)
        {
            Rect result;
            if (!LabelHasContent(label))
            {
                result = EditorGUI.IndentedRect(totalPosition);
            }
            else if (EditorGUIUtility.wideMode)
            {
                Rect labelPosition = new Rect(totalPosition.x + indent, totalPosition.y,
                    EditorGUIUtility.labelWidth - indent, EditorGUIUtility.singleLineHeight);
                Rect rect = totalPosition;
                rect.xMin += EditorGUIUtility.labelWidth;
                if (columns > 1)
                {
                    labelPosition.width -= 1f;
                    rect.xMin -= 1f;
                }

                if (columns == 2)
                {
                    float num = 4f;
                    rect.xMax -= num + 2f;
                }

                EditorGUI.HandlePrefixLabel(totalPosition, labelPosition, label, id);
                result = rect;
            }
            else
            {
                Rect labelPosition2 = new Rect(totalPosition.x + indent, totalPosition.y, totalPosition.width - indent,
                    EditorGUIUtility.singleLineHeight);
                Rect rect2 = totalPosition;
                rect2.xMin += indent + 15f;
                rect2.yMin += EditorGUIUtility.singleLineHeight;
                EditorGUI.HandlePrefixLabel(totalPosition, labelPosition2, label, id);
                result = rect2;
            }

            return result;
        }

        public static object DefaultTypeField(Rect position, GUIContent label, object value)
        {
            object updateValue = null;
            var propertyType = value.GetType();
            if (propertyType == typeof(Vector2))
            {
                updateValue = EditorGUI.Vector2Field(position, label, (Vector2) value);
            }
            else if (propertyType == typeof(Vector3))
            {
                updateValue = EditorGUI.Vector3Field(position, label, (Vector3) value);
            }
            else if (propertyType == typeof(Vector4))
            {
                updateValue = EditorGUI.Vector4Field(position, label, (Vector4) value);
            }
            else if (propertyType == typeof(Rect))
            {
                updateValue = EditorGUI.RectField(position, label, (Rect) value);
            }
            else if (propertyType == typeof(Bounds))
            {
                updateValue = EditorGUI.BoundsField(position, label, (Bounds) value);
            }
            else if (propertyType == typeof(bool))
            {
                updateValue = EditorGUI.Toggle(position, label, (bool) value);
            }
            else if (propertyType == typeof(int))
            {
                updateValue = EditorGUI.IntField(position, label, (int) value);
            }
            else if (propertyType == typeof(long))
            {
                updateValue = EditorGUI.LongField(position, label, (long) value);
            }
            else if (propertyType == typeof(float))
            {
                updateValue = EditorGUI.FloatField(position, label, (float) value);
            }
            else if (propertyType == typeof(double))
            {
                updateValue = EditorGUI.DoubleField(position, label, (double) value);
            }
            else if (propertyType == typeof(string))
            {
                updateValue = EditorGUI.TextField(position, label, (string) value);
            }
            else if (propertyType == typeof(Color) ||
                     propertyType == typeof(Color32))
            {
                updateValue = EditorGUI.ColorField(position, label, (Color) value);
            }
            else if (propertyType == typeof(UnityEngine.Object) ||
                     propertyType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                updateValue = EditorGUI.ObjectField(position, label, (UnityEngine.Object) value, propertyType, false);
            }
            else if (propertyType == typeof(AnimationCurve))
            {
                updateValue = EditorGUI.CurveField(position, label, (AnimationCurve) value);
            }
            else if (propertyType.IsEnum)
            {
                updateValue = EditorGUI.EnumPopup(position, label, (Enum) value);
            }
            else
            {
                throw new NotSupportedException("Invalid default type: " + propertyType.FullName);
            }

            return updateValue;
        }

        public static void DefaultPropertyField(Rect position, GUIContent label, PersistentProperty property)
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
                    updateValue = EditorGUI.Toggle(position, label, (bool) propertyValue);
                }
                else if (propertyType == typeof(int))
                {
                    updateValue = EditorGUI.IntField(position, label, (int) propertyValue);
                }
                else if (propertyType == typeof(long))
                {
                    updateValue = EditorGUI.LongField(position, label, (long) propertyValue);
                }
                else if (propertyType == typeof(float))
                {
                    updateValue = EditorGUI.FloatField(position, label, (float) propertyValue);
                }
                else if (propertyType == typeof(double))
                {
                    updateValue = EditorGUI.DoubleField(position, label, (double) propertyValue);
                }
                else if (propertyType == typeof(string))
                {
                    updateValue = EditorGUI.TextField(position, label, (string) propertyValue);
                }
                else if (propertyType == typeof(Color) ||
                         propertyType == typeof(Color32))
                {
                    updateValue = EditorGUI.ColorField(position, label, (Color) propertyValue);
                }
                else if (propertyType == typeof(UnityEngine.Object) ||
                         propertyType.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    updateValue = EditorGUI.ObjectField(position, label, (UnityEngine.Object) propertyValue,
                        propertyType, false);
                }
                else if (propertyType == typeof(AnimationCurve))
                {
                    updateValue = EditorGUI.CurveField(position, label, (AnimationCurve) propertyValue);
                }
                else if (propertyType.IsEnum)
                {
                    updateValue = EditorGUI.EnumPopup(position, label, (Enum) propertyValue);
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

        public static float GetPropertyHeight(GUIContent label, PersistentProperty property, bool includeChildren)
        {
            var height = 0f;
            var propertyType = property.type;
            var propertyDrawer = UserDatabase.caches.GetPropertyDrawer(propertyType);
            if (propertyDrawer != null)
            {
                height += propertyDrawer.GetHeight(property, label);
            }
            else if (UserDatabase.caches.IsDefaultPropertyType(propertyType))
            {
                return GetDefaultTypeHeight(label, propertyType);
            }
            else
            {
                height += EditorGUIUtility.singleLineHeight;
                var foldout = UserDatabase.caches.GetPropertyFoldout(property);
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

        public static float GetDefaultTypeHeight(GUIContent label, Type propertyType)
        {
            var height = 0f;
            if (propertyType == typeof(Vector2) ||
                propertyType == typeof(Vector3) ||
                propertyType == typeof(Vector4))
            {
                height = ((LabelHasContent(label) && !EditorGUIUtility.wideMode)
                             ? EditorGUIUtility.singleLineHeight
                             : 0f) + EditorGUIUtility.singleLineHeight;
            }
            else if (propertyType == typeof(Rect))
            {
                height = ((LabelHasContent(label) && !EditorGUIUtility.wideMode)
                             ? EditorGUIUtility.singleLineHeight
                             : 0f) + EditorGUIUtility.singleLineHeight * 2;
            }
            else if (propertyType == typeof(Bounds))
            {
                height = (LabelHasContent(label) ? EditorGUIUtility.singleLineHeight : 0f) +
                         EditorGUIUtility.singleLineHeight * 2;
            }
            else
            {
                height = EditorGUIUtility.singleLineHeight;
            }

            return height;
        }

        public static void BeginMatrix(Matrix4x4 matrix)
        {
            s_MatrixStack.Push(GUI.matrix);
            GUI.matrix = matrix;
        }

        public static void EndMatrix()
        {
            if (s_MatrixStack.Count > 0)
            {
                GUI.matrix = s_MatrixStack.Pop();
            }
        }

        public static void BeginColor(Color color)
        {
            s_ColorStack.Push(GUI.color);
            GUI.color = color;
        }

        public static void EndColor()
        {
            if (s_ColorStack.Count > 0)
            {
                GUI.color = s_ColorStack.Pop();
            }
        }

        public static void BeginDisabled(bool disabled)
        {
            s_EnabledStack.Push(GUI.enabled);
            GUI.enabled &= !disabled;
        }

        public static void EndDisabled()
        {
            if (s_EnabledStack.Count > 0)
            {
                GUI.enabled = s_EnabledStack.Pop();
            }
        }

        public static void BeginWideMode(bool wideMode)
        {
            s_WideModeStack.Push(EditorGUIUtility.wideMode);
            EditorGUIUtility.wideMode = wideMode;
        }

        public static void EndWideMode()
        {
            if (s_WideModeStack.Count > 0)
            {
                EditorGUIUtility.wideMode = s_WideModeStack.Pop();
            }
        }

        public static void BeginShowMixedValue(bool showMixedValue)
        {
            s_MixedValueStack.Push(EditorGUI.showMixedValue);
            EditorGUI.showMixedValue = showMixedValue;
        }

        public static void EndShowMixedValue()
        {
            EditorGUI.showMixedValue = s_MixedValueStack.Pop();
        }

        public static void BeginHandlesColor(Color color)
        {
            h_ColorStack.Push(Handles.color);
            Handles.color = color;
        }

        public static void EndHandlesColor()
        {
            if (h_ColorStack.Count > 0)
            {
                Handles.color = h_ColorStack.Pop();
            }
        }

        public static void BeginLabelWidth(float labelWidth)
        {
            s_LabelWidth.Push(EditorGUIUtility.labelWidth);
            EditorGUIUtility.labelWidth = labelWidth;
        }

        public static void EndLabelWidth()
        {
            EditorGUIUtility.labelWidth = s_LabelWidth.Pop();
        }

        public static bool LabelHasContent(GUIContent label)
        {
            return label == null || label.text != string.Empty || label.image != null;
        }

        public static float indent
        {
            get { return EditorGUI.indentLevel * 15f; }
        }

        public static bool IsDefaultPropertyType(Type propertyType)
        {
            return UserDatabase.caches.IsDefaultPropertyType(propertyType);
        }
    }
}