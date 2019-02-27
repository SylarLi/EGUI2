using UnityEngine;

namespace EGUI.UI
{
    public interface ILegacyEventHandler : IEventSystemHandler
    {
        void OnEvent(Event eventData);
    }
}
