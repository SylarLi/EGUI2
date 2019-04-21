using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using EGUI.UI;

namespace EGUI.Editor
{
    internal class SceneFrame : UserFrame
    {
        private Node mRoot;

        public Node root { get { return mRoot; } set { mRoot = value; } }

        private bool mPreview;

        public bool preview { get { return mPreview; } set { mPreview = value; } }

        protected override void OnGUI()
        {
            if (root == null) return;
            root.GetLeaf<EventSystem>().enabled = preview;
            root.Update();
            if (preview) return;
            OnDrawSelection();
            OnCommonEvent();
        }

        private void OnDrawSelection()
        {
            var selectedNodes = UserSelection.nodes;
            if (selectedNodes == null || selectedNodes.Length == 0) return;
            for (int i = 0; i < selectedNodes.Length; i++)
            {
                var selectedNode = selectedNodes[i];
                PersistentGUI.BeginMatrix(GUI.matrix * selectedNode.local2WorldMatrix);
                var localRect = selectedNode.localRect;
                localRect.x += UserSetting.SceneNodeSelectionLineWidth / 2;
                localRect.y += UserSetting.SceneNodeSelectionLineWidth / 2;
                PersistentGUI.DrawAAPolyLine(localRect, UserSetting.SceneNodeSelectionLineWidth, UserSetting.SceneNodeSelectionColor);
                var controlId = GUIUtility.GetControlID(FocusType.Keyboard);
                var eventType = Event.current.GetTypeForControl(controlId);
                switch (eventType)
                {
                    case EventType.MouseMove:
                        {
                            var dragRects = CalcDragSizeRect(localRect, 4);
                            var dir = Array.FindIndex(dragRects, r => r.Contains(Event.current.mousePosition));
                            switch (dir)
                            {
                                case Direction.Top:
                                case Direction.Bottom:
                                    {
                                        UserCursor.SetState(UserCursor.State.Vertical);
                                        Event.current.Use();
                                        break;
                                    }
                                case Direction.Left:
                                case Direction.Right:
                                    {
                                        UserCursor.SetState(UserCursor.State.Horizontal);
                                        Event.current.Use();
                                        break;
                                    }
                                case Direction.TopRight:
                                case Direction.BottomLeft:
                                    {
                                        UserCursor.SetState(UserCursor.State.Diagonal1);
                                        Event.current.Use();
                                        break;
                                    }
                                case Direction.TopLeft:
                                case Direction.BottomRight:
                                    {
                                        UserCursor.SetState(UserCursor.State.Diagonal2);
                                        Event.current.Use();
                                        break;
                                    }
                                default:
                                    {
                                        UserCursor.SetState(UserCursor.State.Default);
                                        Event.current.Use();
                                        break;
                                    }
                            }
                            UserCursor.matrix = dir == -1 ? Matrix4x4.identity : Node.BuildLocal2WorldRotateMatrix(selectedNode);
                            break;
                        }
                    case EventType.MouseDown:
                        {
                            var dragRects = CalcDragSizeRect(localRect, 4);
                            var dir = Array.FindIndex(dragRects, r => r.Contains(Event.current.mousePosition));
                            if (dir >= 0 || localRect.Contains(Event.current.mousePosition))
                            {
                                if (Event.current.button == 0)
                                {
                                    GUIUtility.hotControl = controlId;
                                    var dragState = (DragState)GUIUtility.GetStateObject(typeof(DragState), controlId);
                                    dragState.dir = dir;
                                    dragState.localPosition = selectedNode.localPosition;
                                    dragState.size = selectedNode.size;
                                    Event.current.Use();
                                }
                            }
                            break;
                        }
                    case EventType.MouseDrag:
                        {
                            if (Event.current.button == 0 &&
                                GUIUtility.hotControl == controlId)
                            {
                                var delta = Event.current.delta;
                                var scale = selectedNode.localScale;
                                scale.x = Mathf.Abs(scale.x);
                                scale.y = Mathf.Abs(scale.y);
                                var pivot = selectedNode.pivot;
                                selectedNode.pivot = Vector2.zero;
                                var deltaMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, selectedNode.localAngle), scale);
                                var dragState = (DragState)GUIUtility.GetStateObject(typeof(DragState), controlId);
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
                                Event.current.Use();
                            }
                            break;
                        }
                    case EventType.MouseUp:
                        {
                            if (GUIUtility.hotControl == controlId)
                            {
                                if (Event.current.button == 0)
                                {
                                    var dragState = (DragState)GUIUtility.GetStateObject(typeof(DragState), controlId);
                                    var localPosition = dragState.localPosition;
                                    var size = dragState.size;
                                    var currentLocalPosition = selectedNode.localPosition;
                                    var currentSize = selectedNode.size;
                                    if (localPosition != currentLocalPosition ||
                                        size != currentSize)
                                    {
                                        selectedNode.localPosition = localPosition;
                                        selectedNode.size = size;
                                        Command.Execute(new CombinedCommand(new Command[]
                                        {
                                            new UpdateMemberCommand(selectedNode, "localPosition", currentLocalPosition),
                                            new UpdateMemberCommand(selectedNode, "size", currentSize),
                                        }));
                                    }
                                }
                                GUIUtility.hotControl = 0;
                                Event.current.Use();
                            }
                            break;
                        }
                }
                PersistentGUI.EndMatrix();
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
                        if (Event.current.button == 0)
                        {
                            var hits = Raycaster.RaycastAll(Event.current.mousePosition, root);
                            ArrayUtility.Remove(ref hits, root);
                            if (hits.Length > 0)
                            {
                                var current = UserSelection.node;
                                if (current == null)
                                {
                                    UserUtil.SelectNodes(new Node[] { hits[0] });
                                    Event.current.Use();
                                }
                                else
                                {
                                    var hold = hits[0];
                                    var nodes = CoreUtil.CopyArray(UserSelection.nodes);
                                    var index = Array.IndexOf(hits, current);
                                    if (index >= 0)
                                    {
                                        ArrayUtility.Remove(ref nodes, current);
                                        var next = index + 1;
                                        if (next >= hits.Length) next = 0;
                                        hold = hits[next];
                                    }
                                    if (Event.current.control)
                                    {
                                        if (nodes.Length > 0)
                                            ArrayUtility.Add(ref nodes, hold);
                                        else
                                            nodes = null;
                                    }
                                    else
                                    {
                                        nodes = new Node[] { hold };
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
                        break;
                    }
            }
        }

        // order: top/right/bottom/left/top-left/top-right/bottom-right/bottom-left
        private Rect[] CalcDragSizeRect(Rect rect, float space)
        {
            return new Rect[]
            {
                new Rect(rect.xMin + space, rect.yMin, rect.width - space * 2, space * 2),
                new Rect(rect.xMax - space, rect.yMin + space, space * 2, rect.height - space * 2),
                new Rect(rect.xMin + space, rect.yMax - space, rect.width - space * 2, space * 2),
                new Rect(rect.xMin, rect.yMin + space, space * 2, rect.height - space * 2),
                new Rect(rect.xMin, rect.yMin, space * 2, space * 2),
                new Rect(rect.xMax - space, rect.yMin, space * 2, space * 2),
                new Rect(rect.xMax - space, rect.yMax - space, space * 2, space * 2),
                new Rect(rect.xMin, rect.yMax - space, space * 2, space * 2),
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
            public Vector2 localPosition;
            public Vector2 size;
        }
    }
}
