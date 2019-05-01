using UnityEngine;

namespace EGUI.UI
{
    public interface IKeyDownHandler : IEventSystemHandler
    {
        bool OnKeyDown(Event eventData);
    }
}
