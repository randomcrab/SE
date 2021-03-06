﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Vector2 = System.Numerics.Vector2;
using MonoGameVector3 = Microsoft.Xna.Framework.Vector3;

namespace SE.Core
{
    /// <summary>
    /// Static class which handles screen logic.
    /// </summary>
    public static class Screen
    {
        internal const int _BASE_RES_X = 1920;
        internal const int _BASE_RES_Y = 1080;
        internal static Matrix ScreenScaleMatrix;
        internal static float SizeRatio = 1.0f;

        /// <summary>Where the mouse pointer is located.</summary>
        private static Vector2 screenMousePoint = Vector2.Zero;

        private static GraphicsDeviceManager graphics;

        /// <value>Gets the mouse point in screen coordinates.
        ///        Returns null if the mouse isn't within the game window.</value>
        public static Vector2? MousePoint {
            get {
                if (MouseInWindow) {
                    return screenMousePoint;
                }
                return null;
            }
        }

        /// <summary>Where the viewport is located in the editor. Valid if running through the editor.</summary>
        public static Rectangle EditorViewBounds { get; set; }

        /// <summary>Viewport size.</summary>
        public static Vector2 ViewSize => new Vector2(ViewBounds.Width, ViewBounds.Height);

        /// <summary>Viewport bounds.</summary>
        internal static Rectangle ViewBounds { get; private set; } = Rectangle.Empty;

        /// <summary>True if the mouse is within the game window.</summary>
        public static bool MouseInWindow { get; private set; }

        public static DisplayMode DisplayMode { get; internal set; } = DisplayMode.Normal;

        public static bool IsFullHeadless => DisplayMode == DisplayMode.Decapitated;

        internal static void CalculateScreenScale()
        {
            ScreenScaleMatrix = Matrix.CreateScale(new MonoGameVector3(SizeRatio, SizeRatio, 1));
        }

        internal static void Update()
        {
            MouseState mouseState = InputManager.MouseState;

            // Calculate the mouse point.
            if (GameEngine.IsEditor) {
                Vector2 scale = new Vector2((float) EditorViewBounds.Width / _BASE_RES_X, (float) EditorViewBounds.Height / _BASE_RES_Y);
                Vector2 mousePos = new Vector2(
                    (mouseState.X / SizeRatio - EditorViewBounds.X) / scale.X,
                    (mouseState.Y / SizeRatio - EditorViewBounds.Y) / scale.Y);

                screenMousePoint.X = mousePos.X;
                screenMousePoint.Y = mousePos.Y;
            } else {
                screenMousePoint.X = mouseState.X / SizeRatio;
                screenMousePoint.Y = mouseState.Y / SizeRatio;
            }

            // Determine whether or not the mouse point is contained within the game window.
            if (screenMousePoint.X < 0 || screenMousePoint.X > _BASE_RES_X || screenMousePoint.Y < 0 || screenMousePoint.Y > _BASE_RES_Y)
                MouseInWindow = false;
            else
                MouseInWindow = true;

            CalculateScreenScale();

            ViewBounds = new Rectangle(0, 0,
                Convert.ToInt32(graphics.PreferredBackBufferWidth),
                Convert.ToInt32(graphics.PreferredBackBufferHeight));
        }

        public static void Reset()
        {
            if (IsFullHeadless)
                return;

            // Have to go into full screen, then exit, to fix weird .net core issue.
            graphics = Rendering.GraphicsDeviceManager;
            graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = (int)(_BASE_RES_X * SizeRatio);
            graphics.PreferredBackBufferHeight = (int)(_BASE_RES_Y * SizeRatio);
            graphics.ApplyChanges();
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Rendering.ResetRenderTargets();
            CalculateScreenScale();
        }
    }
}