using System.Reflection;
using UnityEngine;

namespace EGUI
{
    internal sealed class GUIProxy
    {
        private static FieldInfo guiSkinFieldInfo;

        internal static GUISkin skin
        {
            get
            {
                guiSkinFieldInfo = guiSkinFieldInfo ?? typeof(GUI).GetField("s_Skin", BindingFlags.Static | BindingFlags.NonPublic);
                return guiSkinFieldInfo.GetValue(null) as GUISkin;
            }
        }
    }
}
