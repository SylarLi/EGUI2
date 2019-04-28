using UnityEngine;

namespace EGUI.UI
{
    public interface IScrollWheelHandler : IEventSystemHandler
    {
        void OnScrollWheel(Event eventData);
    }
}
