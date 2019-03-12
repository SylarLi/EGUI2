using UnityEngine;

namespace EGUI.UI
{
    [Persistence]
    public class Image : Graphic
    {
        [PersistentField]
        private Sprite mSprite;

        public Sprite sprite { get { return mSprite; } set { if (mSprite != value) { mSprite = value; SetStyleDirty(); } } }

        [PersistentField]
        private Sprite mOverrideSprite;

        public Sprite overrideSprite { get { return mOverrideSprite; } set { if (mOverrideSprite != value) { mOverrideSprite = value; SetStyleDirty(); } } }

        public Sprite activeSprite { get { return overrideSprite != null ? overrideSprite : sprite; } }

        public override void RebuildStyle()
        {
            mStyle = new GUIStyle();
            RectOffset border;
            var normalSprite = activeSprite ?? DefaultResource.GetBlankSprite();
            mStyle.normal = UIUtility.BuildStyleState(normalSprite, out border);
            mStyle.border = border;
        }

        public void SetNativeSize()
        {
            if (sprite != null)
            {
                node.stretchWidth = false;
                node.stretchHeight = false;
                node.size = new Vector2(sprite.rect.width, sprite.rect.height);
            }
        }

        public override Vector2 GetContentSize()
        {
            return activeSprite != null ?
                new Vector2(activeSprite.rect.width, activeSprite.rect.height) :
                base.GetContentSize();  
        }
    }
}