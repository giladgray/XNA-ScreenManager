using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ScreenManagement.Screens.Old {
    public struct Selection {
        public static Selection Zero { get { return new Selection(0, 0, 0); } }
        public static Selection None { get { return new Selection(-1, -1, -1); } }

        public int Column, Row, Index;

        public Selection(int column, int row, int index) {
            this.Column = column;
            this.Row = row;
            this.Index = index;
        }

        public bool Equals(int col, int row, int index = 0) {
            return Column == col && Row == row;
        }
    }

    public class FancyMenuScreen : MenuScreen {

        #region Fields

        List<MenuEntry>[] menuColumns;
        //protected new Selection selectedEntry;
        Vector2[] columnPosition;
        bool relativeSelection = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the list of menu columns, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected new List<MenuEntry>[] MenuEntries {
            get { return menuColumns; }
        }

        /// <summary>
        /// Gets or sets the position of the first menu item in each column. 
        /// All items are arranged vertically under the first item.
        /// </summary>
        public new Vector2[] MenuPosition {
            get { return columnPosition; }
            set { columnPosition = value; }
        }

        /// <summary>
        /// Gets the selected menu entry.
        /// </summary>
        public MenuEntry Selected {
            get {
                if (selectedEntry.Column < 0 || selectedEntry.Row < 0)
                    return null;
                return MenuEntries[selectedEntry.Column][selectedEntry.Row]; 
            }
        }

        /// <summary>
        /// Gets or sets whether relative or absolute selection is used for changing columns.
        /// Absolute selection preserves row selection across columns while relative selection chooses
        /// the nearest menu entry using onscreen position.
        /// </summary>
        public bool RelativeSelection {
            get { return relativeSelection; }
            set { relativeSelection = value; }
        }

        private bool IsVertical {
            get { return Orientation == Screens.Orientation.Vertical; }
        }

        #endregion

        /// <summary>
        /// Creates a new FancyMenuScreen with the given title, number of columns, 
        /// and column orientation. 
        /// </summary>
        /// <param name="title">the title of the menu</param>
        /// <param name="columns">the number of menu columns</param>
        /// <param name="vertical">whether the columns should be oriented vertically (or horizontally if false)</param>
        public FancyMenuScreen(string title, int columns, Orientation orientation = Orientation.Vertical)
            : base(title, orientation) {
            if (columns < 1)
                throw new ArgumentOutOfRangeException("There must be at least one column.");
            AllowEmptySelection = true;

            columnPosition = new Vector2[columns];
            menuColumns = new List<MenuEntry>[columns];
            //put the list of entries from MenuScreen in the first entry of the columns array
            menuColumns[0] = base.MenuEntries;
            //fill the rest of the array with new lists
            for (int i = 1; i < columns; i++) {
                menuColumns[i] = new List<MenuEntry>();
            }

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        Texture2D blank;
        public override void LoadContent() {
            base.LoadContent();
            blank = GameInstance.Content.Load<Texture2D>("ScreenManager/blank");
        }

        /// <summary>
        /// Adds a new menu entry to the menu with the given event handler.
        /// </summary>
        /// <param name="entry">MenuEntry to add</param>
        /// <param name="evt">event handler for selected the menu entry</param>
        /// <returns>the menu entry that was added</returns>
        public MenuEntry AddMenuEntry(int column, MenuEntry entry,
                                      EventHandler<MenuSelectionEventArgs2> evt = null) {
            if (column < 0 || column > menuColumns.Length)
                throw new ArgumentOutOfRangeException("invalid menu column");
            entry.Selected += evt;
            menuColumns[column].Add(entry);
            return entry;
        }

        /// <summary>
        /// Adds a new menu entry to the menu with the given text, alignment, 
        /// and event handler.
        /// </summary>
        /// <param name="text">text of the menu entry</param>
        /// <param name="align">horizontal alignment of menu entry</param>
        /// <param name="evt">event handler for selecting menu entry</param>
        /// <returns>the menu entry that was added</returns>
        public MenuEntry AddMenuEntry(int column, string text, Alignment align = Alignment.Left,
                                      EventHandler<MenuSelectionEventArgs2> evt = null) {
            if (column < 0 || column > menuColumns.Length)
                throw new ArgumentOutOfRangeException("invalid menu column");
            return AddMenuEntry(column, new MenuEntry(text, align), evt);
        }

        #region Handle Input

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input) {
            int dirX = 0, dirY = 0;
            //figure out the directions to move
            if (input.IsMenuUp(ControllingPlayer))
                dirX = -1;
            if (input.IsMenuDown(ControllingPlayer))
                dirX = 1;
            if (input.IsMenuLeft(ControllingPlayer))
                dirY = -1;
            if (input.IsMenuRight(ControllingPlayer))
                dirY = 1;
            //if horizontal orientation, swap directions
            if (!IsVertical) {
                int temp = dirX;
                dirX = dirY;
                dirY = temp;
            }
            //process row movement
            if (dirX != 0) {
                if (selectedEntry.Column < 0 || selectedEntry.Column >= menuColumns.Length)
                    selectedEntry.Column = 0;
                int length = menuColumns[selectedEntry.Column].Count;
                do {
                    selectedEntry.Row += dirX;
                    if (selectedEntry.Row < 0)
                        selectedEntry.Row = length - 1;
                    selectedEntry.Row %= length;
                } while (!Selected.Enabled);
            }

            //handle column selection (left/right)
            //ultimately, index selection will also be in here
            //  if the index can go up/down, let it. otherwise, change columns. easy.
            if (dirY != 0) {
                if (selectedEntry.Row < 0 || selectedEntry.Row >= menuColumns[selectedEntry.Column].Count)
                    selectedEntry.Row = 0;
                if (relativeSelection)
                    ChangeColumnsRelative(dirY);
                else
                    ChangeColumns(dirY);
            }


            // Accept or cancel the menu? We pass in our ControllingPlayer, which may
            // either be null (to accept input from any player) or a specific index.
            // If we pass a null controlling player, the InputState helper returns to
            // us which player actually provided the input. We pass that through to
            // OnSelectEntry and OnCancel, so they can tell which player triggered them.
            PlayerIndex playerIndex;

            if (input.IsMenuSelect(ControllingPlayer, out playerIndex)) {
                OnSelectEntry(selectedEntry, playerIndex);
            }
            else if (input.IsMenuCancel(ControllingPlayer, out playerIndex)) {
                OnCancel(playerIndex);
            }

            #region Mouse Handling
#if WINDOWS
            bool clicked = input.IsNewMouseButtonPress(MouseButtons.Left);
            if (input.IsMouseMoved() || clicked) {
                mousePosition.X = input.CurrentMouseState.X;
                mousePosition.Y = input.CurrentMouseState.Y;
                //check mouse pos against each menu entry and update selection if necessary
                if (AllowEmptySelection)
                    selectedEntry.Column = selectedEntry.Row = selectedEntry.Index = -5;
                for (int col = 0; col < menuColumns.Length; col++) {
                    for (int i = 0; i < menuColumns[col].Count; i++) {
                        MenuEntry menuEntry = menuColumns[col][i];

                        if (menuEntry.Contains(mousePosition) && menuEntry.Enabled) {
                            selectedEntry.Column = col;
                            selectedEntry.Row = i;
                            if (clicked)
                                OnSelectEntry(selectedEntry, PlayerIndex.One);
                            
                        }
                    }
                }
            }
#endif

            HandleSpecialInput(input, Selected);
            #endregion
        }

        private void ChangeColumns(int dir) {
            do {
                selectedEntry.Column += dir;
                if (selectedEntry.Column < 0)
                    selectedEntry.Column = menuColumns.Length - 1;
                selectedEntry.Column %= menuColumns.Length;
                selectedEntry.Row = Math.Min(selectedEntry.Row, menuColumns[selectedEntry.Column].Count - 1);
            } while (!Selected.Enabled);
        }

        private void ChangeColumnsRelative(int dir) {
            //get the Y coord of the current menu entry
            float curPos = (IsVertical ? Selected.Position.Y : Selected.Position.X);
            //move the selected column
            selectedEntry.Column += dir;
            if (selectedEntry.Column < 0)
                selectedEntry.Column = menuColumns.Length - 1;
            selectedEntry.Column %= menuColumns.Length;
            //find closest menu entry
            int row = 0, minRow = -1;
            float min = 1000;
            foreach (MenuEntry e in menuColumns[selectedEntry.Column]) {
                //only process enabled entries
                if (e.Enabled) {
                    //find vertical distance between entries
                    //float dist = Math.Abs((e.Position.Y + e.Size.Y / 2) - curPos);
                    float dist = Math.Abs((IsVertical ? e.Position.Y : e.Position.X) - curPos);
                    //if they're perfectly aligned, make the move instantly
                    if (dist == 0) {
                        selectedEntry.Row = row;
                        return;
                    }
                    //if not, we search for the smallest distance
                    else if (dist < min) {
                        min = dist;
                        minRow = row;
                    }
                }
                row++;
            }
            //if we found a suitable row, change selection. otherwise, move another column
            if (minRow >= 0)
                selectedEntry.Row = minRow;
            else
                ChangeColumnsRelative(dir);
        }


        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(Selection selected, PlayerIndex playerIndex) {
            menuColumns[selected.Column][selected.Row].OnSelectEntry(playerIndex, this);
        }

        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen) {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            // base.Update updates the first column, but we still need to update the rest
            for (int col = 1; col < menuColumns.Length; col++)
                for (int i = 0; i < menuColumns[col].Count; i++)
                    menuColumns[col][i].Update(this, IsActive && selectedEntry.Equals(col, i), gameTime);
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime) {
            if (!valid)
                DoLayout();

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            spriteBatch.Begin();

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            Vector2 shift = Vector2.Zero;
            if (TransitionMovement) {
                if (ScreenState == ScreenState.TransitionOn)
                    shift.X = transitionOffset * 256;
                else
                    shift.X = transitionOffset * 512;
            }

            // Draw each menu entry in turn.
            for (int col = 0; col < menuColumns.Length; col++)
                for (int i = 0; i < menuColumns[col].Count; i++) {
                    menuColumns[col][i].Draw(this, shift, IsActive && selectedEntry.Equals(col, i), gameTime);
                    //Vector2 pos = menuColumns[col][i].Position;
                    //pos += new Vector2(0, menuColumns[col][i].Size.Y / 4);
                    //spriteBatch.Draw(blank, pos, Color.Red);
                }

            DrawTitle(spriteBatch, transitionOffset);

            if (Cursor != null) {
                Vector2 center = new Vector2(Cursor.Width / 2, Cursor.Height / 2);
                spriteBatch.Draw(Cursor, mousePosition - center, Color.White);
            }

            spriteBatch.DrawString(font, string.Format("({0},{1},{2})",
                selectedEntry.Column, selectedEntry.Row, selectedEntry.Index),
                new Vector2(30, ScreenManager.ScreenHeight - 40), Color.Coral);

            spriteBatch.End();
        }

        /// <summary>
        /// Recalculate the positions and sizes of each menu entry.
        /// </summary>
        protected override void DoLayout() {
            SpriteFont font = ScreenManager.Font;
            for (int col = 0; col < menuColumns.Length; col++) {
                Vector2 position = columnPosition[col];
                for (int i = 0; i < menuColumns[col].Count; i++) {
                    MenuEntry entry = menuColumns[col][i];
                    entry.Position = position;
                    Vector2 text = font.MeasureString(entry.Text);
                    //if (entry.Background != null) {
                    //    Vector2 image = new Vector2(entry.Background.Width, entry.Background.Height);
                    //    entry.Size = new Vector2(Math.Max(text.X, image.X), Math.Max(text.Y, image.Y));
                    //}
                    //else
                    entry.Size = text;
                    if (IsVertical)
                        position.Y += entry.GetHeight();
                    else
                        position.X += entry.GetWidth();
                }
            }
            valid = true;
        }

        #endregion
    }
}
