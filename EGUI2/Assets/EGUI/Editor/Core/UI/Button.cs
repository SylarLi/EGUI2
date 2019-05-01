using UnityEngine;

namespace EGUI.UI
{
    public class Button : Selectable, IMouseClickHandler
    {
        public delegate void OnClick();

        public OnClick onClick = () => { };

        public bool OnMouseClick(Event eventData)
        {
            onClick();
            return true;
        }
    }
}
