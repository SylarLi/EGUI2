using UnityEngine;

namespace EGUI.Editor
{
    public abstract class PropertyDrawer
    {
        public virtual void OnGUI(Rect position, PersistentProperty property, GUIContent label)
        {
            
        }

        public virtual float GetPropertyHeight(PersistentProperty property, GUIContent label)
        {
            return 0;
        }
    }
}
