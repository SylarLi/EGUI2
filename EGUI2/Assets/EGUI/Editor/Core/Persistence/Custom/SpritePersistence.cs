using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace EGUI
{
    public class SpritePersistence : CustomPersistence
    {
        public SpritePersistence(Persistence persistence) : base(persistence) { }

        public override Type persistentType { get { return typeof(Sprite); } }

        public override void Parse(object value, BinaryWriter writer)
        {
            var sprite = value as Sprite;
            var assetPath = AssetDatabase.GetAssetPath(sprite);
            if (!string.IsNullOrEmpty(assetPath))
            {
                writer.Write((byte)0);
                SerializeUnityAsset(value, typeof(Sprite), writer);
            }
            else
            {
                writer.Write((byte)1);
                Serialize(sprite.texture, typeof(Texture2D), writer);
                Serialize(sprite.rect, typeof(Rect), writer);
                Serialize(sprite.pivot, typeof(Vector2), writer);
                Serialize(sprite.pixelsPerUnit, typeof(int), writer);
                Serialize(sprite.border, typeof(Vector4), writer);
            }
        }

        public override object Revert(BinaryReader reader)
        {
            var type = reader.ReadByte();
            if (type == 0)
            {
                return DeserializeUnityAsset(typeof(Sprite), reader);
            }
            else if (type == 1)
            {
                Texture2D texture = null;
                Rect rect = default(Rect);
                Vector2 pivot = default(Vector2);
                int pixelsPerUnit = 100;
                Vector4 border = default(Vector4);
                Deserialize(reader, ret => texture = (Texture2D)ret);
                Deserialize(reader, ret => rect = (Rect)ret);
                Deserialize(reader, ret => pivot = (Vector2)ret);
                Deserialize(reader, ret => pixelsPerUnit = (int)ret);
                Deserialize(reader, ret => border = (Vector4)ret);
                return Sprite.Create(texture, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.Tight, border);
            }
            else
            {
                throw new NotSupportedException("Invalide type: " + type);
            }
        }
    }
}
