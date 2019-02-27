﻿using UnityEngine;

namespace EGUI.UI
{
    public sealed class DefaultControl
    {
        private static Color DefaultTextColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        private static Color DefaultPressedTintColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        private static Color DefaultDisabledTintColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);

        public static Image CreateImage(Node parent)
        {
            var node = new Node();
            node.name = "image";
            node.parent = parent;
            node.size = new Vector2(100, 100);
            node.AddLeaf<Drawer>();
            var image = node.AddLeaf<Image>();
            return image;
        }

        public static Text CreateText(Node parent)
        {
            var node = new Node();
            node.name = "text";
            node.parent = parent;
            node.size = new Vector2(100, 16);
            node.AddLeaf<Drawer>();
            var text = node.AddLeaf<Text>();
            text.text = "text";
            text.color = DefaultTextColor;
            return text;
        }

        public static Button CreateButton(Node parent, Selectable.Transition transition = Selectable.Transition.SpriteSwap)
        {
            var image = CreateImage(parent);
            var node = image.node;
            node.name = "button";
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
            var style = GUI.skin.button;
            var border = new Vector4(style.border.left, style.border.bottom, style.border.right, style.border.top);
            var normalState = style.normal;
            var normalSprite = CreateSprite(normalState.background, border);
            var pressedState = style.active;
            var pressedSprite = CreateSprite(pressedState.background, border);
            button.spriteState = new Selectable.SpriteState()
            {
                pressedSprite = pressedSprite
            };
            image.sprite = normalSprite;
            var text = CreateText(button.node);
            text.text = "button";
            text.alignment = TextAnchor.MiddleCenter;
            text.node.stretchWidth = true;
            text.node.stretchHeight = true;
            text.node.stretchSize = Vector2.one;
            text.node.localPosition = new Vector2(0, -2);
            return button;
        }

        public static Toggle CreateToggle(Node parent, Selectable.Transition transition = Selectable.Transition.SpriteSwap)
        {
            var node = new Node();
            node.name = "toggle";
            node.parent = parent;
            node.size = new Vector2(100, 16);
            var toggle = node.AddLeaf<Toggle>();
            toggle.transition = transition;
            toggle.colorState = new Selectable.ColorState()
            {
                normalColor = Color.white,
                focusedColor = Color.white,
                pressedColor = DefaultPressedTintColor,
                disabledColor = DefaultDisabledTintColor,
            };
            var style = GUI.skin.toggle;
            var border = new Vector4(style.border.left, style.border.bottom, style.border.right, style.border.top);
            var normalState = style.normal;
            var normalSprite = CreateSprite(normalState.background, border);
            var pressedState = style.active;
            var pressedSprite = CreateSprite(pressedState.background, border);
            var bg = CreateImage(node);
            bg.node.name = "background";
            bg.sprite = normalSprite;
            Debug.Log(normalSprite.rect.width + " : " + normalSprite.rect.height);
            bg.SetNativeSize();
            toggle.targetGraphic = bg;
            toggle.spriteState = new Selectable.SpriteState()
            {
                pressedSprite = pressedSprite
            };
            var mark = CreateImage(bg.node);
            mark.node.name = "checkmark";
            mark.sprite = DefaultResource.GetToggleCheckmarkSprite();
            mark.SetNativeSize();
            toggle.toggleGraphic = mark;
            var text = CreateText(node);
            text.text = "toggle";
            text.alignment = TextAnchor.UpperLeft;
            text.node.stretchWidth = true;
            text.node.stretchHeight = true;
            text.node.stretchSize = Vector2.one;
            text.node.padding = new Vector4(bg.node.size.x, 0, 0, 0);
            return toggle; 
        }

        public static TextField CreateTextField(Node parent)
        {
            var node = new Node();
            node.name = "textfield";
            node.parent = parent;
            node.size = new Vector2(100, 16);
            node.AddLeaf<Drawer>();
            var textfield = node.AddLeaf<TextField>();
            textfield.text = "textfield";
            textfield.color = DefaultTextColor;
            return textfield;
        }

        private static Sprite CreateSprite(Texture2D texture, Vector4 border)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight, border);
        }
    }
}