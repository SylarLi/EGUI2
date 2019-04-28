using System.Collections.Generic;
using UnityEngine;

namespace EGUI
{
    public class Raycaster
    {
        public static Node[] RaycastAll(Vector2 position, Node node)
        {
            var list = new List<Node>();
            for (int i = node.childCount - 1; i >= 0; i--)
            {
                var ret = RaycastAll(position, node.GetChild(i));
                list.AddRange(ret);
            }
            var pos = node.world2LocalMatrix.MultiplyPoint(position);
            if (node.parent != null && node.localRect.Contains(pos))
            {
                list.Add(node);
            }

            return list.ToArray();
        }
    }
}
