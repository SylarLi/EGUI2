using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using EGUI.UI;

namespace EGUI.Editor
{
    internal sealed class UserMenu
    {
        public static void ShowNodeContext(Node root)
        {
            var menu = new GenericMenu();
            var nodes = UserSelection.nodes;
            if (nodes != null && nodes.Length > 0)
            {
                menu.AddItem(new GUIContent(Language.L_Copy), false, () =>
                {
                    UserUtil.CopyNodes(root);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(Language.L_Copy));
            }
            if (UserClipBoard.data != null && 
                UserClipBoard.data is Node[] && 
                (UserClipBoard.data as Node[]).Length > 0)
            {
                menu.AddItem(new GUIContent(Language.L_Paste), false, () =>
                {
                    UserUtil.PasteNodes(root);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(Language.L_Paste));
            }
            menu.AddSeparator("");
            if (nodes != null && nodes.Length > 0)
            {
                menu.AddItem(new GUIContent(Language.L_Duplicate), false, () =>
                {
                    UserUtil.DuplicateNodes(root);
                });
                menu.AddItem(new GUIContent(Language.L_Delete), false, () =>
                {
                    UserUtil.DeleteNodes(root);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent(Language.L_Duplicate));
                menu.AddDisabledItem(new GUIContent(Language.L_Delete));
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent(Language.L_CreateEmpty), false, () =>
            {
                UserUtil.CreateControl(typeof(Node), root);
            });
            var uiTypes = new Type[] { typeof(Text), typeof(Image), typeof(Button), typeof(Toggle), typeof(TextField) };
            foreach (var uiType in uiTypes)
            {
                menu.AddItem(new GUIContent(Language.L_UI + "/" + uiType.Name), false, () =>
                {
                    UserUtil.CreateControl(uiType, root);
                });
            }
            menu.ShowAsContext();
        }
    }
}
