using System.Linq;
using EGUI.UI;
using UnityEngine;

namespace EGUI.Editor
{
    [UserDrawer(typeof(Scrollbar))]
    internal class ScrollbarUserDrawer : SelectableUserDrawer
    {
        protected override void OnGUI()
        {
            base.OnGUI();
            PersistentGUILayout.PropertyField(target.Find("handleRect"));
            PersistentGUILayout.PropertyField(target.Find("direction"));
            PersistentGUILayout.FloatSlider(target.Find("value"), 0, 1);
            PersistentGUILayout.FloatSlider(target.Find("size"), 0, 1);
        }
    }
}