using UnityEngine;

namespace EGUI.UI
{
    public interface IBeginDragHandler : IEventSystemHandler
    {
        void OnBeginDrag(Event eventData);
    }
}
