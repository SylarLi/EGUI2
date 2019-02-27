using System.Collections.Generic;
using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    public class EventSystem : Leaf
    {
        private List<Node> mTriggers = new List<Node>();

        private Node mMouseDown;

        private Node mDrag;

        public override void Update()
        {
            var eventData = Event.current;
            if (IsValidEvent(eventData.type))
            {
                Trigger(eventData);
            }
        }

        private bool Trigger(Event eventData)
        {
            bool triggered = false;
            mTriggers.Clear();
            if (IsMouseEvent(eventData.type))
            {
                var hits = Raycaster.RaycastAll(eventData.mousePosition, node);
                foreach (var hit in hits)
                {
                    var graphic = hit.GetLeaf<Graphic>();
                    if (graphic != null)
                    {
                        if (!graphic.raycastTarget)
                        {
                            continue;
                        }
                        var interactive = hit.GetLeaf<IInteractive>();
                        if (interactive != null && !interactive.interactive)
                        {
                            continue;
                        }
                    }
                    mTriggers.Add(hit);
                }
            }
            else
            {
                var current = FocusControl.currentSelectable;
                if (current != null && current is Leaf)
                {
                    mTriggers.Add((current as Leaf).node);
                }
            }
            foreach (var trigger in mTriggers)
            {
                var legacyHandler = trigger.GetLeaf<ILegacyEventHandler>(false);
                if (legacyHandler != null)
                {
                    legacyHandler.OnEvent(eventData);
                    triggered = eventData.type == EventType.Used;
                }
                else if (trigger.GetLeaf<IEventSystemHandler>(false) != null)
                {
                    if (IsMouseEvent(eventData.type))
                    {
                        var worldPos = eventData.mousePosition;
                        var localPos = trigger.world2LocalMatrix.MultiplyPoint(worldPos);
                        if (trigger.localRect.Contains(localPos))
                        {
                            switch (eventData.type)
                            {
                                case EventType.MouseDown:
                                    {
                                        triggered = OnDragStop(eventData) || triggered;
                                        triggered = OnClickStart(eventData, trigger) || triggered;
                                        break;
                                    }
                                case EventType.MouseUp:
                                    {
                                        triggered = OnDragStop(eventData) || triggered;
                                        triggered = OnClickStop(eventData, trigger) || triggered;
                                        break;
                                    }
                                case EventType.MouseMove:
                                    {
                                        triggered = OnMouseMove(eventData, trigger) || triggered;
                                        break;
                                    }
                                case EventType.MouseDrag:
                                    {
                                        triggered = OnDragStart(eventData, trigger) || triggered;
                                        triggered = OnDrag(eventData) || triggered;
                                        break;
                                    }
                                case EventType.ScrollWheel:
                                    {
                                        triggered = OnScrollWheel(eventData, trigger) || triggered;
                                        break;
                                    }
                            }
                        }
                    }
                    else if (IsKeyboardEvent(eventData.type))
                    {
                        switch (eventData.type)
                        {
                            case EventType.keyDown:
                                {
                                    triggered = OnKeyDown(eventData, trigger) || triggered;
                                    break;
                                }
                            case EventType.KeyUp:
                                {
                                    triggered = OnKeyUp(eventData, trigger) || triggered;
                                    break;
                                }
                        }
                    }
                }
                if (triggered)
                {
                    break;
                }
            }
            if (!triggered)
            {
                switch (eventData.type)
                {
                    case EventType.MouseDrag:
                        {
                            triggered = OnDrag(eventData) || triggered;
                            break;
                        }
                    case EventType.MouseDown:
                        {
                            triggered = OnDragStop(eventData) || triggered;
                            break;
                        }
                    case EventType.MouseUp:
                        {
                            triggered = OnDragStop(eventData) || triggered;
                            if (mMouseDown != null &&
                                !mTriggers.Contains(mMouseDown))
                            {
                                mMouseDown = null;
                            }
                            break;
                        }
                }
            }
            if (triggered && eventData.type != EventType.Used)
            {
                eventData.Use();
            }
            return triggered;
        }

        private bool IsValidEvent(EventType eventType)
        {
            return eventType != EventType.Repaint &&
                eventType != EventType.Used &&
                eventType != EventType.Ignore;
        }

        private bool IsMouseEvent(EventType eventType)
        {
            return eventType == EventType.MouseDown ||
                eventType == EventType.MouseUp ||
                eventType == EventType.MouseMove ||
                eventType == EventType.MouseDrag ||
                eventType == EventType.ScrollWheel ||
                eventType == EventType.ContextClick ||
                eventType == EventType.DragPerform ||
                eventType == EventType.DragUpdated;
        }

        private bool IsKeyboardEvent(EventType eventType)
        {
            return eventType == EventType.KeyDown ||
                eventType == EventType.KeyUp;
        }

        private bool OnDragStart(Event eventData, Node trigger)
        {
            var triggered = false;
            if (mMouseDown == trigger && mDrag == null)
            {
                mDrag = trigger;
                var handler = mDrag.GetLeaf<IBeginDragHandler>(false);
                if (handler != null)
                {
                    handler.OnBeginDrag(eventData);
                    triggered = true;
                }
            }
            return triggered;
        }

        private bool OnDrag(Event eventData)
        {
            var triggered = false;
            if (mDrag != null)
            {
                var handler = mDrag.GetLeaf<IDragHandler>(false);
                if (handler != null)
                {
                    handler.OnDrag(eventData);
                    triggered = true;
                }
            }
            return triggered;
        }

        private bool OnDragStop(Event eventData)
        {
            bool triggered = false;
            if (mDrag != null)
            {
                var handler = mDrag.GetLeaf<IEndDragHandler>(false);
                if (handler != null)
                {
                    handler.OnEndDrag(eventData);
                    triggered = true;
                }
                mDrag = null;
            }
            return triggered;
        }

        private bool OnClickStart(Event eventData, Node trigger)
        {
            mMouseDown = trigger;
            var triggered = false;
            var handler = mMouseDown.GetLeaf<IMouseDownHandler>(false);
            if (handler != null)
            {
                handler.OnMouseDown(eventData);
                triggered = true;
            }
            return triggered;
        }

        private bool OnClickStop(Event eventData, Node trigger)
        {
            var triggered = false;
            var handler = trigger.GetLeaf<IMouseUpHandler>(false);
            if (handler != null)
            {
                handler.OnMouseUp(eventData);
                triggered = true;
            }
            if (mMouseDown == trigger)
            {
                var handler1 = mMouseDown.GetLeaf<IMouseClickHandler>(false);
                if (handler1 != null)
                {
                    handler1.OnMouseClick(eventData);
                    triggered = true;
                }
            }
            mMouseDown = null;
            return triggered;
        }

        private bool OnMouseMove(Event eventData, Node trigger)
        {
            var triggered = false;
            var handler = trigger.GetLeaf<IMouseMoveHandler>(false);
            if (handler != null)
            {
                handler.OnMouseMove(eventData);
                triggered = true;
            }
            return triggered;
        }

        private bool OnScrollWheel(Event eventData, Node trigger)
        {
            var triggered = false;
            var handler = trigger.GetLeaf<IScrollWheelHandler>(false);
            if (handler != null)
            {
                handler.OnScrollWheel(eventData);
                triggered = true;
            }
            return triggered;
        }

        private bool OnKeyDown(Event eventData, Node trigger)
        {
            var triggered = false;
            var handler = trigger.GetLeaf<IKeyDownHandler>(false);
            if (handler != null)
            {
                handler.OnKeyDown(eventData);
                triggered = true;
            }
            return triggered;
        }

        private bool OnKeyUp(Event eventData, Node trigger)
        {
            var triggered = false;
            var handler = trigger.GetLeaf<IKeyUpHandler>(false);
            if (handler != null)
            {
                handler.OnKeyUp(eventData);
                triggered = true;
            }
            return triggered;
        }
    }
}