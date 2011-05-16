using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScreenManagement.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ScreenManagement.Screens {
    public class RadialMenuScreen : MenuScreen {
        private int radius;
        private float offset;

        public int Radius {
            get { return radius; }
            set { radius = value; }
        }

        public float Offset {
            get { return offset; }
            set { offset = value; }
        }

        public RadialMenuScreen(string title) : this(title, 100, 0) { }

        public RadialMenuScreen(string title, int radius, float offset)
            : base(title) {
                this.radius = radius;
                this.offset = offset;
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime) {
            SpriteBatch sb = ScreenManager.SpriteBatch;
            sb.Begin();

            if (Title != null)
                Title.Draw(sb, Font, TitlePosition, false, gameTime);

            if (MenuEntries.Count > 0) {
                float theta = MathHelper.TwoPi / MenuEntries.Count, angle = offset;
                int index = 0;
                Vector2 position;
                foreach (MenuEntry item in MenuEntries) {
                    position = MenuPosition + new Vector2(radius * (float)Math.Cos(angle), radius * (float)Math.Sin(angle));
                    item.Draw(sb, Font, position, Selected == index++, gameTime);
                    angle += theta;
                }
                //sb.Draw(Constants.blank, new Rectangle((int)origin.X - 2, (int)origin.Y - 2, radius / 2, 5), null,
                //    GameplayScreen.FadeColor(Color.Orange, 200), theta * selected, Vector2.Zero, SpriteEffects.None, 0);
                //sb.DrawString(font, "" + selected, origin, Color.Red);
            }

            sb.End();
        }

        public override void HandleInput(ScreenManagement.InputState input) {
            //check players in reverse order so Player One always has precedence
            if (ControllingPlayer == null) {
                for (int i = 3; i >= 0; i--) {
                    ProcessThumbstick(input.CurrentGamePadStates[i].ThumbSticks.Left);
                }
            }
            else
                ProcessThumbstick(input.CurrentGamePadStates[(int)ControllingPlayer.Value].ThumbSticks.Left);

#if WINDOWS
            if (input.IsMouseMoved()) {
                ProcessThumbstick(input.CurrentMousePosition - MenuPosition);
            }
#endif

            PlayerIndex playerIndex;

            if (input.IsMenuSelect(ControllingPlayer, out playerIndex)) {
                OnSelectEntry(Selected, playerIndex);
            }
            else if (input.IsMenuCancel(ControllingPlayer, out playerIndex)) {
                OnCancel(this, new MenuSelectionEventArgs(SelectedEntry, this, playerIndex));
            }
        }

        private void ProcessThumbstick(Vector2 thumbstick) {
            double theta, angle;
            if (thumbstick.LengthSquared() > 0) {
                theta = Math.PI * 2 / MenuEntries.Count;
                angle = Math.Atan2(-thumbstick.Y, thumbstick.X) + theta / 2 + Math.PI * 2 - offset;
                Selected = (int)(angle / theta) % MenuEntries.Count;
            }
        }
    }
}
