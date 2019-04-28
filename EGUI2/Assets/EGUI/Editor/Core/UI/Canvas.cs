using System.Collections.Generic;
using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    public class Canvas : Leaf
    {
        private List<Drawer> mDrawers = new List<Drawer>();

        private bool mRebuildDrawingList = true;

        public void MarkRebuildDrawingList()
        {
            mRebuildDrawingList = true;
        }

        public override void Update()
        {
            var eventType = Event.current.type;
            if (eventType == EventType.Repaint)
            {
                if (mRebuildDrawingList)
                {
                    RebuildDrawingList();
                    mRebuildDrawingList = false;
                }

                for (var i = 0; i < mDrawers.Count; i++)
                {
                    var drawer = mDrawers[i];
                    if (drawer != null && drawer.active)
                    {
                        drawer.Draw();
                    }
                }
            }
        }

        public void RebuildDrawingList()
        {
            mDrawers.Clear();
            var derives = new Queue<Node>();
            derives.Enqueue(node);
            while (derives.Count > 0)
            {
                var current = derives.Dequeue();
                var drawer = current.GetLeaf<Drawer>();
                if (drawer != null)
                {
                    mDrawers.Add(drawer);
                }

                foreach (var child in current)
                {
                    derives.Enqueue(child);
                }
            }
        }
    }
}