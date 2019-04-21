using UnityEngine;

namespace EGUI.Editor
{
    public abstract class UserPropertyDrawer
    {
        public virtual void OnGUI(Rect position, PersistentProperty property, GUIContent label)
        {
            
        }

        public virtual float GetHeight(PersistentProperty property, GUIContent label)
        {
            return 0;
        }
    }
}
