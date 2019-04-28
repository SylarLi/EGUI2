using UnityEngine;

namespace EGUI.UI
{
    public interface IMouseClickHandler : IEventSystemHandler
    {
        void OnMouseClick(Event eventData);
    }
}
