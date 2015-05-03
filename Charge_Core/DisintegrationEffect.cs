using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Charge
{
    class DisintegrationEffect : WorldEntity
    {
        public PixelEffect disintegrated; //Disintegrated section
        float wipePercent; //Percent desintegrated
        bool horizontal; //Horiontal or vertical disintegration
        float wipeSpeed; //Speed at which it disintegrates
        float inc; //Used to manage float speeds with in positions

        /// <summary>
        /// Create a disintegration effect
        /// </summary>
        /// <param name="position">Main position for the effect</param>
        /// <param name="toWipe">Texture to disintegrate</param>
        /// <param name="pixelTex">Texture for pixels</param>
        /// <param name="colors">List of pixel colors</param>
        /// <param name="wipeTime">Time it takes to disintegrate</param>
        /// <param name="horizontal">Disintegrate horizontally if true, vertically if false</param>
        public DisintegrationEffect(Rectangle position, Texture2D toWipe, Texture2D pixelTex, List<Color> colors, float wipeTime, bool horizontal)
        {
            init(position, toWipe);
            this.horizontal = horizontal;
            SetWipeTime(wipeTime);
            inc = 0;
            wipePercent = 0;
            int disWidth = 0;
            int disHeight = 0;
            if (horizontal) disHeight = position.Height;
            else disWidth = position.Width;
            disintegrated = new PixelEffect(new Rectangle(position.X, position.Y, disWidth, disHeight), pixelTex, colors);
            disintegrated.SetSpawnFreqAndFade(3, wipeTime);
            if (horizontal) disintegrated.xVel = -20;
            else disintegrated.yVel = -20;
        }


        public override void Update(float deltaTime, float playerSpeed, float objectSpeed = 0.0f)
        {
            //Move the amount disintegrated
            inc += wipeSpeed * deltaTime;
            int moveAmt = 0;
            if (inc > 1)
            {
                moveAmt = Convert.ToInt32(Math.Floor(inc));
                inc = inc % 1;
            }
            if(moveAmt > 0) MoveWipe(moveAmt);

            //Change the disintegrated pixel effect's size
            disintegrated.position = GetDisintegratedRectangle();
            disintegrated.Update(deltaTime, playerSpeed);

            //Done disintegrating
            if (wipePercent >= 1)
            {
                wipePercent = 1;
                this.destroyMe = true;
            }

            base.Update(deltaTime, playerSpeed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            disintegrated.Draw(spriteBatch);
            spriteBatch.Draw(tex, GetRemainingRectangle(), GetCropRectangle(), Color.White);
        }

        /// <summary>
        /// Provides the rectangle position defining which region of the toWipe texture we should draw
        /// </summary>
        public Rectangle GetCropRectangle()
        {
            int imgWidth = tex.Width;
            int imgHeight = tex.Height;

            int x = 0;
            int y = 0;
            if(horizontal) x = Convert.ToInt32(Math.Floor(imgWidth * wipePercent));
            else y = Convert.ToInt32(Math.Floor(imgHeight * wipePercent));
            
            int width = imgWidth - x;
            int height = imgHeight - y;

            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Provides the position of the remaining non-disintegrated section
        /// </summary>
        public Rectangle GetRemainingRectangle()
        {
            int x = position.X;
            int y = position.Y;
            
            if(horizontal) x += Convert.ToInt32(Math.Floor(position.Width * wipePercent));
            else y += Convert.ToInt32(Math.Floor(position.Height * wipePercent));

            int width = position.Width;
            int height = position.Height;
            
            if(horizontal) width = Convert.ToInt32(Math.Floor(position.Width * (1 - wipePercent)));
            else height = Convert.ToInt32(Math.Floor(position.Height * (1 - wipePercent)));

            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Provides the position of the disintgerated section
        /// </summary>
        public Rectangle GetDisintegratedRectangle()
        {
            Rectangle remaining = GetRemainingRectangle();
            int x = 0;
            int y = 0;
            if(disintegrated != null) {
                x = disintegrated.position.X;
                y = disintegrated.position.Y;
            }
            else
            {
                x = position.X;
                y = position.Y;
            }
            
            int width = position.Width + (position.X - disintegrated.position.X);
            int height = position.Height + (position.Y - disintegrated.position.Y);

            if (horizontal) width -= remaining.Width/3;
            else height -= remaining.Height/3;

            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Returns width if horizontal, height otherwise.
        /// </summary>
        public float GetWidthOrHeight()
        {
            if (horizontal) return position.Width;
            return position.Height;
        }

        /// <summary>
        /// Moves the disintegrated section by the given amount
        /// </summary>
        public void MoveWipe(float amt)
        {
            float percent = amt / GetWidthOrHeight();
            wipePercent += percent;
        }

        /// <summary>
        /// Sets the time that the disintegration will take
        /// </summary>
        /// <param name="time">Time in seconds</param>
        public void SetWipeTime(float time)
        {
            if (time == -1)
            {
                wipeSpeed = 0;
                return;
            }
            
            wipeSpeed = GetWidthOrHeight() / time;
        }

    }
}
