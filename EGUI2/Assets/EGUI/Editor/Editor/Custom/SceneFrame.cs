using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Net.Sockets;
using System.Xml.Schema;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;
using EventSystem = EGUI.UI.EventSystem;

namespace EGUI.Editor
{
    internal class SceneFrame : UserFrame
    {
        private Node mRoot;

        public Node root
        {
            get { return mRoot; }
            set { mRoot = value; }
        }

        private bool mPreview;

        public bool preview
        {
            get { return mPreview; }
            set { mPreview = value; }
        }

        protected override void OnResize()
        {
            if (root != null)
                root.size = new Vector2(rect.width, rect.height);
        }

        protected override void OnGUI()
        {
            if (root == null) return;
            root.GetLeaf<EventSystem>().enabled = preview;
            if (!preview) DrawNetGrid();
            root.Update();
            if (preview) return;
            OnDrawTips();
            OnDrawSelection();
            OnCommonEvent();
        }

        private void OnDrawTips()
        {
            if (root.childCount == 0)
            {
                EditorGUI.LabelField(nativeRect, Locale.L_EmptySceneTips, UserSetting.FrameTipsLabelStyle);
            }
        }

        private void DrawNetGrid()
        {
            PersistentGUI.BeginHandlesColor(UserSetting.NetGridLineColor);
            for (int x = 0, index = 0; x < rect.width; x += UserSetting.NetGridLineSpace, index += 1)
                Handles.DrawAAPolyLine(UserSetting.NetGridLineWidth * (index % 10 == 0 ? 2 : 1), new Vector3(x, 0, 0),
                    new Vector3(x, rect.height, 0));
            for (int y = 0, index = 0; y < rect.height; y += UserSetting.NetGridLineSpace, index += 1)
                Handles.DrawAAPolyLine(UserSetting.NetGridLineWidth * (index % 10 == 0 ? 2 : 1), new Vector3(0, y, 0),
                    new Vector3(rect.width, y, 0));
            PersistentGUI.EndHandlesColor();
        }

        private void OnDrawSelection()
        {
            var selectedNodes = UserDatabase.selection.nodes;
            if (selectedNodes == null || selectedNodes.Length == 0) return;
            foreach (var selectedNode in selectedNodes)
            {
                PersistentGUI.BeginMatrix(GUI.matrix * selectedNode.GUIMatrix);
                var localRect = selectedNode.localRect;
                var scale = selectedNode.worldScale;
                scale.x = Mathf.Max(scale.x, 0.01f);
                scale.y = Mathf.Max(scale.y, 0.01f);
                var ww = UserSetting.SceneNodeSelectionLineWidth / scale.x;
                var wh = UserSetting.SceneNodeSelectionLineWidth / scale.y;
                var ww2 = ww * 0.5f;
                var wh2 = wh * 0.5f;
                PersistentGUI.BeginHandlesColor(UserSetting.SceneNodeSelectionColor);
                Handles.DrawAAPolyLine(wh,
                    new Vector3(localRect.x + ww2, localRect.y + wh2, 0),
                    new Vector3(localRect.xMax - ww2, localRect.y + wh2, 0));
                Handles.DrawAAPolyLine(ww,
                    new Vector3(localRect.xMax - ww2, localRect.y + wh2, 0),
                    new Vector3(localRect.xMax - ww2, localRect.yMax - wh2, 0));
                Handles.DrawAAPolyLine(wh,
                    new Vector3(localRect.xMax - ww2, localRect.yMax - wh2, 0),
                    new Vector3(localRect.x + ww2, localRect.yMax - wh2, 0));
                Handles.DrawAAPolyLine(ww,
                    new Vector3(localRect.x + ww2, localRect.yMax - wh2, 0),
                    new Vector3(localRect.x + ww2, localRect.y + wh2, 0));
                PersistentGUI.EndHandlesColor();
                PersistentGUI.EndMatrix();
            }
        }

