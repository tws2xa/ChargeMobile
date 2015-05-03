using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace Charge
{
    class Background
    {

        Texture2D bkgImg;
        float scrollPos; //The position of the background's scrolling

        /// <summary>
        /// Create the background.
        /// </summary>
        public Background(Texture2D bkgImg)
        {
            this.bkgImg = bkgImg;
            scrollPos = 0;
        }

        /// <summary>
        /// Updates the background's scroll
        /// </summary>
        public void Update(float deltaTime, float playerSpeed, float objectSpeed = 0.0f)
        {
            scrollPos += playerSpeed * deltaTime * (1.0f / 5.0f);
            if (scrollPos > bkgImg.Width) scrollPos = (scrollPos % bkgImg.Width);
        }

        /// <summary>
        /// Draws the background
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            int curScroll = Convert.ToInt32(Math.Round(scrollPos)); //Drawing must use an int, so we round scrollpos

            Rectangle sourceRect1 = new Rectangle(curScroll, 0, bkgImg.Width - curScroll, bkgImg.Height);
            Rectangle sourceRect2 = new Rectangle(0, 0, curScroll, bkgImg.Height);

            Rectangle destRect1 = new Rectangle(0, 0, sourceRect1.Width, GameplayVars.WinHeight);
            Rectangle destRect2 = new Rectangle(sourceRect1.Width, 0, sourceRect2.Width, GameplayVars.WinHeight);

            spriteBatch.Draw(bkgImg, destRect1, sourceRect1, Color.White);
            spriteBatch.Draw(bkgImg, destRect2, sourceRect2, Color.White);
        }
    }
}
