using UnityEngine;

namespace EGUI.UI
{
    public interface IMouseUpHandler : IEventSystemHandler
    {
        bool OnMouseUp(Event eventData);
    }
}
