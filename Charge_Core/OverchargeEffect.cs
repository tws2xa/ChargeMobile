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
    class OverchargeEffect : WorldEntity
    {

        float opacity; //Fade over time
        float tmpWidth; //Maintain as  a float, since position only uses ints
        Player player; //Keep track of player's positions
        
        public OverchargeEffect(Rectangle startPos, Texture2D tex, Player player)
        {
            this.tex = tex;
            this.position = startPos;
            tmpWidth = position.Width;
            opacity = 1.0f;
            this.player = player;
        }

        public override void Update(float deltaTime, float playerSpeed, float objectSpeed = 0.0f)
        {
            opacity -= deltaTime/2.0f;

            if (Math.Abs(player.position.Y - position.Y) > player.position.Height)
            {
                //Fade more quickly when the player moves away
                //It looked weird when the player was on a different
                //tier and there were still a bunch of lines below/above him/her
                opacity -= 2 * deltaTime;
            }

            if (this.opacity <= 0)
            {
                opacity = 0;
                this.destroyMe = true;
            }

            //tmpWidth += ChargeMain.GetPlayerSpeed() / 2.0f * deltaTime;
            position.Width = Convert.ToInt32(tmpWidth);

            base.Update(deltaTime, playerSpeed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(tex, position, Color.White * opacity);
        }

    }
}
