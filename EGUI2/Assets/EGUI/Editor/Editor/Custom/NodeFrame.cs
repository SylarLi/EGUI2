using System;
using System.Collections.Generic;
using System.Linq;
using EGUI.UI;
using UnityEngine;
using UnityEditor;

namespace EGUI.Editor
{
    internal class NodeFrame : UserFrame
    {
        private PersistentObject mTarget;

        public PersistentObject target
        {
            get { return mTarget; }
            set { mTarget = value; }
        }

        protected override void OnGUI()
        {
            if (target == null)
            {
                EditorGUI.LabelField(nativeRect, Locale.L_Inspector, UserSetting.FrameTipsLabelStyle);
                return;
            }

            PersistentGUI.BeginLabelWidth(100);
            var brect = EditorGUILayout.GetControlRect(false, EditorStyles.toolbarButton.fixedHeight);
            var tsize = EditorStyles.toggle.CalcSize(GUIContent.none);
            var trect = new Rect(brect.x + 5, brect.y + (brect.height - tsize.y) * 0.5f - 1, tsize.x, tsize.y);
            var lheight = EditorStyles.textField.CalcHeight(GUIContent.none, brect.width);
            var lrect = new Rect(trect.xMax, brect.y + (brect.height - lheight) * 0.5f, brect.width - trect.xMax,
                lheight);
            PersistentGUI.PropertyField(trect, GUIContent.none, target.Find("enabled"));
            PersistentGUI.PropertyField(lrect, GUIContent.none, target.Find("name"));

            var nodes = target.GetValues<Node>();
            if (nodes.Length > 1)
            {
                PersistentGUILayout.PropertyField(target.Find("anchoredPosition"));
                PersistentGUILayout.PropertyField(target.Find("size"));
            }
            else
            {
                var node = nodes[0];
                var anchorMin = node.anchorMin;
                var anchorMax = node.anchorMax;
                var labels = new GUIContent[4];
                var props = new PersistentProperty[4];
                if (anchorMin.x == anchorMax.x)
                {
                    labels[0] = new GUIContent("X");
                    props[0] = target.Find("anchoredPosition.x");
                    labels[2] = new GUIContent("W");
                    props[2] = target.Find("size.x");
                }
                else
                {
                    labels[0] = new GUIContent("Left");
                    props[0] = target.Find("offsetMin.x");
                    labels[2] = new GUIContent("Right");
                    props[2] = target.Find("offsetMax.x");
                }

                if (anchorMin.y == anchorMax.y)
                {
                    labels[1] = new GUIContent("Y");
                    props[1] = target.Find("anchoredPosition.y");
                    labels[3] = new GUIContent("H");
                    props[3] = target.Find("size.y");
                }
                else
                {
                    labels[1] = new GUIContent("Top");
                    props[1] = target.Find("offsetMin.y");
                    labels[3] = new GUIContent("Bottom");
                    props[3] = target.Find("offsetMax.y");
                }

                var position = EditorGUILayout.GetControlRect(false,
                    EditorGUIUtility.singleLineHeight * (EditorGUIUtility.wideMode ? 2 : 3));
                position.height = EditorGUIUtility.singleLineHeight;
                PersistentGUI.MultiPropertyField2x2(position, new GUIContent("Position"), labels, props, 50);
            }

            var baseDisplays = new[] {"anchorMin", "anchorMax", "pivot", "localPosition", "localAngle", "localScale"};
            foreach (var display in baseDisplays)
                PersistentGUILayout.PropertyField(target.Find(display));

            var leafTypes = new List<Type>();
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
            leafTypes.Sort((t1, t2) => String.Compare(t1.Name, t2.Name, StringComparison.Ordinal));
            foreach (var leafType in leafTypes)
            {
                var leaves = Array.ConvertAll(nodes, n => n.GetLeaf(leafType));
                var obj = new PersistentObject(leaves);
                PersistentGUILayout.UserDrawerLayout(obj);
                EditorGUILayout.Space();
            }

            if (nodes.Length > 1 && nodes.Any(n => n.GetAllLeaves(true).Count() != leafTypes.Count))
            {
                EditorGUILayout.Separator();
                PersistentGUI.BeginColor(Color.yellow);
                EditorGUILayout.LabelField(Locale.L_MultiEditLeavesTips);
                PersistentGUI.EndColor();
            }

            EditorGUILayout.Separator();
            if (GUILayout.Button(Locale.L_AddLeaf, GUILayout.MaxWidth(100)))
            {
                var allTypes = CoreUtil.FindSubTypes(typeof(Leaf));
                Array.Sort(allTypes, (t1, t2) => String.Compare(t1.Name, t2.Name, StringComparison.Ordinal));
                var menu = new GenericMenu();
                foreach (var type in allTypes)
                {
                    var leafType = type;
                    menu.AddItem(new GUIContent(leafType.Name), false, () => { UserUtil.AddLeaf(nodes, leafType); });
                }

                menu.ShowAsContext();
            }

            PersistentGUI.EndLabelWidth();
        }

        protected override void OnLostFocus()
        {
            GUI.FocusControl(null);
        }
    }
}