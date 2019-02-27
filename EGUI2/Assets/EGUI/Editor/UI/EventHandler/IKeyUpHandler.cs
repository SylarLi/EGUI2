using UnityEngine;

namespace EGUI.UI
{
    public interface IKeyUpHandler : IEventSystemHandler
    {
        void OnKeyUp(Event eventData);
    }
}
