using UnityEngine;

namespace EGUI.UI
{
    public interface IEndDragHandler : IEventSystemHandler
    {
        void OnEndDrag(Event eventData);
    }
}
