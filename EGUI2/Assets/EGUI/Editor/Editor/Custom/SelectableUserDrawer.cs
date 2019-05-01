using EGUI.UI;
using UnityEditor;

namespace EGUI.Editor
{
    public class SelectableUserDrawer : UserDrawer
    {
        protected override void OnGUI()
        {
            PersistentGUILayout.PropertyField(target.Find("interactive"));
            var transition = target.Find("transition");
            PersistentGUILayout.PropertyField(transition);
            if (!transition.hasMultipleDifferentValues)
            {
                EditorGUI.indentLevel += 1;
                PersistentGUI.BeginLabelWidth(120f);
                PersistentGUILayout.PropertyField(target.Find("targetGraphic"));
                switch (transition.GetValue<Selectable.Transition>())
                {
                    case Selectable.Transition.ColorTint:
                        var colorState = target.Find("colorState");
                        PersistentGUILayout.PropertyField(colorState.Find("normalColor"));
                        PersistentGUILayout.PropertyField(colorState.Find("focusedColor"));
                        PersistentGUILayout.PropertyField(colorState.Find("pressedColor"));
                        PersistentGUILayout.PropertyField(colorState.Find("disabledColor"));
                        break;
                    case Selectable.Transition.SpriteSwap:
                        var spriteState = target.Find("spriteState");
                        PersistentGUILayout.PropertyField(spriteState.Find("focusedSprite"));
                        PersistentGUILayout.PropertyField(spriteState.Find("pressedSprite"));
                        PersistentGUILayout.PropertyField(spriteState.Find("disabledSprite"));
                        break;
                }
                PersistentGUI.EndLabelWidth();
                EditorGUI.indentLevel -= 1;
            }
            
        }
    }
}