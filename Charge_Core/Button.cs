using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Charge
{
    class Button
    {
        private Texture2D tex;
        private Color backgroundColor;

        private SpriteFont font;
        private String text;
        private Color foregroundColor;

        private Rectangle position;

        /// <summary>
        /// Creates a new button
        /// </summary>
        /// <param name="text">The text of the button</param>
        /// <param name="position">Position within the window where the button will be drawn</param>
        /// <param name="tex">Background texture for the button</param>
        /// <param name="font">Font to draw the text</param>
        /// <param name="backgroundColor">Color to draw the background texture</param>
        /// <param name="foregroundColor">Color to draw the text</param>
        public Button(String text, Rectangle position, Color foregroundColor, SpriteFont font, Texture2D tex, Color backgroundColor)
        {
            this.tex = tex;
            this.backgroundColor = backgroundColor;

            this.text = text;
            this.font = font;
            this.foregroundColor = foregroundColor;

            this.position = position;
        }

        /// <summary>
        /// Draws the button
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the background
            spriteBatch.Draw(tex, position, backgroundColor);

            // Center the text within the Button
            Vector2 textSize = font.MeasureString(text);
            float textX = position.Left + (position.Width - textSize.X) / 2;
            float textY = position.Top + (position.Height - textSize.Y) / 2;
            Vector2 textPos = new Vector2(textX, textY);

            // Draw the text
            spriteBatch.DrawString(font, text, textPos, foregroundColor);
        }

        /// <summary>
        /// Returns the region that the Button occupies
        /// </summary>
        /// <returns></returns>
        public Rectangle GetButtonRegion()
        {
            return position;
        }
    }
}
