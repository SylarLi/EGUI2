using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace EGUI.Editor
{
    public sealed class EditorUtil
    {
        public static string GetNiceDisplayName(string name)
        {
            if (name.Length <= 2)
                return name;
            name = Regex.Replace(name, @"_(\w)", match => match.Groups[1].Value.ToUpper());
            name = Regex.Replace(name, @"^m([A-Z])", "$1");
            name = name.Substring(0, 1).ToUpper() + 
                Regex.Replace(name.Substring(1), @"[A-Z]", " $0");
            return name;
        }
    }
}