        private void OnCommonEvent()
        {
            var controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            var eventType = Event.current.GetTypeForControl(controlId);
            switch (eventType)
            {
                case EventType.MouseMove:
                {
                    if (!nativeRect.Contains(Event.current.mousePosition)) break;
                    var selectedNodes = UserDatabase.selection.nodes;
                    if (selectedNodes == null || selectedNodes.Length == 0) break;
                    var flag = false;
                    foreach (var selectedNode in selectedNodes)
                    {
                        PersistentGUI.BeginMatrix(GUI.matrix * selectedNode.GUIMatrix);
                        var mousePos = Event.current.mousePosition;
                        var localRect = selectedNode.localRect;
                        var scale = selectedNode.worldScale;
                        scale.x = Mathf.Max(scale.x, 0.01f);
                        scale.y = Mathf.Max(scale.y, 0.01f);
                        var dragRects = CalcDragSizeRect(localRect, 4f / scale.x, 4f / scale.y);
                        var dir = Array.FindIndex(dragRects, r => r.Contains(mousePos));
                        switch (dir)
                        {
                            case Direction.Top:
                            case Direction.Bottom:
                            {
                                Cursor.SetState(Cursor.State.Vertical);
                                Event.current.Use();
                                break;
                            }
                            case Direction.Left:
                            case Direction.Right:
                            {
                                Cursor.SetState(Cursor.State.Horizontal);
                                Event.current.Use();
                                break;
                            }
                            case Direction.TopRight:
                            case Direction.BottomLeft:
                            {
                                Cursor.SetState(Cursor.State.Diagonal1);
                                Event.current.Use();
                                break;
                            }
                            case Direction.TopLeft:
                            case Direction.BottomRight:
                            {
                                Cursor.SetState(Cursor.State.Diagonal2);
                                Event.current.Use();
                                break;
                            }
                        }

                        PersistentGUI.EndMatrix();
                        if (dir >= 0)
                        {
                            Cursor.matrix = Node.BuildGUIRotationMatrix(selectedNode);
                            flag = true;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        Cursor.SetState(Cursor.State.Default);
                        Cursor.matrix = Matrix4x4.identity;
                        Event.current.Use();
                    }

                    break;
                }
                case EventType.MouseDown:
                {
                    if (!nativeRect.Contains(Event.current.mousePosition)) break;
                    if (Event.current.button != 0) break;
                    GUIUtility.hotControl = controlId;
                    var dragState = (DragState) GUIUtility.GetStateObject(typeof(DragState), controlId);
                    dragState.dir = -1;
                    dragState.mousePos = Event.current.mousePosition;
                    var selectedNodes = UserDatabase.selection.nodes;
                    if (selectedNodes == null || selectedNodes.Length == 0) break;
                    foreach (var selectedNode in selectedNodes)
                    {
                        PersistentGUI.BeginMatrix(GUI.matrix * selectedNode.GUIMatrix);
                        var mousePos = Event.current.mousePosition;
                        var localRect = selectedNode.localRect;
                        var scale = selectedNode.worldScale;
                        scale.x = Mathf.Max(scale.x, 0.01f);
                        scale.y = Mathf.Max(scale.y, 0.01f);
                        var dragRects = CalcDragSizeRect(localRect, 4f / scale.x, 4f / scale.y);
                        var dir = Array.FindIndex(dragRects, r => r.Contains(mousePos));
                        if (dir >= 0 || localRect.Contains(mousePos))
                        {
                            dragState.dir = dir;
                            dragState.localPosition = selectedNodes.Select(i => i.localPosition).ToArray();
                            dragState.size = selectedNodes.Select(i => i.size).ToArray();
                            break;
                        }

                        PersistentGUI.EndMatrix();
                    }

                    break;
                }
                case EventType.MouseDrag:
                {
                    if (!nativeRect.Contains(Event.current.mousePosition)) break;
                    if (Event.current.button != 0 || GUIUtility.hotControl != controlId) break;
                    var selectedNodes = UserUtil.FilterNested(UserDatabase.selection.nodes);
                    if (selectedNodes == null || selectedNodes.Length == 0) break;
                    foreach (var selectedNode in selectedNodes)
                    {
                        PersistentGUI.BeginMatrix(GUI.matrix * selectedNode.GUIMatrix);
                        var delta = Event.current.delta;
                        var scale = selectedNode.localScale;
                        scale.x = Mathf.Abs(scale.x);
                        scale.y = Mathf.Abs(scale.y);
                        var pivot = selectedNode.pivot;
                        selectedNode.pivot = Vector2.zero;
                        var deltaMatrix = Matrix4x4.TRS(Vector3.zero,
                            Quaternion.Euler(0, 0, selectedNode.localAngle), scale);
                        var dragState = (DragState) GUIUtility.GetStateObject(typeof(DragState), controlId);
                        switch (dragState.dir)
                        {
                            case Direction.Top:
                            {
                                var offset = deltaMatrix.MultiplyVector(new Vector2(0, delta.y));
                                selectedNode.localPosition += new Vector2(offset.x, offset.y);
                                selectedNode.size += new Vector2(0, -delta.y);
                                break;
                            }
                            case Direction.Bottom:
                            {
                                selectedNode.size += new Vector2(0, delta.y);
                                break;
                            }
                            case Direction.Left:
                            {
                                var offset = deltaMatrix.MultiplyVector(new Vector2(delta.x, 0));
                                selectedNode.localPosition += new Vector2(offset.x, offset.y);
                                selectedNode.size += new Vector2(-delta.x, 0);
                                break;
                            }
                            case Direction.Right:
                            {
                                selectedNode.size += new Vector2(delta.x, 0);
                                break;
                            }
                            case Direction.TopLeft:
                            {
                                var offset = deltaMatrix.MultiplyVector(new Vector2(delta.x, delta.y));
                                selectedNode.localPosition += new Vector2(offset.x, offset.y);
                                selectedNode.size += new Vector2(-delta.x, -delta.y);
                                break;
                            }
                            case Direction.TopRight:
                            {
                                var offset = deltaMatrix.MultiplyVector(new Vector2(0, delta.y));
                                selectedNode.localPosition += new Vector2(offset.x, offset.y);
                                selectedNode.size += new Vector2(delta.x, -delta.y);
                                break;
                            }
                            case Direction.BottomRight:
                            {
                                selectedNode.size += new Vector2(delta.x, delta.y);
                                break;
                            }
                            case Direction.BottomLeft:
                            {
                                var offset = deltaMatrix.MultiplyVector(new Vector2(delta.x, 0));
                                selectedNode.localPosition += new Vector2(offset.x, offset.y);
                                selectedNode.size += new Vector2(-delta.x, delta.y);
                                break;
                            }
                            default:
                            {
                                var offset = deltaMatrix.MultiplyVector(new Vector2(delta.x, delta.y));
                                selectedNode.localPosition += new Vector2(offset.x, offset.y);
                                break;
                            }
                        }

                        selectedNode.pivot = pivot;
                        PersistentGUI.EndMatrix();
                        Event.current.Use();
                    }

                    break;
                }
                case EventType.MouseUp:
                {
                    if (!nativeRect.Contains(Event.current.mousePosition)) break;
                    if (Event.current.button == 1)
                    {
                        UserMenu.ShowNodeContext(root);
                        Event.current.Use();
                    }

                    if (GUIUtility.hotControl != controlId) break;
                    var dragState = (DragState) GUIUtility.GetStateObject(typeof(DragState), controlId);
                    if (Event.current.button == 0)
                    {
                        var selectedNodes = UserDatabase.selection.nodes;
                        if (selectedNodes != null && selectedNodes.Length > 0 && dragState.localPosition != null &&
                            selectedNodes.Length == dragState.localPosition.Length)
                        {
                            var commands = new List<Command>();
                            for (var i = 0; i < selectedNodes.Length; i++)
                            {
                                var selectedNode = selectedNodes[i];
                                PersistentGUI.BeginMatrix(GUI.matrix * selectedNode.GUIMatrix);
                                var localPosition = dragState.localPosition[i];
                                var size = dragState.size[i];
                                var currentLocalPosition = selectedNode.localPosition;
                                var currentSize = selectedNode.size;
                                if (Vector2.Distance(localPosition, currentLocalPosition) >
                                    UserSetting.DistanceComparisionTolerance ||
                                    Vector2.Distance(size, currentSize) > UserSetting.DistanceComparisionTolerance)
                                {
                                    selectedNode.localPosition = localPosition;
                                    selectedNode.size = size;
                                    commands.Add(new UpdateMemberCommand(selectedNode, "localPosition",
                                        currentLocalPosition));
                                    commands.Add(new UpdateMemberCommand(selectedNode, "size", currentSize));
                                }

                                PersistentGUI.EndMatrix();
                            }

                            if (commands.Count > 0)
                            {
                                Command.Execute(new CombinedCommand(commands.ToArray()));
                                Event.current.Use();
                            }
                        }
                    }

                    var mousePos = Event.current.mousePosition;
                    if (Event.current.button == 0 &&
                        Vector2.Distance(dragState.mousePos, mousePos) <
                        UserSetting.DistanceComparisionTolerance)
                    {
                        var hits = Raycaster.RaycastAll(mousePos, root);
                        if (hits.Length > 0)
                        {
                            var current = UserDatabase.selection.node;
                            if (current == null)
                            {
                                UserUtil.SelectNodes(new[] {hits[0]});
                                Event.current.Use();
                            }
                            else
                            {
                                var hold = hits[0];
                                var nodes = CoreUtil.CopyArray(UserDatabase.selection.nodes);
                                foreach (var node in nodes)
                                {
                                    var index = Array.IndexOf(hits, node);
                                    if (index >= 0)
                                    {
                                        ArrayUtility.Remove(ref nodes, node);
                                        var next = index + 1;
                                        if (next >= hits.Length) next = 0;
                                        hold = hits[next];
                                        break;
                                    }
                                }

                                if (Event.current.control)
                                {
                                    if (nodes.Length > 0)
                                        ArrayUtility.Add(ref nodes, hold);
                                }
                                else
                                {
                                    nodes = new Node[] {hold};
                                }

                                UserUtil.SelectNodes(nodes);
                                Event.current.Use();
                            }
                        }
                        else
                        {
                            UserUtil.SelectNodes(null);
                            Event.current.Use();
                        }
                    }

                    GUIUtility.hotControl = 0;

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
                        case KeyCode.LeftArrow:
                        case KeyCode.RightArrow:
                        case KeyCode.UpArrow:
                        case KeyCode.DownArrow:
                        {
                            var selectedNodes = UserUtil.FilterNested(UserDatabase.selection.nodes);
                            if (selectedNodes == null || selectedNodes.Length == 0) break;
                            foreach (var selectedNode in selectedNodes)
                            {
                                var distance = 1;
                                if (Event.current.control) distance = 5;
                                else if (Event.current.shift) distance = 10;
                                switch (Event.current.keyCode)
                                {
                                    case KeyCode.LeftArrow:
                                    {
                                        Command.Execute(new UpdateMemberCommand(selectedNode, "localPosition",
                                            selectedNode.localPosition + new Vector2(-distance, 0)));
                                        Event.current.Use();
                                        break;
                                    }
                                    case KeyCode.UpArrow:
                                    {
                                        Command.Execute(new UpdateMemberCommand(selectedNode, "localPosition",
                                            selectedNode.localPosition + new Vector2(0, -distance)));
                                        Event.current.Use();
                                        break;
                                    }
                                    case KeyCode.RightArrow:
                                    {
                                        Command.Execute(new UpdateMemberCommand(selectedNode, "localPosition",
                                            selectedNode.localPosition + new Vector2(distance, 0)));
                                        Event.current.Use();
                                        break;
                                    }
                                    case KeyCode.DownArrow:
                                    {
                                        Command.Execute(new UpdateMemberCommand(selectedNode, "localPosition",
                                            selectedNode.localPosition + new Vector2(0, distance)));
                                        Event.current.Use();
                                        break;
                                    }
                                }
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
                        Event.current.commandName == "Duplicate")
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
                    }

                    break;
                }
            }
        }

        // order: top/right/bottom/left/top-left/top-right/bottom-right/bottom-left
        private Rect[] CalcDragSizeRect(Rect rect, float width, float height)
        {
            return new Rect[]
            {
                new Rect(rect.xMin + width, rect.yMin - height, rect.width - width * 2, height * 2),
                new Rect(rect.xMax - width, rect.yMin + height, width * 2, rect.height - height * 2),
                new Rect(rect.xMin + width, rect.yMax - height, rect.width - width * 2, height * 2),
                new Rect(rect.xMin - width, rect.yMin + height, width * 2, rect.height - height * 2),
                new Rect(rect.xMin - width, rect.yMin - height, width * 2, height * 2),
                new Rect(rect.xMax - width, rect.yMin - height, width * 2, height * 2),
                new Rect(rect.xMax - width, rect.yMax - height, width * 2, height * 2),
                new Rect(rect.xMin - width, rect.yMax - height, width * 2, height * 2),
            };
        }

        private class Direction
        {
            public const int Top = 0;
            public const int Right = 1;
            public const int Bottom = 2;
            public const int Left = 3;
            public const int TopLeft = 4;
            public const int TopRight = 5;
            public const int BottomRight = 6;
            public const int BottomLeft = 7;
        }

        private class DragState
        {
            public int dir;
            public Vector2[] localPosition;
            public Vector2[] size;
            public Vector2 mousePos;
        }
    }
}