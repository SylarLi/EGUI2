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

                foreach (var drawer in mDrawers)
                {
                    if (drawer != null && drawer.active)
                        drawer.Draw();
                }
            }
        }

        public void RebuildDrawingList()
        {
            mDrawers.Clear();
            var drawer = GetLeaf<Drawer>();
            if (drawer != null)
                mDrawers.Add(drawer);
            node.GetLeavesInChildren(mDrawers);
        }
    }
}