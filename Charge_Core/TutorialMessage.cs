using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Charge
{
    class TutorialMessage
    {
        private static int ID_COUNTER = 0; // Increments the id of each TutorialMessage

        private String message;
        private Vector2 messagePosition;

        private float time; // Time, in seconds, to show the message
        private float timeSinceCreated; // How much time, in seconds, has passed since this message was first shown
        private bool doesExpire; // Should this message disappear after a set amount of time

        private Texture2D tex;
        private Color texColor;
        private Rectangle texturePosition;
        private float textureRotation;
        private Vector2 textureOrigin;

        private int id;

        public TutorialMessage(String message, bool doesExpire, Vector2 messagePosition, Color texColor, Rectangle? texturePosition, Texture2D tex = null, float rotation = 0.0f, float time = 0.0f)
        {
            this.message = message;
            this.messagePosition = messagePosition;

            if (texturePosition.HasValue)
            {
                this.texturePosition = texturePosition.Value;
            }

            this.tex = tex;
            this.textureRotation = rotation;
            this.texColor = texColor;

            if (tex != null)
            {
                textureOrigin = new Vector2(tex.Width / 2, tex.Height / 2);
            }

            this.time = time;
            timeSinceCreated = 0;
            this.doesExpire = doesExpire;

            id = ID_COUNTER;
            ID_COUNTER++;
        }

        public void Update(float deltaTime)
        {
            timeSinceCreated += deltaTime;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (tex != null)
            {
                spriteBatch.Draw(tex, texturePosition, null, texColor, textureRotation, textureOrigin, SpriteEffects.None, 0);
            }

            spriteBatch.DrawString(ChargeMain.FontSmall, message, new Vector2(messagePosition.X + 2, messagePosition.Y + 2), Color.Black);
            spriteBatch.DrawString(ChargeMain.FontSmall, message, messagePosition, Color.WhiteSmoke);
        }

        /// <summary>
        /// Resets the current timer and sets a new timeout for the message. The message will disappear [timeout] seconds from when this function is called.
        /// </summary>
        /// <param name="timeout">The new timeout, in seconds</param>
        public void SetTimeout(float timeout)
        {
            timeSinceCreated = 0;
            time = timeout;
        }

        public bool ShouldRemove()
        {
            return doesExpire && timeSinceCreated >= time;
        }

        public int GetId()
        {
            return id;
        }
    }
}
