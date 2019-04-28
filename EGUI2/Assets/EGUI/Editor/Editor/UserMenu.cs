using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using EGUI.UI;
using UnityEngine.SocialPlatforms;

namespace EGUI.Editor
{
    internal sealed class UserMenu
    {
        public static void ShowNodeContext(Node root)
        {
            var menu = new GenericMenu();
            var nodes = UserDatabase.selection.nodes;
            if (nodes != null && nodes.Length > 0)
            {
                menu.AddItem(new GUIContent(Locale.L_Copy), false, () =>
                {
                    UserUtil.CopyNodes(root);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(Locale.L_Copy));
            }
            if (UserClipBoard.data != null && 
                UserClipBoard.data is Node[] && 
                ((Node[]) UserClipBoard.data).Length > 0)
            {
                menu.AddItem(new GUIContent(Locale.L_Paste), false, () =>
                {
                    UserUtil.PasteNodes(root);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(Locale.L_Paste));
            }
            menu.AddSeparator("");
            if (nodes != null && nodes.Length > 0)
            {
                menu.AddItem(new GUIContent(Locale.L_Duplicate), false, () =>
                {
                    UserUtil.DuplicateNodes(root);
                });
                menu.AddItem(new GUIContent(Locale.L_Delete), false, UserUtil.DeleteNodes);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(Locale.L_Duplicate));
                menu.AddDisabledItem(new GUIContent(Locale.L_Delete));
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent(Locale.L_CreateEmpty), false, () =>
            {
                UserUtil.CreateControl(typeof(Node), root);
            });
            var uiTypes = new Type[] { typeof(Text), typeof(Image), typeof(Button), typeof(Toggle), typeof(TextField), typeof(Scrollbar) };
            foreach (var type in uiTypes)
            {
                var uiType = type;
                menu.AddItem(new GUIContent(Locale.L_UI + "/" + uiType.Name), false, () =>
                {
                    UserUtil.CreateControl(uiType, root);
                });
            }
            menu.ShowAsContext();
        }
    }
}
