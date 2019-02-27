using UnityEngine;

namespace EGUI.UI
{
    public interface IMouseUpHandler : IEventSystemHandler
    {
        void OnMouseUp(Event eventData);
    }
}
