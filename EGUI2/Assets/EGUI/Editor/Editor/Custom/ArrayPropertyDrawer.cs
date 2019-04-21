using System;
using UnityEngine;
using UnityEditor;

namespace EGUI.Editor
{
    [UserPropertyDrawer(typeof(Array))]
    internal class ArrayPropertyDrawer : UserPropertyDrawer
    {
        public override void OnGUI(Rect position, PersistentProperty property, GUIContent label)
        {
            var rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;
            var propLength = property.Find("Length");
            PersistentGUI.BeginShowMixedValue(propLength.hasMultipleDifferentValues);
            EditorGUI.BeginChangeCheck();
            var length = EditorGUI.DelayedIntField(rect, label, propLength.GetValue<int>());
            if (EditorGUI.EndChangeCheck())
            {
                length = Mathf.Max(0, length);
                property.ResizeArray(length);
            }
            PersistentGUI.EndShowMixedValue();
            rect.y += EditorGUIUtility.singleLineHeight;
            rect.y += EditorGUIUtility.standardVerticalSpacing;
            var elType = property.type.GetElementType();
            if (property.length == 1 && 
                PersistentGUI.IsDefaultPropertyType(elType))
            {
                EditorGUI.indentLevel += 1;
                var array = property.GetValue<Array>();
                for (int i = 0; i < array.Length; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    var itemLabel = new GUIContent("Element " + i);
                    var itemValue = PersistentGUI.DefaultTypeField(rect, itemLabel, array.GetValue(i));
                    if (EditorGUI.EndChangeCheck())
                    {
                        var newArray = Array.CreateInstance(elType, array.Length);
                        Array.Copy(array, newArray, array.Length);
                        newArray.SetValue(itemValue, i);
                        property.SetValue(newArray);
                    }
                    rect.y += PersistentGUI.GetDefaultTypeHeight(itemLabel, elType);
                    rect.y += EditorGUIUtility.standardVerticalSpacing;
                }
                EditorGUI.indentLevel -= 1;
            }
            else
            {
                PersistentGUI.BeginColor(Color.yellow);
                EditorGUI.LabelField(rect, new GUIContent("Array's element is not editable."));
                PersistentGUI.EndColor();
            }
        }

        public override float GetHeight(PersistentProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;
            var elType = property.type.GetElementType();
            if (property.length == 1 &&
                PersistentGUI.IsDefaultPropertyType(elType))
            {
                var array = property.GetValue<Array>();
                for (int i = 0; i < array.Length; i++)
                {
                    var itemLabel = new GUIContent("Element " + i);
                    height += EditorGUIUtility.standardVerticalSpacing;
                    height += PersistentGUI.GetDefaultTypeHeight(itemLabel, elType);
                }
            }
            else
            {
                height += EditorGUIUtility.singleLineHeight;
            }
            return height;
        }
    }
}