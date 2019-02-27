using UnityEngine;

namespace EGUI.UI
{
    public class Button : Selectable, IMouseClickHandler
    {
        public delegate void OnClick();

        public OnClick onClick = () => { };

        public void OnMouseClick(Event eventData)
        {
            onClick();
        }
    }
}
