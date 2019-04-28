using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace EGUI
{
    public sealed class Cursor
    {
        public enum State
        {
            Default,
            DragAnything,
            Vertical,
            Horizontal,
            Diagonal1,
            Diagonal2,
        }

        private static State mState = State.Default;

        private static Texture2D mCursorTex;

        private static Vector2 mCursorHotpot;

        private static Matrix4x4 mMatrix = Matrix4x4.identity;

        public static Matrix4x4 matrix { get { return mMatrix; } set { mMatrix = value; } }

        public static State GetState()
        {
            return mState;
        }

        public static void SetState(State state)
        {
            if (mState != state)
            {
                mState = state;
                UpdateState();
            }
        }

        private static void UpdateState()
        {
            var state = GetState();
            switch (state)
            {
                case State.DragAnything:
                    {
                        mCursorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultResource.HelpCursorTex);
                        mCursorHotpot = new Vector2(0, 0);
                        break;
                    }
                case State.Vertical:
                    {
                        mCursorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultResource.VerticalCursorTex);
                        mCursorHotpot = new Vector2(4, 10);
                        break;
                    }
                case State.Horizontal:
                    {
                        mCursorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultResource.HorizontalCursorTex);
                        mCursorHotpot = new Vector2(10, 4);
                        break;
                    }
                case State.Diagonal1:
                    {
                        mCursorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultResource.Diagonal1CursorTex);
                        mCursorHotpot = new Vector2(8, 8);
                        break;
                    }
                case State.Diagonal2:
                    {
                        mCursorTex = AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultResource.Diagonal2CursorTex);
                        mCursorHotpot = new Vector2(8, 8);
                        break;
                    }
            }
            UnityEngine.Cursor.visible = state == State.Default || mCursorTex == null;
        }

        public static void ResetState()
        {
            matrix = Matrix4x4.identity;
            mState = State.Default;
            UpdateState();
        }

        public static void Update()
        {
            if (mCursorTex == null)
            {
                UnityEngine.Cursor.visible = true;
            }
            if (!UnityEngine.Cursor.visible)
            {
                var rawMatrix = GUI.matrix;
                GUI.matrix *= matrix;
                var mousePos = Event.current.mousePosition;
                var offset = mousePos - mCursorHotpot;
                GUI.matrix *= Matrix4x4.Translate(offset);
                GUI.DrawTexture(new Rect(Vector2.zero, new Vector2(mCursorTex.width, mCursorTex.height)), mCursorTex);
                GUI.matrix = rawMatrix;
            }
        }
    }
}
