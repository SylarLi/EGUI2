using System.Linq;
using EGUI.UI;
using UnityEngine;

namespace EGUI.Editor
{
    [UserDrawer(typeof(Image))]
    internal class ImageUserDrawer : UserDrawer
    {
        protected override void OnGUI()
        {
            PersistentGUILayout.PropertyField(target.Find("sprite"));
            PersistentGUILayout.PropertyField(target.Find("color"));
            PersistentGUILayout.PropertyField(target.Find("raycastTarget"));
            var images = target.GetValues<Image>();
            if (images.Any(i => i.sprite != null))
            {
                if (GUILayout.Button("Set Native Size", GUILayout.MaxWidth(150)))
                {
                    var commands = images.Where(i =>
                            i.sprite != null &&
                            (i.node.size.x != i.sprite.rect.width || i.node.size.y != i.sprite.rect.height))
                        .Select(i =>
                            new UpdateMemberCommand(i.node, "size",
                                new Vector2(i.sprite.rect.width, i.sprite.rect.height)));
                    if (commands.Any())
                        Command.Execute(new CombinedCommand(commands.ToArray())); 
                }
            }   
        }
    }
}
