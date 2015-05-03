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
    class ChargeBlast : WorldEntity
    {
        /// <summary>
        /// Create the ChargeBlast with position and sprite
        /// </summary>
        public ChargeBlast(Rectangle position, Texture2D tex)
        {
            base.init(position, tex);
        }

        /// <summary>
        /// Override update so that blast can radiate outwards
        /// </summary>
        public override void Update(float deltaTime, float playerSpeed, float objectSpeed = 0.0f)
        {
            
        }
    }
}
