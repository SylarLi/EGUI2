using UnityEngine;

namespace EGUI.Editor
{
    [Persistence]
    public abstract class PropertyDrawer : Object
    {
        public virtual void OnGUI(Rect rect, PersistentProperty persistentProperty, GUIContent label)
        {
            
        }

        public virtual float GetPropertyHeight(PersistentProperty persistentProperty, GUIContent label)
        {
            return 0;
        }
    }
}
