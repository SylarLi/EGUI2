using EGUI.UI;
using UnityEditor;

namespace EGUI.Editor
{
    [UserDrawer(typeof(Drawer))]
    internal class DrawerUserDrawer : UserDrawer
    {
        protected override bool enableDisplayed { get { return false; } }

        protected override void OnGUI()
        {
            EditorGUILayout.Space();
        }
    }
}
