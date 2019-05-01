using UnityEngine;

namespace EGUI.UI
{
    public interface IKeyUpHandler : IEventSystemHandler
    {
        bool OnKeyUp(Event eventData);
    }
}
