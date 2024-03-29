﻿using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using EGUI.UI;

namespace EGUI.Editor
{
    public sealed class UserUtil
    {
        private static Persistence mPersistence;

        public static Persistence persistence { get { mPersistence = mPersistence ?? new Persistence(); return mPersistence; } }

        public static byte[] Serialize(object obj)
        {
            return persistence.Serialize(obj);
        }

        public static object Deserialize(byte[] bytes)
        {
            return persistence.Deserialize(bytes);
        }

        public static object Duplicate(object obj)
        {
            return Deserialize(Serialize(obj));
        }

        public static Node[] Duplicate(Node[] nodes, Node root)
        {
            var refs = new List<Node>();
            FilterNoneNested(refs, nodes, root);
            foreach (var r in refs)
                persistence.Register(r);
            nodes = Duplicate(nodes) as Node[];
            persistence.ClearRegister();
            foreach (var node in nodes)
            {
                node.SetParent(null);
                node.MarkInternalDisposed(true);
            }
            return nodes;
        }

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

        public static void GetDisplayedMembersInType(ICollection<MemberInfo> members, Type type)
        {
            var fields = new List<FieldInfo>();
            var fieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            CoreUtil.GetFields(fields, type, fieldFlags, true);
            foreach (var f in fields)
                if (!f.IsLiteral && !f.IsInitOnly && CoreUtil.IsSafetyReflectionType(f.FieldType))
                    members.Add(f);
        }

        internal static void AddLeaf(Node[] nodes, Type type)
        {
            var commands = new List<Command>();
            foreach (var node in nodes)
            {
                var requiredTypes = CoreUtil.GetRequiredTypes(type);
                foreach (var requiredType in requiredTypes)
                {
                    if (node.GetLeaf(requiredType) == null)
                        commands.Add(new NodeAddLeafCommand(node, (Leaf)Activator.CreateInstance(requiredType)));
                }
                commands.Add(new NodeAddLeafCommand(node, (Leaf)Activator.CreateInstance(type)));
            }
            Command.Execute(new CombinedCommand(commands.ToArray()));
        }

        internal static void RemoveLeaf(Leaf[] leaves)
        {
            var nested = leaves.Where(CoreUtil.LeafIsRequiredByOthers);
            if (nested.Any())
            {
                EditorUtility.DisplayDialog(Locale.L_Tips, string.Format(Locale.L_RemoveLeafFailed, nested.First().GetType().Name), Locale.L_Close);
            }
            else
            {
                var commands = (from leaf in leaves
                                select new NodeRemoveLeafCommand(leaf.node, leaf)).ToArray();
                Command.Execute(new CombinedCommand(commands));
            }
        }

        internal static void SelectNodes(Node[] nodes)
        {
            if (!CoreUtil.CompareIList(UserDatabase.selection.nodes, nodes))
            {
                Command.Execute(new NodeSelectionCommand(nodes));
            }
        }

        internal static void MoveNodes(Node[] nodes, Node parent, int index)
        {
            var commands = new List<Command>();
            var childCount = parent.childCount;
            foreach (var node in nodes)
            {
                var targetIndex = index;
                if (node.parent == parent && (targetIndex < 0 || targetIndex >= childCount))
                {
                    targetIndex = childCount - 1;
                }
                if (!node.IsAncestorOf(parent) && node != parent && (node.parent != parent || node.GetSiblingIndex() != targetIndex))
                {
                    commands.Add(new NodeMoveCommand(node, parent, targetIndex));
                    childCount += 1;
                    index += 1;
                }
            }
            if (commands.Count > 0)
            {
                Command.Execute(new CombinedCommand(commands.ToArray()));
            }
        }

        internal static void CopyNodes(Node root)
        {
            var nodes = UserDatabase.selection.nodes ?? new Node[0];
            var pending = FilterNested(FilterDuplicated(nodes));
            if (pending.Length > 0)
            {
                pending = Duplicate(pending, root);
                UserClipBoard.Copy(pending);
            }
        }

