using UnityEngine;

namespace EGUI.UI
{
    public interface IEndDragHandler : IEventSystemHandler
    {
        bool OnEndDrag(Event eventData);
    }
}
