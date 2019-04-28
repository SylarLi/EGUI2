using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    public class Text : MaskableGraphic
    {
        [PersistentField] private string mText = "text";

        public string text
        {
            get { return mText; }
            set { mText = value; }
        }

        [PersistentField] private Font mFont;

        public Font font
        {
            get { return mFont; }
            set
            {
                if (mFont != value)
                {
                    mFont = value;
                    SetStyleDirty();
                }
            }
        }

        [PersistentField] private FontStyle mFontStyle = FontStyle.Normal;

        public FontStyle fontStyle
        {
            get { return mFontStyle; }
            set
            {
                if (mFontStyle != value)
                {
                    mFontStyle = value;
                    SetStyleDirty();
                }
            }
        }

        [PersistentField] private int mFontSize = 11;

        public int fontSize
        {
            get { return mFontSize; }
            set
            {
                if (mFontSize != value)
                {
                    mFontSize = value;
                    SetStyleDirty();
                }
            }
        }

        [PersistentField] private bool mRichText = true;

        public bool richText
        {
            get { return mRichText; }
            set
            {
                if (mRichText != value)
                {
                    mRichText = value;
                    SetStyleDirty();
                }
            }
        }

        [PersistentField] private TextAnchor mAlignment = TextAnchor.UpperLeft;

        public TextAnchor alignment
        {
            get { return mAlignment; }
            set
            {
                if (mAlignment != value)
                {
                    mAlignment = value;
                    SetStyleDirty();
                }
            }
        }

        [PersistentField] private TextClipping mClipping = TextClipping.Clip;

        public TextClipping clipping
        {
            get { return mClipping; }
            set
            {
                if (mClipping != value)
                {
                    mClipping = value;
                    SetStyleDirty();
                }
            }
        }

        [PersistentField] private bool mWordWrap = false;

        public bool wordWrap
        {
            get { return mWordWrap; }
            set
            {
                if (mWordWrap != value)
                {
                    mWordWrap = value;
                    SetStyleDirty();
                }
            }
        }

        public override void RebuildStyle()
        {
            mStyle = new GUIStyle
            {
                font = font != null ? font : UnityEditor.EditorGUIUtility.Load("Lucida Grande") as Font,
                fontStyle = fontStyle,
                fontSize = fontSize,
                richText = richText,
                alignment = alignment,
                clipping = clipping,
                wordWrap = wordWrap,
                normal = new GUIStyleState {textColor = color}
            };
        }

        protected override void OnDraw()
        {
            DrawContent(new GUIContent(text));
        }

        public override Vector2 GetContentSize()
        {
            return style.CalcSize(new GUIContent(text));
        }
    }
}