using UnityEngine;

namespace EGUI.UI
{
    public interface IBeginDragHandler : IEventSystemHandler
    {
        bool OnBeginDrag(Event eventData);
    }
}
