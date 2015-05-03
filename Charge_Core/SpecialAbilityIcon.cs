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
        private Texture2D cooldownTex;

        private float totalCooldown;
        private float globalCooldown;

        public SpecialAbilityIcon(Rectangle pos, Texture2D tex, Texture2D cooldownTex)
        {
            base.init(pos, tex);
            this.cooldownTex = cooldownTex;

            totalCooldown = 0;
            globalCooldown = 0;
        }

        public override void Update(float deltaTime, float playerSpeed, float objectSpeed = 0.0f)
        {
            //Do nothing.
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch); //Draw icon
            if(totalCooldown > 0) {
                int height = Convert.ToInt32(Math.Ceiling(position.Height * globalCooldown / totalCooldown));
                Rectangle cooldownPos = new Rectangle(position.X, position.Bottom - height, position.Width, height);
                spriteBatch.Draw(cooldownTex, cooldownPos, new Color(100, 100, 100) * 0.7f);
            }
        }

        public void SetCooldown(float total, float global)
        {
            totalCooldown = total;
            globalCooldown = global;
        }
    }
}
