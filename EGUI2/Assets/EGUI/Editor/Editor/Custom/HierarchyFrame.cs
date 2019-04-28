using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace EGUI.Editor
{
    internal sealed class HierarchyFrame : UserFrame
    {
        private Node mRoot;

        public Node root
        {
            get { return mRoot; }
            set { mRoot = value; }
        }

        protected override void OnGUI()
        {
            if (root == null) return;
            OnDrawTips();
            for (int i = 0; i < root.childCount; i++)
            {
                OnNodeGUI(root.GetChild(i));
            }

            OnCommonEvent();
        }
        
        private void OnDrawTips()
        {
            if (root.childCount == 0)
            {
                EditorGUI.LabelField(nativeRect, Locale.L_Hierarchy, UserSetting.FrameTipsLabelStyle);
            }
        }

        private void OnNodeGUI(Node node)
        {
            var nodeRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            nodeRect.height += EditorGUIUtility.standardVerticalSpacing;
            var selected = UserDatabase.selection.nodes != null && UserDatabase.selection.nodes.Contains(node);
            if (selected)
                EditorGUI.DrawRect(nodeRect,
                    focused ? UserSetting.HierarchySelectedFocusedColor : UserSetting.HierarchySelectedColor);
            var controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            var eventType = Event.current.GetTypeForControl(controlId);
            var mousePos = Event.current.mousePosition;
            var foldout = UserDatabase.caches.GetHierarchyFoldout(node);
            var topRect = new Rect(nodeRect.x, nodeRect.y - 2, nodeRect.width, 4);
            var bottomRect = new Rect(nodeRect.x, nodeRect.yMax - 1, nodeRect.width, 4);
            var centerRect = new Rect(nodeRect.x, nodeRect.y + 2, nodeRect.width, nodeRect.height - 4);
            var fsize = EditorStyles.foldout.CalcSize(GUIContent.none);
            var frect = new Rect(nodeRect.x + PersistentGUI.indent, nodeRect.y, fsize.x, fsize.y);
            switch (eventType)
            {
                case EventType.Repaint:
                {
                    if (node.childCount > 0)
                        EditorStyles.foldout.Draw(frect, GUIContent.none, controlId, foldout);
                    break;
                }
                case EventType.MouseDown:
                {
                    if (node.childCount > 0 &&
                        frect.Contains(mousePos))
                    {
                        foldout = !foldout;
                        UserDatabase.caches.SetHierarchyFoldout(node, foldout);
                        Event.current.Use();
                    }
                    else if (nodeRect.Contains(mousePos) &&
                             (Event.current.button == 0 ||
                              Event.current.button == 1))
                    {
                        var nodes = UserDatabase.selection.nodes != null
                            ? CoreUtil.CopyArray(UserDatabase.selection.nodes)
                            : new Node[0];
                        if (Event.current.control)
                        {
                            if (selected)
                            {
                                ArrayUtility.Remove(ref nodes, node);
                            }
                            else
                            {
                                ArrayUtility.Add(ref nodes, node);
                                var list = GUIUtility.GetStateObject(typeof(List<Node>), controlId) as List<Node>;
                                list.Add(node);
                            }

                            UserUtil.SelectNodes(nodes);
                        }
                        else if (Event.current.shift && nodes.Length > 0)
                        {
                            var filter = new List<Node>();
                            var list = new List<Node>();
                            foreach (var n in root)
                                FlattenNode(list, n);
                            var index = list.IndexOf(node);
                            var nIndex = list.IndexOf(UserDatabase.selection.node);
                            var from = Mathf.Min(nIndex, index);
                            var to = Mathf.Max(nIndex, index);
                            for (var i = @from; i <= to; i++)
                            {
                                if (!filter.Contains(list[i]))
                                    filter.Add(list[i]);
                            }

                            UserUtil.SelectNodes(filter.ToArray());
                        }
                        else
                        {
                            if (!selected)
                                UserUtil.SelectNodes(new Node[] {node});
                            if (node.childCount > 0 && Event.current.clickCount == 2)
                            {
                                foldout = !foldout;
                                UserDatabase.caches.SetHierarchyFoldout(node, foldout);
                            }
                        }

                        Event.current.Use();
                    }

                    break;
                }
                case EventType.MouseUp:
                {
                    if (frect.Contains(mousePos)) break;
                    if (UserDragDrop.dragging)
                    {
                        var nodes = UserDragDrop.data as Node[];
                        if (nodes != null && nodes.Length > 0)
                        {
                            if (topRect.Contains(mousePos))
                            {
                                UserUtil.MoveNodes(nodes, node.parent, node.GetSiblingIndex());
                                UserDragDrop.StopDrag();
                                Cursor.SetState(Cursor.State.Default);
                                UserDatabase.caches.SetHierarchyFoldout(node.parent, true);
                                Event.current.Use();
                            }
                            else if (bottomRect.Contains(mousePos))
                            {
                                UserUtil.MoveNodes(nodes, node.parent, node.GetSiblingIndex() + 1);
                                UserDragDrop.StopDrag();
                                Cursor.SetState(Cursor.State.Default);
                                UserDatabase.caches.SetHierarchyFoldout(node.parent, true);
                                Event.current.Use();
                            }
                            else if (centerRect.Contains(mousePos))
                            {
                                UserUtil.MoveNodes(nodes, node, -1);
                                UserDragDrop.StopDrag();
                                Cursor.SetState(Cursor.State.Default);
                                UserDatabase.caches.SetHierarchyFoldout(node, true);
                                Event.current.Use();
                            }
                        }
                    }
                    else
                    {
                        if (Event.current.button == 0 && nodeRect.Contains(mousePos))
                        {
                            var nodes = UserDatabase.selection.nodes != null
                                ? CoreUtil.CopyArray(UserDatabase.selection.nodes)
                                : new Node[0];
                            if (Event.current.control)
                            {
                                if (selected)
                                {
                                    var list =
                                        GUIUtility.QueryStateObject(typeof(List<Node>), controlId) as List<Node>;
                                    if (list != null && list.Count > 0)
                                    {
                                        if (list[0] != node)
                                        {
                                            ArrayUtility.Remove(ref nodes, node);
                                            UserUtil.SelectNodes(nodes);
                                        }

                                        list.Clear();
                                    }
                                }
                            }
                            else if (!Event.current.shift)
                            {
                                if (selected)
                                    UserUtil.SelectNodes(new Node[] {node});
                            }

                            if (Event.current.button != 1)
                            {
                                Event.current.Use();
                            }
                        }
                    }

                    break;
                }
                case EventType.MouseDrag:
                {
                    if (Event.current.button == 0 && !UserDragDrop.dragging)
                    {
                        var nodes = UserDatabase.selection.nodes;
                        if (nodes != null && nodes.Contains(node) && nodeRect.Contains(mousePos))
                        {
                            var data = new Node[nodes.Length];
                            Array.Copy(nodes, data, data.Length);
                            UserDragDrop.StartDrag(data);
                            Cursor.SetState(Cursor.State.DragAnything);
                            Event.current.Use();
                        }
                    }

                    break;
                }
            }

            EditorGUI.LabelField(new Rect(nodeRect.x + 12, nodeRect.y, nodeRect.width, nodeRect.height),
                new GUIContent(node.name));
            if (UserDragDrop.dragging)
            {
                var indentOffset = PersistentGUI.indent + 12;
                if (topRect.Contains(mousePos))
                {
                    PersistentGUI.DrawAAPolyLine(
                        new Rect(topRect.x + indentOffset, topRect.y, topRect.width - indentOffset, topRect.height), 2,
                        UserSetting.HierarchyDragTipsColor);
                }
                else if (centerRect.Contains(mousePos))
                {
                    PersistentGUI.DrawAAPolyLine(
                        new Rect(centerRect.x + indentOffset, centerRect.y, centerRect.width - indentOffset,
                            centerRect.height), 2, UserSetting.HierarchyDragTipsColor);
                }
            }

            if (foldout && node.childCount > 0)
            {
                EditorGUI.indentLevel += 1;
                for (int i = 0; i < node.childCount; i++)
                {
                    OnNodeGUI(node.GetChild(i));
                }

                EditorGUI.indentLevel -= 1;
            }
            else
            {
                if (UserDragDrop.dragging)
                {
                    if (bottomRect.Contains(mousePos))
                    {
                        var indentOffset = PersistentGUI.indent + 12;
                        PersistentGUI.DrawAAPolyLine(
                            new Rect(bottomRect.x + indentOffset, bottomRect.y, bottomRect.width - indentOffset,
                                bottomRect.height), 2, UserSetting.HierarchyDragTipsColor);
                    }
                }
            }
        }

        private void OnCommonEvent()
        {
            var controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            var eventType = Event.current.GetTypeForControl(controlId);
            var mousePos = Event.current.mousePosition;
            switch (eventType)
            {
                case EventType.MouseDown:
                {
                    if (Event.current.button == 0 &&
                        nativeRect.Contains(mousePos))
                    {
                        UserUtil.SelectNodes(null);
                        Event.current.Use();
                    }

                    break;
                }
                case EventType.MouseUp:
                {
                    if (Event.current.button == 1 &&
                        nativeRect.Contains(mousePos))
                    {
                        UserMenu.ShowNodeContext(root);
                        Event.current.Use();
                    }

                    break;
                }
                case EventType.KeyDown:
                {
                    if (!focused) break;
                    switch (Event.current.keyCode)
                    {
                        case KeyCode.Delete:
                        {
                            UserUtil.DeleteNodes();
                            Event.current.Use();
                            break;
                        }
                        case KeyCode.UpArrow:
                        case KeyCode.DownArrow:
                        case KeyCode.LeftArrow:
                        case KeyCode.RightArrow:
                        {
                            var node = UserDatabase.selection.node;
                            if (node != null)
                            {
                                List<Node> list = new List<Node>();
                                foreach (var n in root)
                                    FlattenNode(list, n);
                                var index = list.IndexOf(node);
                                switch (Event.current.keyCode)
                                {
                                    case KeyCode.UpArrow:
                                    {
                                        if (index > 0)
                                            UserUtil.SelectNodes(new Node[] {list[index - 1]});
                                        break;
                                    }
                                    case KeyCode.DownArrow:
                                    {
                                        if (index < list.Count - 1)
                                            UserUtil.SelectNodes(new Node[] {list[index + 1]});
                                        break;
                                    }
                                    case KeyCode.LeftArrow:
                                    {
                                        if (node.childCount > 0)
                                            UserDatabase.caches.SetHierarchyFoldout(node, false);
                                        break;
                                    }
                                    case KeyCode.RightArrow:
                                    {
                                        if (node.childCount > 0)
                                            UserDatabase.caches.SetHierarchyFoldout(node, true);
                                        break;
                                    }
                                }

                                Event.current.Use();
                            }

                            break;
                        }
                    }

                    break;
                }
                case EventType.ValidateCommand:
                {
                    if (!focused) break;
                    if (Event.current.commandName == "Copy" ||
                        Event.current.commandName == "Paste" ||
                        Event.current.commandName == "Duplicate" ||
                        Event.current.commandName == "SelectAll")
                        Event.current.Use();
                    break;
                }
                case EventType.ExecuteCommand:
                {
                    if (!focused) break;
                    switch (Event.current.commandName)
                    {
                        case "Copy":
                        {
                            UserUtil.CopyNodes(root);
                            Event.current.Use();
                            break;
                        }
                        case "Paste":
                        {
                            UserUtil.PasteNodes(root);
                            Event.current.Use();
                            break;
                        }
                        case "Duplicate":
                        {
                            UserUtil.DuplicateNodes(root);
                            Event.current.Use();
                            break;
                        }
                        case "SelectAll":
                        {
                            var list = new List<Node>();
                            foreach (var n in root)
                                FlattenNode(list, n);
                            UserUtil.SelectNodes(list.ToArray());
                            Event.current.Use();
                            break;
                        }
                    }

                    break;
                }
            }
        }

        private void FlattenNode(ICollection<Node> list, Node node)
        {
            list.Add(node);
            if (UserDatabase.caches.GetHierarchyFoldout(node))
                foreach (var n in node)
                    FlattenNode(list, n);
        }

        public void ScrollTo(Node node)
        {
            var offset = GetOffset(node);
            var top = scrollPos.y;
            var bottom = top + rect.height;
            if (offset < top)
            {
                scrollPos = new Vector2(scrollPos.x, offset);
            }
            else if (offset > bottom)
            {
                scrollPos = new Vector2(scrollPos.x,
                    offset - bottom + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }
        }

        private float GetHeight(Node node)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing +
                   node.Where(child => UserDatabase.caches.GetHierarchyFoldout(child))
                       .Select(GetHeight).Sum();
        }

        private float GetFowardSiblingsHeight(Node node)
        {
            var height = 0f;
            if (node.parent != null)
            {
                for (var i = node.GetSiblingIndex() - 1; i >= 0; i--)
                {
                    height += GetHeight(node.parent.GetChild(i));
                }
            }

            return height;
        }

        private float GetOffset(Node node)
        {
            var height = 0f;
            while (node.parent != null)
            {
                height += GetFowardSiblingsHeight(node);
                if (node.parent != root)
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                node = node.parent;
            }

            return height;
        }
    }
}