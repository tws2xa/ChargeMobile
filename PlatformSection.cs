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
    class PlatformSection : WorldEntity
    {

        public string containedObj = null; //Object stored above the section

        public static string WALLSTR = "wall";
        public static string BATTERYSTR = "battery";

        Color tint;
        
        public PlatformSection(Rectangle position, Texture2D tex, Color tint)
        {
            this.tint = tint;
            base.init(position, tex);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, 1);
        }

        public void Draw(SpriteBatch spriteBatch, float brightness)
        {
            /*float variationAmt = 255 - GameplayVars.MinPlatformBrightness;
            float toSub =  variationAmt - variationAmt * brightness;

            int R = Convert.ToInt32(tint.R - toSub);
            int G = Convert.ToInt32(tint.G - toSub);
            int B = Convert.ToInt32(tint.B - toSub);

            Color drawCol = new Color(R, G, B);

            spriteBatch.Draw(tex, position, drawCol);*/
            spriteBatch.Draw(tex, position, tint);

            //base.Draw(spriteBatch);
        }
    }
}
