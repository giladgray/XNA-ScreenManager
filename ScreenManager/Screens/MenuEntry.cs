using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ScreenManagement.Screens {
    public class MenuEntry {
        #region Fields

        string text;
        Texture2D background;

        Color textColor         = Color.Black;
        Color textSelectedColor = Color.Yellow;
        Color bgColor           = Color.White;
        Color bgSelectedColor   = Color.White;
        Alignment alignment     = Alignment.Center;

        int padding = 8;
        float scale = 1.0f;
        float rotation = 0f;
        bool enabled = true, visible = true;
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the label of this menu entry.
        /// </summary>
        public string Text {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// Gets or sets the background image.
        /// </summary>
        public Texture2D Background {
            get { return background; }
            set { background = value; }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment of the entry relative to its position
        /// </summary>
        public Alignment Alignment {
            get { return alignment; }
            set { alignment = value; }
        }

        /// <summary>
        /// Gets or sets the text color
        /// </summary>
        public Color TextColor {
            get { return textColor; }
            set { textColor = value; }
        }

        /// <summary>
        /// Gets or sets the color of text when the entry is selected
        /// </summary>
        public Color TextSelectedColor {
            get { return textSelectedColor; }
            set { textSelectedColor = value; }
        }

        /// <summary>
        /// Gets or sets the color used for tinting the background image
        /// </summary>
        public Color BackgroundColor {
            get { return bgColor; }
            set { bgColor = value; }
        }


        /// <summary>
        /// Gets or sets the color used for tinting the background image
        /// when the entry is selected.
        /// </summary>
        public Color BackgroundSelectedColor {
            get { return bgSelectedColor; }
            set { bgSelectedColor = value; }
        }

        /// <summary>
        /// Gets or sets the empty space between neighboring menu entries.
        /// </summary>
        public int Padding {
            get { return padding; }
            set { padding = value; }
        }

        /// <summary>
        /// Gets or sets the scale factor of the entry, affecting both text and image.
        /// </summary>
        public float Scale {
            get { return scale; }
            set { scale = value; }
        }

        /// <summary>
        /// Gets or sets the angle of rotation in radians.
        /// </summary>
        public float Rotation {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// Gets or sets whether this menu entry can be selected.
        /// If disabled, the entry will be skipped over.
        /// </summary>
        public bool Enabled {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets or sets whether the entry is drawn to the screen.
        /// </summary>
        public bool Visible {
            get { return visible; }
            set { visible = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new MenuEntry that displays some text. 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="select"></param>
        public MenuEntry(string text,
                          EventHandler<MenuSelectionEventArgs> select) {
            this.text = text;
            this.Selected += select;
        }

        /// <summary>
        /// Creates a new MenuEntry that displays text centered on a background texture.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="background"></param>
        /// <param name="select"></param>
        public MenuEntry(string text, Texture2D background,
                          EventHandler<MenuSelectionEventArgs> select)
            : this(text, select) {
            this.background = background;
        }

        #endregion


        #region Events


        /// <summary>
        /// Event raised when the menu entry is selected.
        /// </summary>
        public event EventHandler<MenuSelectionEventArgs> Selected;


        /// <summary>
        /// Method for raising the Selected event.
        /// </summary>
        protected internal virtual void OnSelectEntry(PlayerIndex playerIndex, MenuScreen screen) {
            if (Selected != null)
                Selected(this, new MenuSelectionEventArgs(this, screen, playerIndex));
        }

        /// <summary>
        /// Performs the Selected event for this menu entry.
        /// </summary>
        /// <param name="playerIndex">index of the player who initiated the action</param>
        public virtual void DoClick(PlayerIndex playerIndex, MenuScreen screen) {
            OnSelectEntry(playerIndex, screen);
        }


        #endregion

        public virtual float GetHeight(SpriteFont font) {
            if (background != null)
                return background.Height;
            else
                return font.MeasureString(text).Y;
        }
        public virtual float GetWidth(SpriteFont font) {
            if (background != null)
                return background.Width;
            else
                return font.MeasureString(text).X;
        }

        public virtual void Update(MenuScreen screen, bool isSelected,
                                                      GameTime gameTime) {

        }
        /// <summary>
        /// Draws the MenuEntry onto the given SpriteBatch using a specified font.
        /// </summary>
        /// <param name="sb">SpriteBatch to draw MenuEntry onto</param>
        /// <param name="font">font for menu entry text</param>
        /// <param name="position">coordinates of center of MenuEntry</param>
        /// <param name="isSelected">whether the entry is currently selected in the menu</param>
        /// <param name="gameTime">snapshot of current game timing</param>
        public virtual void Draw(SpriteBatch sb, SpriteFont font, Vector2 position,
                                 bool isSelected, GameTime gameTime) {
            if (!visible) return;

            if (background != null) {   //text and image
                Vector2 origin = new Vector2((int)alignment * background.Width / 2,
                    background.Height / 2);
                sb.Draw(background, position, null, isSelected ? bgSelectedColor : bgColor, 
                    rotation, origin, scale, SpriteEffects.None, 0);
                sb.DrawString(font, text, position + new Vector2(background.Width / 2, background.Height / 2) - font.MeasureString(text) / 2, isSelected ? textSelectedColor : textColor, 
                    rotation, origin, scale, SpriteEffects.None, 0);
            }
            else {  //just text
                Vector2 origin = font.MeasureString(text) / 2;
                origin.X *= (int)alignment;
                sb.DrawString(font, text, position, isSelected ? textSelectedColor : textColor, 
                    0, origin, scale, SpriteEffects.None, 0);
            }
        }

        //public void Draw(MenuScreen2 screen, Vector2 position, bool isSelected, GameTime gameTime) {
        //    Draw(screen.ScreenManager.SpriteBatch, screen.Font, position, isSelected, gameTime);
        //}
    }
}
