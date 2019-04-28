using UnityEngine;

namespace EGUI.UI
{
    public interface IMouseMoveHandler : IEventSystemHandler
    {
        void OnMouseMove(Event eventData);
    }
}
