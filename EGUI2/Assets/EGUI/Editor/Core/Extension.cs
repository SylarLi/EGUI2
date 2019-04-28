using UnityEngine;

namespace EGUI
{
    public static class Extension
    {
        public static bool Intersects(this Rect r1, Rect r2, out Rect area)
        {
            area = Rect.zero;
            if (r2.Overlaps(r1))
            {
                var x1 = Mathf.Min(r1.xMax, r2.xMax);
                var x2 = Mathf.Max(r1.xMin, r2.xMin);
                var y1 = Mathf.Min(r1.yMax, r2.yMax);
                var y2 = Mathf.Max(r1.yMin, r2.yMin);
                area.x = Mathf.Min(x1, x2);
                area.y = Mathf.Min(y1, y2);
                area.width = Mathf.Max(0f, x1 - x2);
                area.height = Mathf.Max(0f, y1 - y2);
                return true;
            }

            return false;
        }
    }
}