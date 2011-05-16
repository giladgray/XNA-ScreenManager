/**
 * Anit Das
 * Cobin Dopkeen
 * Gilad Gray
 * Nadia Rodriguez
 **/
#region File Description
//-----------------------------------------------------------------------------
// ScreenManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ScreenManagement;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace ScreenManagement {
    /// <summary>
    /// The GameScreen manager is a component which manages one or more GameScreen
    /// instances. It maintains a stack of screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes input to the
    /// topmost active GameScreen.
    /// </summary>
    public class ScreenManager : DrawableGameComponent {
        #region Fields

        List<GameScreen> screens = new List<GameScreen>();
        List<GameScreen> screensToUpdate = new List<GameScreen>();

        InputState input = new InputState();

        SpriteBatch spriteBatch;
        SpriteFont font, bigFont, smallFont;
        Texture2D blankTexture;
        GamerServicesComponent gamerServices;

        bool isInitialized;

        bool traceEnabled;

        #endregion

        #region Properties


        /// <summary>
        /// A default SpriteBatch shared by all the screens. This saves
        /// each GameScreen having to bother creating their own local instance.
        /// </summary>
        public SpriteBatch SpriteBatch {
            get { return spriteBatch; }
        }


        /// <summary>
        /// Gets or sets a font shared by all the screens. This saves
        /// each GameScreen having to bother loading their own local copy.
        /// </summary>
        /// <remarks>The default value is 22-pt Verdana</remarks>
        public SpriteFont Font {
            get { return font; }
            set { font = value; }
        }

        /// <summary>
        /// Gets or sets a larger font shared by all the screens (roughly 50% larger than
        /// the default font). 
        /// </summary>
        /// <remarks>The default value is 36-pt Verdana</remarks>
        public SpriteFont BigFont {
            get { return bigFont; }
            set { bigFont = value; }
        }

        /// <summary>
        /// Gets or sets a smaller font shared by all the screens (roughly half the
        /// size of the default font).
        /// </summary>
        /// <remarks>The default value is 12-pt Verdana</remarks>
        public SpriteFont SmallFont {
            get { return smallFont; }
            set { smallFont = value; }
        }


        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled {
            get { return traceEnabled; }
            set { traceEnabled = value; }
        }

        public GamerServicesComponent GamerServices {
            get { return gamerServices; }
            set { gamerServices = value; }
        }

        /// <summary>
        /// Gets a Vector2 containing the horizontal and vertical resolution of the screen.
        /// </summary>
        public Vector2 ScreenSize {
            get {
                return new Vector2(ScreenWidth, ScreenHeight);
            }
        }

        /// <summary>
        /// Gets the horizontal resolution of the screen.
        /// </summary>
        public int ScreenWidth {
            get { return GraphicsDevice.PresentationParameters.BackBufferWidth; }
        }

        /// <summary>
        /// Gets the vertical resolution of the screen.
        /// </summary>
        public int ScreenHeight {
            get { return GraphicsDevice.PresentationParameters.BackBufferHeight; }
        }

        /// <summary>
        /// Gets a Vector2 representing a point in the exact center of the screen, or
        /// half the width and half the height of the screen resolution.
        /// </summary>
        public Vector2 ScreenCenter {
            get { return ScreenSize / 2; }
        }

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new GameScreen manager component.
        /// </summary>
        public ScreenManager(Game game)
            : base(game) {
        }


        /// <summary>
        /// Initializes the GameScreen manager component.
        /// </summary>
        public override void Initialize() {
            base.Initialize();
            
            isInitialized = true;
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent() {
            // Load content belonging to the GameScreen manager.
            ContentManager content = Game.Content;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            smallFont = content.Load<SpriteFont>("ScreenManager/SmallFont");
            font = content.Load<SpriteFont>("ScreenManager/DefaultFont");
            bigFont = content.Load<SpriteFont>("ScreenManager/BigFont");
            blankTexture = content.Load<Texture2D>("ScreenManager/blank");
            //Screens.Old.CheckMenuEntry.CheckedIcon = blankTexture;

            // Tell each of the screens to load their content.
            foreach (GameScreen GameScreen in screens) {
                GameScreen.LoadContent();
            }
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent() {
            // Tell each of the screens to unload their content.
            foreach (GameScreen GameScreen in screens) {
                GameScreen.UnloadContent();
            }
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows each GameScreen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime) {
            // Read the keyboard and gamepad.
            input.Update();

            // Make a copy of the master GameScreen list, to avoid confusion if
            // the process of updating one GameScreen adds or removes others.
            screensToUpdate.Clear();

            foreach (GameScreen GameScreen in screens)
                screensToUpdate.Add(GameScreen);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0) {
                // Pop the topmost GameScreen off the waiting list.
                GameScreen GameScreen = screensToUpdate[screensToUpdate.Count - 1];

                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                // Update the GameScreen.
                GameScreen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (GameScreen.ScreenState == ScreenState.TransitionOn ||
                    GameScreen.ScreenState == ScreenState.Active) {
                    // If this is the first active GameScreen we came across,
                    // give it a chance to handle input.
                    if (!otherScreenHasFocus) {
                        GameScreen.HandleInput(input);

                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!GameScreen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }

            // Print debug trace?
            if (traceEnabled)
                TraceScreens();
        }


        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        void TraceScreens() {
            List<string> screenNames = new List<string>();

            foreach (GameScreen GameScreen in screens)
                screenNames.Add(GameScreen.GetType().Name);

            //Trace.WriteLine(string.Join(", ", screenNames.ToArray()));
        }


        /// <summary>
        /// Tells each GameScreen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime) {
            foreach (GameScreen GameScreen in screens) {
                if (GameScreen.ScreenState == ScreenState.Hidden)
                    continue;

                GameScreen.Draw(gameTime);
            }
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Adds a new GameScreen to the GameScreen manager.
        /// </summary>
        public void AddScreen(GameScreen GameScreen, PlayerIndex? controllingPlayer) {
            GameScreen.ControllingPlayer = controllingPlayer;
            GameScreen.ScreenManager = this;
            GameScreen.IsExiting = false;

            // If we have a graphics device, tell the GameScreen to load content.
            if (isInitialized) {
                GameScreen.LoadContent();
            }

            screens.Add(GameScreen);
        }


        /// <summary>
        /// Removes a GameScreen from the GameScreen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the GameScreen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen GameScreen) {
            // If we have a graphics device, tell the GameScreen to unload content.
            if (isInitialized) {
                GameScreen.UnloadContent();
            }

            screens.Remove(GameScreen);
            screensToUpdate.Remove(GameScreen);
        }

        /// <summary>
        /// Removes all GameScreens from the GameScreen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the GameScreens can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveAllScreens() {
            // If we have a graphics device, tell each GameScreen to unload content.
            if (isInitialized) {
                foreach (GameScreen screen in screens)
                    screen.UnloadContent();
            }

            screens.Clear();
            screensToUpdate.Clear();
        }

        /// <summary>
        /// Get a screen by index.
        /// </summary>
        public GameScreen GetScreen(int index) {
            if (index >= 0 && index < screens.Count)
                return screens[index];
            else
                return null;
        }

        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public GameScreen[] GetScreens() {
            return screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToColor(Color color, int alpha) {
            Viewport viewport = GraphicsDevice.Viewport;

            spriteBatch.Begin();
            spriteBatch.Draw(blankTexture,
                             new Rectangle(0, 0, viewport.Width, viewport.Height),
                             new Color(color.R, color.G, color.B, alpha));
                             //new Color(0, 0, 0, (byte)alpha));

            spriteBatch.End();
        }


        #endregion
    }
}
