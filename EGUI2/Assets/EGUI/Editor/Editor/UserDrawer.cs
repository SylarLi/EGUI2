using UnityEngine;
using UnityEditor;
using System.Linq;

namespace EGUI.Editor
{
    public class UserDrawer
    {
        private PersistentObject mTarget;

        public PersistentObject target { get { return mTarget; } set { mTarget = value; } }

        private bool mFoldout = true;

        protected virtual bool enableDisplayed { get { return true; } }

        public void OnDraw()
        {
            OnTitleGUI();
            if (mFoldout)
                OnGUI();
        }

        protected virtual void OnTitleGUI()
        {
            var title = new GUIContent(target.type.Name);
            var rect = EditorGUILayout.GetControlRect(false, EditorStyles.toolbarButton.fixedHeight);
            var fsize = EditorStyles.foldout.CalcSize(GUIContent.none);
            var frect = new Rect(rect.x + 5, rect.y + (rect.height - fsize.y) * 0.5f, fsize.x, fsize.y);
            var tsize = EditorStyles.toggle.CalcSize(GUIContent.none);
            var trect = new Rect(frect.xMax, rect.y + (rect.height - tsize.y) * 0.5f - 1, tsize.x, tsize.y);
            var lheight = EditorStyles.boldLabel.CalcHeight(title, rect.width);
            var lrect = new Rect(trect.xMax, rect.y + (rect.height - lheight) * 0.5f, rect.width, lheight);
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = Event.current.GetTypeForControl(controlId);
            switch (eventType)
            {
                case EventType.Repaint:
                    {
                        EditorStyles.toolbarButton.Draw(rect, GUIContent.none, controlId, mFoldout);
                        EditorStyles.foldout.Draw(frect, GUIContent.none, controlId, mFoldout);
                        break;
                    }
                case EventType.MouseDown:
                    {
                        if (rect.Contains(Event.current.mousePosition) &&
                            !trect.Contains(Event.current.mousePosition))
                        {
                            if (Event.current.button == 0)
                            {
                                mFoldout = !mFoldout;
                                Event.current.Use();
                            }
                            else if (Event.current.button == 1)
                            {
                                var menu = new GenericMenu();
                                menu.AddItem(new GUIContent(Locale.L_RemoveLeaf), false, () =>
                                {
                                    var leaves = target.GetValues<Leaf>();
                                    UserUtil.RemoveLeaf(leaves);
                                });
                                menu.ShowAsContext();
                                Event.current.Use();
                            }
                        }
                        break;
                    }
            }
            if (enableDisplayed)
                PersistentGUI.PropertyField(trect, GUIContent.none, target.Find("enabled"), false);
            EditorGUI.LabelField(lrect, title, EditorStyles.boldLabel);
        }

        protected virtual void OnGUI()
        {
            var children = target.ListChildren();
            EditorGUI.indentLevel += 1;
            foreach (var child in children)
            {
                var childLabel = new GUIContent(child.displayName);
                PersistentGUILayout.PropertyField(childLabel, child);
            }
            EditorGUI.indentLevel -= 1;
        }
    }
}
