using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace EGUI.Editor
{
    internal class NodeFrame : UserFrame
    {
        private PersistentObject mTarget;

        public PersistentObject target { get { return mTarget; } set { mTarget = value; } }

        protected override void OnGUI()
        {
            if (target == null) return;
            var rect = EditorGUILayout.GetControlRect(false, EditorStyles.toolbarButton.fixedHeight);
            var tsize = EditorStyles.toggle.CalcSize(GUIContent.none);
            var trect = new Rect(rect.x + 5, rect.y + (rect.height - tsize.y) * 0.5f - 1, tsize.x, tsize.y);
            var lheight = EditorStyles.textField.CalcHeight(GUIContent.none, rect.width);
            var lrect = new Rect(trect.xMax, rect.y + (rect.height - lheight) * 0.5f, rect.width - trect.xMax, lheight);
            PersistentGUI.PropertyField(trect, GUIContent.none, target.Find("enabled"));
            PersistentGUI.PropertyField(lrect, GUIContent.none, target.Find("name"));

            var baseDisplays = new string[] { "localPosition", "localAngle", "localScale", "pivot" };
            foreach (var display in baseDisplays)
                PersistentGUILayout.PropertyField(target.Find(display));

            var propStretchWidth = target.Find("stretchWidth");
            if (!propStretchWidth.hasMultipleDifferentValues)
            {
                PersistentGUILayout.PropertyField(propStretchWidth);
                EditorGUI.indentLevel += 1;
                var propWidth = target.Find(propStretchWidth.GetValue<bool>() ? "stretchSize.x" : "size.x");
                PersistentGUILayout.PropertyField(new GUIContent("Width"), propWidth);
                EditorGUI.indentLevel -= 1;
            }
            var propStretchHeight = target.Find("stretchHeight");
            if (!propStretchHeight.hasMultipleDifferentValues)
            {
                PersistentGUILayout.PropertyField(propStretchHeight);
                EditorGUI.indentLevel += 1;
                var propHeight = target.Find(propStretchHeight.GetValue<bool>() ? "stretchSize.y" : "size.y");
                PersistentGUILayout.PropertyField(new GUIContent("Height"), propHeight);
                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.LabelField("Padding");
            EditorGUI.indentLevel += 1;
            PersistentGUILayout.PropertyField(new GUIContent("Left"), target.Find("padding.x"));
            PersistentGUILayout.PropertyField(new GUIContent("Top"), target.Find("padding.y"));
            PersistentGUILayout.PropertyField(new GUIContent("Right"), target.Find("padding.z"));
            PersistentGUILayout.PropertyField(new GUIContent("Bottom"), target.Find("padding.w"));
            EditorGUI.indentLevel -= 1;

            var leafTypes = new List<Type>();
            var nodes = target.GetValues<Node>();
            foreach (var node in nodes)
            {
                var leaves = node.GetAllLeaves();
                foreach (var leaf in leaves)
                {
                    var leafType = leaf.GetType();
                    if (!leafTypes.Contains(leafType))
                    {
                        leafTypes.Add(leafType);
                    }
                }
            }
            leafTypes = leafTypes.FindAll(t => nodes.All(n => n.GetLeaf(t) != null));
            leafTypes.Sort((t1, t2) => t1.Name.CompareTo(t2.Name));
            foreach (var leafType in leafTypes)
            {
                var leaves = Array.ConvertAll(nodes, n => n.GetLeaf(leafType));
                var obj = new PersistentObject(leaves);
                PersistentGUILayout.UserDrawerLayout(obj);
            }
            if (nodes.Length > 1 && nodes.Any(n => n.GetAllLeaves(true).Count() != leafTypes.Count))
            {
                EditorGUILayout.Separator();
                PersistentGUI.BeginColor(Color.yellow);
                EditorGUILayout.LabelField(Language.L_MultiEditLeavesTips);
                PersistentGUI.EndColor();
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button(Language.L_AddLeaf, GUILayout.MaxWidth(100)))
            {
                var allTypes = CoreUtil.FindSubTypes(typeof(Leaf));
                Array.Sort(allTypes, (t1, t2) => t1.Name.CompareTo(t2.Name));
                var menu = new GenericMenu();
                foreach (var type in allTypes)
                {
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        UserUtil.AddLeaf(nodes, type);
                    });
                }
                menu.ShowAsContext();
            }
        }
    }
}
