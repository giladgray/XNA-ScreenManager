/**
 * Anit Das
 * Cobin Dopkeen
 * Gilad Gray
 * Nadia Rodriguez
 **/
#region File Description
//-----------------------------------------------------------------------------
// PlayerIndexEventArgs.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

//GameStateManagement is the old namespace
namespace ScreenManagement.Screens {
    /// <summary>
    /// Custom event argument which includes the index of the player who
    /// triggered the event. This is used by the MenuEntry.Selected event.
    /// </summary>
    public class PlayerIndexEventArgs : EventArgs {

        PlayerIndex playerIndex;

        /// <summary>
        /// Gets the index of the player who triggered this event.
        /// </summary>
        public PlayerIndex PlayerIndex {
            get { return playerIndex; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PlayerIndexEventArgs(PlayerIndex playerIndex) {
            this.playerIndex = playerIndex;
        }
    }

    //public class MenuSelectionEventArgs2 : PlayerIndexEventArgs {
    //    Screens.Old.MenuEntry entry;
    //    Screens.Old.MenuScreen screen;

    //    /// <summary>
    //    /// Gets the text of the selected menu entry
    //    /// </summary>
    //    public String MenuCommand {
    //        get { return entry.Text; }
    //    }

    //    /// <summary>
    //    /// Gets the menu entry that triggered this event
    //    /// </summary>
    //    public Screens.Old.MenuEntry MenuEntry {
    //        get { return entry; }
    //    }

    //    public Screens.Old.MenuScreen MenuScreen {
    //        get { return screen; }
    //    }

    //    public MenuSelectionEventArgs2(Screens.Old.MenuEntry entry, Screens.Old.MenuScreen screen, PlayerIndex playerIndex)
    //        : base(playerIndex) {
    //        this.entry = entry;
    //        this.screen = screen;
    //    }

    //}

    public class MenuSelectionEventArgs : PlayerIndexEventArgs {
        MenuEntry entry;
        MenuScreen screen;

        /// <summary>
        /// Gets the text of the selected menu entry
        /// </summary>
        public String MenuCommand {
            get { return entry.Text; }
        }

        /// <summary>
        /// Gets the menu entry that triggered this event
        /// </summary>
        public MenuEntry MenuEntry {
            get { return entry; }
        }

        public MenuScreen MenuScreen {
            get { return screen; }
        }

        public MenuSelectionEventArgs(MenuEntry entry, MenuScreen screen, PlayerIndex playerIndex)
            : base(playerIndex) {
            this.entry = entry;
            this.screen = screen;
        }

    }
}
