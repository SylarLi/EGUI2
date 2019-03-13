using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EGUI.Editor
{
    public sealed class PersistentGUILayout
    {
        public static void PropertyField(PersistentProperty property, params GUILayoutOption[] options)
        {
            PropertyField(new GUIContent(property.displayName), property, true, options);
        }

        public static void PropertyField(GUIContent label, PersistentProperty property, params GUILayoutOption[] options)
        {
            PropertyField(label, property, true, options);
        }

        public static void PropertyField(GUIContent label, PersistentProperty property, bool includeChildren, params GUILayoutOption[] options)
        {
            Rect rect;
            var propertyType = property.type;
            var propertyDrawer = PersistentGUI.Caches.GetDrawer(propertyType);
            if (propertyType == typeof(bool) && propertyDrawer == null)
            {
                rect = GetToggleRect(true, options);
            }
            else
            {
                rect = EditorGUILayout.GetControlRect(PersistentGUI.LabelHasContent(label), PersistentGUI.GetPropertyHeight(label, property, includeChildren), options);
            }
            PersistentGUI.PropertyField(rect, label, property, includeChildren);
        }

        internal static Rect GetToggleRect(bool hasLabel, params GUILayoutOption[] options)
        {
            float num = 10f - EditorGUIUtility.fieldWidth;
            return GUILayoutUtility.GetRect((!hasLabel) ? (EditorGUIUtility.fieldWidth + num) : (kLabelFloatMinW + num), kLabelFloatMaxW + num, 16f, 16f, EditorStyles.numberField, options);
        }

        internal static float kLabelFloatMinW
        {
            get
            {
                return EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + 5f;
            }
        }

        internal static float kLabelFloatMaxW
        {
            get
            {
                return EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth + 5f;
            }
        }
    }
}
