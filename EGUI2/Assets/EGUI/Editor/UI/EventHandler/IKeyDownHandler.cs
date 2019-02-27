using UnityEngine;

namespace EGUI.UI
{
    public interface IKeyDownHandler : IEventSystemHandler
    {
        void OnKeyDown(Event eventData);
    }
}
