﻿using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Sprite = UnityEngine.Sprite;

namespace EGUI
{
    public class SpritePersistence : CustomPersistence
    {
        public SpritePersistence(Persistence persistence) : base(persistence)
        {
        }

        public override Type persistentType
        {
            get { return typeof(Sprite); }
        }

        public override void Parse(object value, BinaryWriter writer)
        {
            var sprite = value as Sprite;
            var assetPath = AssetDatabase.GetAssetPath(sprite);
            if (!string.IsNullOrEmpty(assetPath))
            {
                writer.Write((byte) 0);
                SerializeUnityAsset(value, typeof(Sprite), writer);
            }
            else
            {
                writer.Write((byte) 1);
                Serialize(sprite.texture, typeof(Texture2D), writer);
                Serialize(sprite.rect, typeof(Rect), writer);
                Serialize(sprite.pivot, typeof(Vector2), writer);
                writer.Write(sprite.pixelsPerUnit);
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

            if (type == 1)
            {
                Texture2D texture = null;
                var rect = default(Rect);
                var pivot = default(Vector2);
                var pixelsPerUnit = 100f;
                var border = default(Vector4);
                Deserialize(reader, ret => texture = (Texture2D) ret);
                Deserialize(reader, ret => rect = (Rect) ret);
                Deserialize(reader, ret => pivot = (Vector2) ret);
                pixelsPerUnit = reader.ReadSingle();
                Deserialize(reader, ret => border = (Vector4) ret);
                return Sprite.Create(texture, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.Tight, border);
            }

            throw new NotSupportedException("Invalid type: " + type);
        }
    }
}