using UnityEngine;

namespace EGUI.UI
{
    public interface IDragHandler : IEventSystemHandler
    {
        bool OnDrag(Event eventData);
    }
}
