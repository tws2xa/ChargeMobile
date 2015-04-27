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
    //More methods and fields may be added later
    class Projectile : WorldEntity
    {
        float moveSpeed; //Movement speed

        /// <summary>
        /// Create the projectile with position and sprite
        /// </summary>
        public Projectile(Rectangle position, Texture2D tex, float moveSpeed)
        {
            this.moveSpeed = moveSpeed;
            base.init(position, tex);
        }

        /// <summary>
        /// Override update to allow for bullet movement
        /// </summary>
        public override void Update(float deltaTime)
        {
            this.position.X += Convert.ToInt32(moveSpeed);

            if (this.CheckOffLeftSideOfScreen() || this.position.X > GameplayVars.WinWidth + 10)
            {
                destroyMe = true;
            }
        }

    }
}
