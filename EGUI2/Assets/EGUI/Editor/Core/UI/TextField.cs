using System;
using System.Reflection;
using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    public class TextField : Text, ISelectable, IInteractive, ILegacyEventHandler
    {
        private static readonly Color TextSelectionColor = new Color(0.2392157f, 0.5019608f, 0.8745099f, 0.65f);
        private static readonly Color TextCursorColor = new Color(0.7058824f, 0.7058824f, 0.7058824f, 1f);

        [PersistentField] private Color mSelectionColor = TextSelectionColor;

        public Color selectionColor
        {
            get { return mSelectionColor; }
            set { mSelectionColor = value; }
        }

        [PersistentField] private Color mCursorColor = TextCursorColor;

        public Color cursorColor
        {
            get { return mCursorColor; }
            set { mCursorColor = value; }
        }

        [PersistentField] private Color mDisabledColor = Color.grey;

        public Color disabledColor
        {
            get { return mDisabledColor; }
            set { mDisabledColor = value; }
        }

        [PersistentField] private bool mMultiline;

        public bool multiline
        {
            get { return mMultiline; }
            set { mMultiline = value; }
        }

        [PersistentField] private bool mPassword;

        public bool password
        {
            get { return mPassword; }
            set { mPassword = value; }
        }

        [PersistentField] private bool mFocused;

        public bool focused
        {
            get { return mFocused; }
            set
            {
                if (mFocused != value)
                {
                    mFocused = value;
                    if (!mFocused) OnDeselect();
                }
            }
        }

        [PersistentField] private bool mInteractive = true;

        public bool interactive
        {
            get { return mInteractive; }
            set { mInteractive = value; }
        }

        public delegate void OnInputValueChanged(string value);

        public OnInputValueChanged onInputValueChanged = value => { };

        private static FieldInfo mRefTextEditor;

        private static MethodInfo mRefIsEditing;

        private static MethodInfo mRefTextField;

        public override void RebuildStyle()
        {
            mStyle = new GUIStyle(GUI.skin.textField);
            mStyle.font = font != null ? font : UnityEditor.EditorGUIUtility.Load("Lucida Grande") as Font;
            mStyle.fontStyle = fontStyle;
            mStyle.fontSize = fontSize;
            mStyle.richText = richText;
            mStyle.alignment = alignment;
            mStyle.clipping = clipping;
            mStyle.wordWrap = wordWrap;
            mStyle.normal.textColor = color;
            mStyle.focused.textColor = color;
        }

        protected override void OnDraw()
        {
            PrepareProxyField();
            DrawProcess(DrawTextField);
        }

        private void DrawTextField()
        {
            var rawColor = GUI.color;
            var rawSelectionColor = GUI.skin.settings.selectionColor;
            var rawCursorColor = GUI.skin.settings.cursorColor;
            if (!interactive) GUI.color *= disabledColor;
            GUI.skin.settings.selectionColor *= selectionColor;
            GUI.skin.settings.cursorColor *= cursorColor;
            var controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            var parameters = new[]
            {
                mRefTextEditor.GetValue(null),
                controlID,
                drawer.guiRect,
                text,
                style,
                null,
                false,
                false,
                multiline,
                password,
            };
            mRefTextField.Invoke(null, parameters);
            GUI.color = rawColor;
            GUI.skin.settings.selectionColor = rawSelectionColor;
            GUI.skin.settings.cursorColor = rawCursorColor;
        }

        public void OnEvent(Event eventData)
        {
            PrepareProxyField();
            var controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            var recycledEditor = mRefTextEditor.GetValue(null);
            var isEditingBefore = (bool) mRefIsEditing.Invoke(recycledEditor, new object[] {controlID});
            var parameters = new[]
            {
                recycledEditor,
                controlID,
                node.localRect,
                text,
                style,
                null,
                false,
                false,
                multiline,
                password,
            };
            var newText = mRefTextField.Invoke(null, parameters);
            var isEditingAfter = (bool) mRefIsEditing.Invoke(recycledEditor, new object[] {controlID});
            if (!isEditingBefore && isEditingAfter)
            {
                FocusControl.currentSelectable = this;
            }

            if ((bool) parameters[6])
            {
                text = (string) newText;
                onInputValueChanged.Invoke(text);
                Command.Execute(new UpdateMemberCommand(this, "text", newText));
            }
        }

        private void OnDeselect()
        {
            var controlID = GUIUtility.GetControlID(FocusType.Keyboard);
            if (GUIUtility.keyboardControl == controlID)
            {
                GUIUtility.keyboardControl = 0;
            }
        }

        private void PrepareProxyField()
        {
            if (mRefTextEditor == null || mRefTextField == null)
            {
                var type = typeof(UnityEditor.EditorGUI);
                mRefTextEditor = type.GetField("s_RecycledEditor", BindingFlags.NonPublic | BindingFlags.Static);
                mRefIsEditing = mRefTextEditor.FieldType.GetMethod("IsEditingControl",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                mRefTextField = type.GetMethod("DoTextField", BindingFlags.NonPublic | BindingFlags.Static);
            }
        }
    }
}