/**
 * Anit Das
 * Cobin Dopkeen
 * Gilad Gray
 * Nadia Rodriguez
 **/
#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace ScreenManagement {
    /// <summary>
    /// Enumerates mouse buttons
    /// </summary>
    public enum MouseButtons { Left, Middle, Right, XButton1, XButton2, Wheel }

    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// methods for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState {
        #region Fields

        public const int MaxInputs = 4;

        public readonly KeyboardState[] CurrentKeyboardStates;
        public readonly GamePadState[] CurrentGamePadStates;

        public readonly KeyboardState[] LastKeyboardStates;
        public readonly GamePadState[] LastGamePadStates;

        public MouseState CurrentMouseState;
        public MouseState LastMouseState;
        public Vector2 CurrentMousePosition, LastMousePosition;

        public readonly bool[] GamePadWasConnected;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState() {
            CurrentKeyboardStates = new KeyboardState[MaxInputs];
            CurrentGamePadStates = new GamePadState[MaxInputs];

            LastKeyboardStates = new KeyboardState[MaxInputs];
            LastGamePadStates = new GamePadState[MaxInputs];

            CurrentMousePosition = new Vector2();

            GamePadWasConnected = new bool[MaxInputs];
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update() {
            LastMouseState = CurrentMouseState;
            LastMousePosition = CurrentMousePosition;
            CurrentMouseState = Mouse.GetState();
            CurrentMousePosition.X = CurrentMouseState.X;
            CurrentMousePosition.Y = CurrentMouseState.Y;
            for (int i = 0; i < MaxInputs; i++) {
                LastKeyboardStates[i] = CurrentKeyboardStates[i];
                LastGamePadStates[i] = CurrentGamePadStates[i];

                CurrentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
                CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);

                // Keep track of whether a gamepad has ever been
                // connected, so we can detect if it is unplugged.
                if (CurrentGamePadStates[i].IsConnected) {
                    GamePadWasConnected[i] = true;
                }
            }
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer,
                                            out PlayerIndex playerIndex) {
            if (controllingPlayer.HasValue) {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentKeyboardStates[i].IsKeyDown(key) &&
                        LastKeyboardStates[i].IsKeyUp(key));
            }
            else {
                // Accept input from any player.
                return (IsNewKeyPress(key, PlayerIndex.One, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Three, out playerIndex) ||
                        IsNewKeyPress(key, PlayerIndex.Four, out playerIndex));
            }
        }

        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. This
        /// method is designed for use in PC games where there is only one keyboard.
        /// </summary>
        public bool IsNewKeyPress(Keys key) {
            return (CurrentKeyboardStates[0].IsKeyDown(key) &&
                    LastKeyboardStates[0].IsKeyUp(key));
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a button press
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer,
                                                     out PlayerIndex playerIndex) {
            if (controllingPlayer.HasValue) {
                // Read input from the specified player.
                playerIndex = controllingPlayer.Value;

                int i = (int)playerIndex;

                return (CurrentGamePadStates[i].IsButtonDown(button) &&
                        LastGamePadStates[i].IsButtonUp(button));
            }
            else {
                // Accept input from any player.
                return (IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
                        IsNewButtonPress(button, PlayerIndex.Four, out playerIndex));
            }
        }

        /// <summary>
        /// Helper for checking if a button was newly pressed during this update.
        /// This method is designed for use in one-player games and only checks
        /// the gamepad for player one.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool IsNewButtonPress(Buttons button) {
            return (CurrentGamePadStates[0].IsButtonDown(button) &&
                    LastGamePadStates[0].IsButtonUp(button));
        }

        /// <summary>
        /// Helper for checking if a mouse button was newly pressed during this update.
        /// No controlling player parameter is required because there is only one mouse.
        /// </summary>
        public bool IsNewMouseButtonPress(MouseButtons button) {
            switch (button) {
                case MouseButtons.Left:
                    return CurrentMouseState.LeftButton == ButtonState.Pressed &&
                        LastMouseState.LeftButton == ButtonState.Released;
                case MouseButtons.Right:
                    return CurrentMouseState.RightButton == ButtonState.Pressed &&
                        LastMouseState.RightButton == ButtonState.Released;
                case MouseButtons.Middle:
                    return CurrentMouseState.MiddleButton == ButtonState.Pressed &&
                        LastMouseState.MiddleButton == ButtonState.Released;
                case MouseButtons.XButton1:
                    return CurrentMouseState.XButton1 == ButtonState.Pressed &&
                        LastMouseState.XButton1 == ButtonState.Released;
                case MouseButtons.XButton2:
                    return CurrentMouseState.XButton2 == ButtonState.Pressed &&
                        LastMouseState.XButton2 == ButtonState.Released;
                case MouseButtons.Wheel:
                    return CurrentMouseState.ScrollWheelValue !=
                        LastMouseState.ScrollWheelValue;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Helper for checking if the position of the mouse has changed during this update.
        /// </summary>
        public bool IsMouseMoved() {
            return CurrentMouseState.X != LastMouseState.X ||
                CurrentMouseState.Y != LastMouseState.Y;
        }


        /// <summary>
        /// Checks for a "menu select" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuSelect(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex) {
            return IsNewKeyPress(Keys.Space, controllingPlayer, out playerIndex) ||
                   IsNewKeyPress(Keys.Enter, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.A, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuCancel(PlayerIndex? controllingPlayer,
                                 out PlayerIndex playerIndex) {
            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.B, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu up" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuUp(PlayerIndex? controllingPlayer) {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Up, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadUp, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickUp, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu down" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuDown(PlayerIndex? controllingPlayer) {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Down, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadDown, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickDown, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu left" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuLeft(PlayerIndex? controllingPlayer) {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Left, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadLeft, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickLeft, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "menu right" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuRight(PlayerIndex? controllingPlayer) {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Right, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.DPadRight, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.LeftThumbstickRight, controllingPlayer, out playerIndex);
        }


        /// <summary>
        /// Checks for a "pause the game" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsPauseGame(PlayerIndex? controllingPlayer) {
            PlayerIndex playerIndex;

            return IsNewKeyPress(Keys.Escape, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Back, controllingPlayer, out playerIndex) ||
                   IsNewButtonPress(Buttons.Start, controllingPlayer, out playerIndex);
        }


        #endregion
    }
}
