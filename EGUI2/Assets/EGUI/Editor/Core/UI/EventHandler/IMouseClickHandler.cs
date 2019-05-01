using UnityEngine;

namespace EGUI.UI
{
    public interface IMouseClickHandler : IEventSystemHandler
    {
        bool OnMouseClick(Event eventData);
    }
}
