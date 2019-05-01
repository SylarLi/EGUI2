using UnityEngine;

namespace EGUI.UI
{
    public interface IScrollWheelHandler : IEventSystemHandler
    {
        bool OnScrollWheel(Event eventData);
    }
}
