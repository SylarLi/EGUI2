using UnityEditor;
using UnityEngine;

namespace EGUI.UI
{
    public sealed class DefaultControl
    {
        private static readonly Color DefaultTextColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        private static readonly Color DefaultPressedTintColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        private static readonly Color DefaultDisabledTintColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

        public static Node CreateNode(Node parent)
        {
            var node = new Node {name = "Node", parent = parent, size = new Vector2(100, 100)};
            return node;
        }

        public static Image CreateImage(Node parent)
        {
            var node = new Node {name = "Image", parent = parent, size = new Vector2(100, 100)};
            node.AddLeaf<Drawer>();
            var image = node.AddLeaf<Image>();
            return image;
        }

        public static Text CreateText(Node parent)
        {
            var node = new Node {name = "Text", parent = parent, size = new Vector2(100, 16)};
            node.AddLeaf<Drawer>();
            var text = node.AddLeaf<Text>();
            text.text = "Text";
            text.color = DefaultTextColor;
            return text;
        }

        public static Button CreateButton(Node parent, Selectable.Transition transition = Selectable.Transition.SpriteSwap)
        {
            var image = CreateImage(parent);
            var node = image.node;
            node.name = "Button";
            node.size = new Vector2(60, 25);
            var button = node.AddLeaf<Button>();
            button.targetGraphic = image;
            button.transition = transition;
            button.colorState = new Selectable.ColorState()
            {
                normalColor = Color.white,
                focusedColor = Color.white,
                pressedColor = DefaultPressedTintColor,
                disabledColor = DefaultDisabledTintColor,
            };
            var normalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultResource.DefaultButtonSprite);
            var pressedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultResource.DefaultButtonActiveSprite);
            button.spriteState = new Selectable.SpriteState()
            {
                pressedSprite = pressedSprite
            };
            image.sprite = normalSprite;
            var text = CreateText(button.node);
            text.text = "Button";
            text.alignment = TextAnchor.MiddleCenter;
            text.node.anchorMin = Vector2.zero;
            text.node.anchorMax = Vector2.one;
            text.node.offsetMin = Vector2.zero;
            text.node.offsetMax = new Vector2(0, 3);
            return button;
        }

        public static Toggle CreateToggle(Node parent, Selectable.Transition transition = Selectable.Transition.SpriteSwap)
        {
            var node = new Node {name = "Toggle", parent = parent, size = new Vector2(100, 16)};
            var toggle = node.AddLeaf<Toggle>();
            toggle.transition = transition; 
            toggle.colorState = new Selectable.ColorState()
            {
                normalColor = Color.white,
                focusedColor = Color.white,
                pressedColor = DefaultPressedTintColor,
                disabledColor = DefaultDisabledTintColor,
            };
            var normalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultResource.DefaultToggleSprite);
            var pressedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultResource.DefaultToggleActiveSprite);
            var bg = CreateImage(node);
            bg.node.name = "Background";
            bg.sprite = normalSprite;
            bg.SetNativeSize();
            toggle.targetGraphic = bg;
            toggle.spriteState = new Selectable.SpriteState()
            {
                pressedSprite = pressedSprite
            };
            var mark = CreateImage(bg.node);
            mark.node.name = "Checkmark";
            mark.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultResource.DefaultToggleCheckmarkSprite);
            mark.SetNativeSize();
            toggle.toggleGraphic = mark;
            var text = CreateText(node);
            text.text = "Toggle";
            text.alignment = TextAnchor.UpperLeft;
            text.node.anchorMin = Vector2.zero;
            text.node.anchorMax = Vector2.one;
            text.node.offsetMin = new Vector2(bg.node.size.x, 0);
            return toggle; 
        }

        public static TextField CreateTextField(Node parent)
        {
            var node = new Node {name = "Textfield", parent = parent, size = new Vector2(100, 16)};
            node.AddLeaf<Drawer>();
            var textfield = node.AddLeaf<TextField>();
            textfield.text = "Textfield";
            textfield.color = DefaultTextColor;
            return textfield;
        }

        public static Scrollbar CreateScrollbar(Node parent)
        {
            var node = new Node {name = "Scrollbar", parent = parent, size = new Vector2(160, 16)};
            node.AddLeaf<Drawer>();
            var background = node.AddLeaf<Image>();
            background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultResource.DefaultScrollbarBGSprite);
            var slidingAreaNode = CreateNode(node);
            slidingAreaNode.name = "Sliding Area";
            slidingAreaNode.anchorMin = Vector2.zero;
            slidingAreaNode.anchorMax = Vector2.one;
            slidingAreaNode.offsetMin = Vector2.zero;
            slidingAreaNode.offsetMax = Vector2.zero;
            var handleImage = CreateImage(slidingAreaNode);
            handleImage.node.name = "Handle";
            handleImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DefaultResource.DefaultScrollbarThumbSprite);
            handleImage.SetNativeSize();
            var scrollbar = node.AddLeaf<Scrollbar>();
            scrollbar.handleRect = handleImage.node;
            scrollbar.targetGraphic = handleImage;
            scrollbar.size = 0.2f;
            scrollbar.direction = Scrollbar.Direction.LeftToRight;
            scrollbar.handleRect.offsetMin = Vector2.zero;
            scrollbar.handleRect.offsetMax = Vector2.zero;
            return scrollbar;
        }
    }
}
