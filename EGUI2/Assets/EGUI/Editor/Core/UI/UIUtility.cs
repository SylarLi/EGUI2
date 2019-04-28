using UnityEngine;

namespace EGUI.UI
{
    public sealed class UIUtility
    {
        public static GUIStyleState BuildStyleState(Sprite sprite, out RectOffset border)
        {
            var state = new GUIStyleState();
            state.background = sprite.texture;
            var spb = sprite.border;
            border = new RectOffset(
                Mathf.FloorToInt(spb.x),
                Mathf.FloorToInt(spb.z),
                Mathf.FloorToInt(spb.w),
                Mathf.FloorToInt(spb.y));
            return state;
        }
    }
}
