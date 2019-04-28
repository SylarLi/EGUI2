using UnityEngine;

namespace EGUI.UI
{
    public interface IDragHandler : IEventSystemHandler
    {
        void OnDrag(Event eventData);
    }
}
