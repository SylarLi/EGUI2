using UnityEngine;

namespace EGUI.UI
{
    public interface IMouseDownHandler : IEventSystemHandler
    {
        void OnMouseDown(Event eventData);
    }
}
