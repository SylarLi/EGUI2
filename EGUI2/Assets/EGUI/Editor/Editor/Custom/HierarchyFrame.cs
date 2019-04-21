using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using EGUI.UI;
using System.Collections.Generic;

namespace EGUI.Editor
{
    internal sealed class HierarchyFrame : UserFrame
    {
        private Node mRoot;

        public Node root { get { return mRoot; } set { mRoot = value; } }

        protected override void OnGUI()
        {
            if (root == null) return;
            for (int i = 0; i < root.childCount; i++)
            {
                OnNodeGUI(root.GetChild(i));
            }
            OnCommonEvent();
        }

        private void OnNodeGUI(Node node)
        {
            var nodeRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            var selected = UserSelection.nodes != null && UserSelection.nodes.Contains(node);
            if (selected) EditorGUI.DrawRect(nodeRect, focused ? UserSetting.HierarchySelectedFocusedColor : UserSetting.HierarchySelectedColor);
            var controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            var eventType = Event.current.GetTypeForControl(controlId);
            var mousePos = Event.current.mousePosition;
            var foldout = PersistentGUI.Caches.GetHierarchyFoldout(node);
            if (node.childCount > 0)
            {
                var fsize = EditorStyles.foldout.CalcSize(GUIContent.none);
                var frect = new Rect(nodeRect.x + PersistentGUI.indent, nodeRect.y, fsize.x, fsize.y);
                switch (eventType)
                {
                    case EventType.Repaint:
                        {
                            EditorStyles.foldout.Draw(frect, GUIContent.none, controlId, foldout);
                            break;
                        }
                    case EventType.MouseDown:
                        {
                            if (frect.Contains(mousePos))
                            {
                                foldout = !foldout;
                                PersistentGUI.Caches.SetHierarchyFoldout(node, foldout);
                                Event.current.Use();
                            }
                            break;
                        }
                    case EventType.MouseUp:
                    case EventType.MouseDrag:
                        {
                            if (frect.Contains(mousePos))
                            {
                                Event.current.Use();
                            }
                            break;
                        }
                }
            }
            EditorGUI.LabelField(new Rect(nodeRect.x + 12, nodeRect.y, nodeRect.width, nodeRect.height), new GUIContent(node.name));

            var topRect = new Rect(nodeRect.x, nodeRect.y - 2, nodeRect.width, 4);
            var bottomRect = new Rect(nodeRect.x, nodeRect.yMax - 1, nodeRect.width, 4);
            var centerRect = new Rect(nodeRect.x, nodeRect.y + 2, nodeRect.width, nodeRect.height - 4);
            if (UserDragDrop.dragging)
            {
                var indentOffset = PersistentGUI.indent + 12;
                if (topRect.Contains(mousePos))
                {
                    PersistentGUI.DrawAAPolyLine(new Rect(topRect.x + indentOffset, topRect.y, topRect.width - indentOffset, topRect.height), 2, UserSetting.HierarchyDragTipsColor);
                }
                else if (centerRect.Contains(mousePos))
                {
                    PersistentGUI.DrawAAPolyLine(new Rect(centerRect.x + indentOffset, centerRect.y, centerRect.width - indentOffset, centerRect.height), 2, UserSetting.HierarchyDragTipsColor);
                }
            }

            eventType = Event.current.GetTypeForControl(controlId);
            switch (eventType)
            {
                case EventType.MouseDown:
                    {
                        if (nodeRect.Contains(mousePos))
                        {
                            var nodes = UserSelection.nodes != null ? CoreUtil.CopyArray(UserSelection.nodes) : new Node[0];
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
                                List<Node> filter = new List<Node>();
                                List<Node> list = new List<Node>();
                                foreach (var n in root)
                                    FlatternNode(list, n);
                                var index = list.IndexOf(node);
                                var nIndex = list.IndexOf(UserSelection.node);
                                var from = Mathf.Min(nIndex, index);
                                var to = Mathf.Max(nIndex, index);
                                for (int i = from; i <= to; i++)
                                {
                                    if (!filter.Contains(list[i]))
                                        filter.Add(list[i]);
                                }
                                UserUtil.SelectNodes(filter.ToArray());
                            }
                            else
                            {
                                if (!selected)
                                    UserUtil.SelectNodes(new Node[] { node });
                                if (node.childCount > 0 && Event.current.clickCount == 2)
                                {
                                    foldout = !foldout;
                                    PersistentGUI.Caches.SetHierarchyFoldout(node, foldout);
                                }
                            }
                            Event.current.Use();
                        }
                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (UserDragDrop.dragging)
                        {
                            var nodes = UserDragDrop.data as Node[];
                            if (nodes != null && nodes.Length > 0)
                            {
                                if (topRect.Contains(mousePos))
                                {
                                    UserUtil.MoveNodes(nodes, node.parent, node.GetSiblingIndex());
                                    UserDragDrop.StopDrag();
                                    UserCursor.SetState(UserCursor.State.Default);
                                    PersistentGUI.Caches.SetHierarchyFoldout(node.parent, true);
                                    Event.current.Use();
                                }
                                else if (bottomRect.Contains(mousePos))
                                {
                                    UserUtil.MoveNodes(nodes, node.parent, node.GetSiblingIndex() + 1);
                                    UserDragDrop.StopDrag();
                                    UserCursor.SetState(UserCursor.State.Default);
                                    PersistentGUI.Caches.SetHierarchyFoldout(node.parent, true);
                                    Event.current.Use();
                                }
                                else if (centerRect.Contains(mousePos))
                                {
                                    UserUtil.MoveNodes(nodes, node, -1);
                                    UserDragDrop.StopDrag();
                                    UserCursor.SetState(UserCursor.State.Default);
                                    PersistentGUI.Caches.SetHierarchyFoldout(node, true);
                                    Event.current.Use();
                                }
                            }
                        }
                        else
                        {
                            if (Event.current.button == 0 && nodeRect.Contains(mousePos))
                            {
                                var nodes = UserSelection.nodes != null ? CoreUtil.CopyArray(UserSelection.nodes) : new Node[0];
                                if (Event.current.control)
                                {
                                    if (selected)
                                    {
                                        var list = GUIUtility.QueryStateObject(typeof(List<Node>), controlId) as List<Node>;
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
                                        UserUtil.SelectNodes(new Node[] { node });
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
                            var nodes = UserSelection.nodes;    
                            if (nodes != null && nodes.Contains(node) && nodeRect.Contains(mousePos))
                            {
                                nodes = new Node[UserSelection.nodes.Length];
                                Array.Copy(UserSelection.nodes, nodes, nodes.Length);
                                UserDragDrop.StartDrag(nodes);
                                UserCursor.SetState(UserCursor.State.DragAnything);
                                Event.current.Use();
                            }
                        }
                        break;
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
                        PersistentGUI.DrawAAPolyLine(new Rect(bottomRect.x + indentOffset, bottomRect.y, bottomRect.width - indentOffset, bottomRect.height), 2, UserSetting.HierarchyDragTipsColor);
                    }
                }
            }
        }

        private void OnCommonEvent()
        {
            var controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            var eventType = Event.current.GetTypeForControl(controlId);
            switch (eventType)
            {
                case EventType.MouseDown:
                    {
                        if (new Rect(0, 0, rect.width, rect.height).Contains(Event.current.mousePosition))
                        {
                            if (Event.current.button == 0)
                            {
                                UserUtil.SelectNodes(null);
                                Event.current.Use();
                            }
                        }
                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (new Rect(0, 0, rect.width, rect.height).Contains(Event.current.mousePosition))
                        {
                            if (Event.current.button == 1)
                            {
                                UserMenu.ShowNodeContext(root);
                                Event.current.Use();
                            }
                        }
                        break;
                    }
                case EventType.KeyDown:
                    {
                        switch (Event.current.keyCode)
                        {
                            case KeyCode.Delete:
                                {
                                    UserUtil.DeleteNodes(root);
                                    Event.current.Use();
                                    break;
                                }
                            case KeyCode.UpArrow:
                            case KeyCode.DownArrow:
                            case KeyCode.LeftArrow:
                            case KeyCode.RightArrow:
                                {
                                    var node = UserSelection.node;
                                    if (node != null)
                                    {
                                        List<Node> list = new List<Node>();
                                        foreach (var n in root)
                                            FlatternNode(list, n);
                                        var index = list.IndexOf(node);
                                        switch (Event.current.keyCode)
                                        {
                                            case KeyCode.UpArrow:
                                                {
                                                    if (index > 0)
                                                        UserUtil.SelectNodes(new Node[] { list[index - 1] });
                                                    break;
                                                }
                                            case KeyCode.DownArrow:
                                                {
                                                    if (index < list.Count - 1)
                                                        UserUtil.SelectNodes(new Node[] { list[index + 1] });
                                                    break;
                                                }
                                            case KeyCode.LeftArrow:
                                                {
                                                    if (node.childCount > 0)
                                                        PersistentGUI.Caches.SetHierarchyFoldout(node, false);
                                                    break;
                                                }
                                            case KeyCode.RightArrow:
                                                {
                                                    if (node.childCount > 0)
                                                        PersistentGUI.Caches.SetHierarchyFoldout(node, true);
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
                        if (Event.current.commandName == "Copy" ||
                            Event.current.commandName == "Paste" ||
                            Event.current.commandName == "Duplicate" ||
                            Event.current.commandName == "SelectAll")
                            Event.current.Use();
                        break;
                    }
                case EventType.ExecuteCommand:
                    {
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
                                        FlatternNode(list, n);
                                    UserUtil.SelectNodes(list.ToArray());
                                    Event.current.Use();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void FlatternNode(ICollection<Node> list, Node node)
        {
            list.Add(node);
            if (PersistentGUI.Caches.GetHierarchyFoldout(node))
                foreach (var n in node)
                    FlatternNode(list, n);
        }
    }
}
