using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ScreenManagement.Screens {
    public enum Alignment {
        Left,
        Center,
        Right
    }
    public enum Orientation {
        Horizontal,
        Vertical
    }
    //public abstract class MenuCollection : ICollection<MenuEntry2> {
    //    public abstract void SelectUp();
    //    public abstract void SelectDown();
    //    public abstract void SelectLeft();
    //    public abstract void SelectRight();
    //}

    public class MenuScreen : GameScreen {

        #region fields

        List<MenuEntry> menuEntries;
        int selected;
        MenuEntry title;
        Vector2 menuPosition, titlePosition;
        Color menuColor = Color.White, menuSelectedColor = Color.Yellow;
        SpriteFont font;
        //bool inherit;
        Orientation orientation = Orientation.Vertical;

        protected Alignment DefaultAlignment = Alignment.Center;

        #endregion

        #region Properties

        public List<MenuEntry> MenuEntries {
            get { return menuEntries; }
        }

        public MenuEntry SelectedEntry {
            get {
                if (selected >= 0 && selected < menuEntries.Count)
                    return menuEntries[selected];
                else
                    return null;
            }
        }

        public int Selected {
            get { return selected; }
            set { selected = value; }
        }

        public MenuEntry Title {
            get { return title; }
            set { title = value; }
        }

        public Vector2 MenuPosition {
            get { return menuPosition; }
            set { menuPosition = value; }
        }

        public Vector2 TitlePosition {
            get { return titlePosition; }
            set { titlePosition = value; }
        }

        public SpriteFont Font {
            get { return font; }
            set { font = value; }
        }

        #endregion

        #region Initialization 

        public MenuScreen(string title)
                : base() {
            menuEntries = new List<MenuEntry>();
            this.title = new MenuEntry(title, null);
        }

        public override void LoadContent() {
            base.LoadContent();
            font = ScreenManager.Font;
        }

        public override void UnloadContent() {
            base.UnloadContent();
        }

        public MenuEntry AddMenuEntry(string text, Texture2D background, EventHandler<MenuSelectionEventArgs> selected, bool inherit) {
            MenuEntry entry = new MenuEntry(text, background, selected);
            entry.Alignment = DefaultAlignment;
            if (inherit) {
                entry.TextColor = menuColor;
                entry.TextSelectedColor = menuSelectedColor;
                entry.BackgroundColor = Color.White;
                entry.BackgroundSelectedColor = menuSelectedColor;
            }
            menuEntries.Add(entry);
            return entry;
        }

        public MenuEntry AddMenuLabel(string text, Texture2D background, Color textColor) {
            MenuEntry entry = new MenuEntry(text, background, null);
            entry.Enabled = false;
            entry.TextColor = textColor;
            entry.Alignment = DefaultAlignment;

            menuEntries.Add(entry);

            return entry;
        }

        #endregion

        #region Draw and Update

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime) {
            SpriteBatch sb = ScreenManager.SpriteBatch;
            sb.Begin();

            if(title != null)
                title.Draw(sb, font, titlePosition, false, gameTime);

            Vector2 position = menuPosition;
            int index = 0;
            foreach (MenuEntry entry in menuEntries) {
                entry.Draw(sb, font, position, selected == index, gameTime);
                if (orientation == Orientation.Vertical)
                    position.Y += entry.GetHeight(font) + entry.Padding;
                else
                    position.X += entry.GetWidth(font) + entry.Padding;
                index++;
            }

            sb.End();
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime,
                bool otherScreenHasFocus, bool coveredByOtherScreen) {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Update each nested MenuEntry object.
            for (int i = 0; i < menuEntries.Count; i++) {
                menuEntries[i].Update(this, IsActive && selected == i, gameTime);
            }
        }

        public override void HandleInput(InputState input) {
            if (MenuEntries.Count > 0) {

                int dir = 0;
                // Move to the previous menu entry?
                if (input.IsMenuUp(ControllingPlayer)) {
                    dir = -1;
                }

                // Move to the next menu entry?
                if (input.IsMenuDown(ControllingPlayer)) {
                    dir = 1;
                }
                if (dir != 0) {
                    do {
                        selected += dir;
                        if (selected < 0)
                            selected = menuEntries.Count - 1;
                        selected %= menuEntries.Count;
                    } while (!MenuEntries[selected].Enabled);
                }

                // Accept or cancel the menu? We pass in our ControllingPlayer, which may
                // either be null (to accept input from any player) or a specific index.
                // If we pass a null controlling player, the InputState helper returns to
                // us which player actually provided the input. We pass that through to
                // OnSelectEntry and OnCancel, so they can tell which player triggered them.
                PlayerIndex playerIndex;

                if (input.IsMenuSelect(ControllingPlayer, out playerIndex)) {
                    OnSelectEntry(selected, playerIndex);
                }
                else if (input.IsMenuCancel(ControllingPlayer, out playerIndex)) {
                    OnCancel(this, new MenuSelectionEventArgs(SelectedEntry, this, playerIndex));
                }
            }
        }


        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex) {
            menuEntries[entryIndex].OnSelectEntry(playerIndex, this);
        }


        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel(object sender, MenuSelectionEventArgs args) {
            ExitScreen();
        }

        #endregion

    }
}
