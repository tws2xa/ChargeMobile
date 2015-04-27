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
    class SpecialAbilityIcon : WorldEntity
    {

        Texture2D cooldownTex;

        public SpecialAbilityIcon(Rectangle pos, Texture2D tex, Texture2D cooldownTex)
        {
            base.init(pos, tex);
            this.cooldownTex = cooldownTex;
        }

        public override void Update(float deltaTime)
        {
            //Do nothing.
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch); //Draw icon
            if(ChargeMain.GetTotalCooldown() > 0) {
                int height = Convert.ToInt32(Math.Ceiling(position.Height * ChargeMain.GetGlobalCooldown() / ChargeMain.GetTotalCooldown()));
                Rectangle cooldownPos = new Rectangle(position.X, position.Bottom - height, position.Width, height);
                spriteBatch.Draw(cooldownTex, cooldownPos, new Color(100, 100, 100) * 0.7f);
            }
        }

    }
}
