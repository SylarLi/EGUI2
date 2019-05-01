using UnityEngine;

namespace EGUI.UI
{
    public interface IMouseMoveHandler : IEventSystemHandler
    {
        bool OnMouseMove(Event eventData);
    }
}