        internal static void PasteNodes(Node root)
        {
            var parent = UserDatabase.selection.node != null ? UserDatabase.selection.node.parent : root;
            var pending = UserClipBoard.Paste() as Node[];
            if (pending != null && pending.Length > 0)
            {
                Command.Execute(new NodeDuplicateThenSelectionCommand(pending, pending.Select(i => parent).ToArray(), root));
            }
        }

        internal static void DuplicateNodes(Node root)
        {
            var nodes = UserDatabase.selection.nodes ?? new Node[0];
            var pending = FilterNested(FilterDuplicated(nodes));
            if (pending.Length > 0)
            {
                Command.Execute(new NodeDuplicateThenSelectionCommand(pending, pending.Select(i => i.parent).ToArray(), root));
            }
        }

        internal static void DeleteNodes()
        {
            var nodes = UserDatabase.selection.nodes ?? new Node[0];
            var pending = FilterNested(FilterDuplicated(nodes));
            if (pending.Length > 0)
            {
                Command.Execute(new NodeDeleteThenUpdateSelectionCommand(pending));
            }
        }

        internal static void CreateControl(Type type, Node root)
        {
            var nodes = UserDatabase.selection.nodes;
            if (nodes == null || nodes.Length == 0) nodes = new Node[] { root };
            var pending = FilterNested(FilterDuplicated(nodes));
            if (pending.Length > 0)
            {
                var parent = pending[pending.Length - 1];
                UserDatabase.caches.SetHierarchyFoldout(parent, true);
                Command.Execute(new DefaultControlCreateThenUpdateSelectionCommand(parent, type));
            }
        }

        internal static Node[] FilterDuplicated(Node[] nodes)
        {
            var list = new List<Node>();
            foreach (var node in nodes)
            {
                if (!list.Contains(node))
                    list.Add(node);
            }
            return list.ToArray();
        }

        internal static Node[] FilterNested(Node[] nodes)
        {
            var list = new List<Node>();
            if (nodes != null && nodes.Length > 0)
            {
                list.Add(nodes[0]);
                for (int i = 1; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    var omit = false;
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (node.IsAncestorOf(list[j]))
                        {
                            list[j] = node;
                            omit = true;
                            break;
                        }
                        else if (list[j].IsAncestorOf(node))
                        {
                            omit = true;
                            break;
                        }
                    }
                    if (!omit)
                        list.Add(node);
                }
            }
            return list.ToArray();
        }

        internal static void FilterNoneNested(ICollection<Node> list, Node[] regular, Node node)
        {
            if (!regular.Contains(node))
            {
                list.Add(node);
                foreach (var n in node)
                    FilterNoneNested(list, regular, n);
            }
        }

        internal static Node TraverseRoot(Node node)
        {
            Debug.Assert(node != null);
            while (node.parent != null)
                node = node.parent;
            return node;
        }

        internal static void SaveFile(Node root, string path)
        {
            var eventSystem = root.GetLeaf<EventSystem>();
            var enabled = eventSystem.enabled;
            eventSystem.enabled = true;
            var archetype = ScriptableObject.CreateInstance<Archetype>();
            archetype.data = persistence.Serialize(root);
            AssetDatabase.CreateAsset(archetype, path);
            eventSystem.enabled = enabled;
        }

        internal static string SaveFileAs(Node root)
        {
            var path = EditorUtility.SaveFilePanelInProject("Save as", "Archetype", "asset", "Save Archetype");
            if (!string.IsNullOrEmpty(path))
                SaveFile(root, path);
            return path;
        }

        internal static void LoadFile(out Node root, out string path)
        {
            root = null;
            path = EditorUtility.OpenFilePanel("Load", "", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Replace(Application.dataPath, "");
                var archetype = AssetDatabase.LoadAssetAtPath<Archetype>(path);
                root = archetype != null ? persistence.Deserialize<Node>(archetype.data) : null;    
            }
        }
    }
}