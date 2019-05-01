using UnityEngine;

namespace EGUI.UI
{
    public interface IMouseDownHandler : IEventSystemHandler
    {
        bool OnMouseDown(Event eventData);
    }
}
