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
    class Barrier : AnimatedWorldEntity
    {
        private static readonly double FrameTime = 0.075; // How long to wait before switching to the next animation frame
        private static readonly int NumAnimationFrames = 5; // How many frames are included in the sprite strip

        public bool doPixelEffect = false;

        private PixelEffect pixelEffect;
        private double timeElapsedSinceLastMovement;

        /// <summary>
        /// Create the barrier with position and sprite
        /// </summary>
        public Barrier(Rectangle position, Texture2D tex, Texture2D pixelTex) : base(position, tex, FrameTime, NumAnimationFrames, true)
        {
			timeElapsedSinceLastMovement = 0;

            if (doPixelEffect)
            {
                int pixelWidth = position.Width / 3;
                Rectangle pixelRect = new Rectangle(position.X + position.Width / 2 - pixelWidth / 2, position.Y, pixelWidth, position.Height);
                pixelEffect = new PixelEffect(pixelRect, pixelTex, new List<Color>() { Color.White, Color.Black });
                pixelEffect.spawnFadeTime = -1;
                pixelEffect.spawnFrequency = 0.2f;
                pixelEffect.pixelYVel = 65;
            }
        }

        /// <summary>
        /// Override update to allow for correct barrier movememnt.
        /// </summary>
        public override void Update(float deltaTime, float playerSpeed, float objectSpeed = 0.0f)
        {
            base.Update(deltaTime, playerSpeed); // Update animation and move with player speed

			timeElapsedSinceLastMovement += deltaTime;

			double movementInPixels = 0;
			if (objectSpeed > 0) // Avoid divide by zero errors
			{
				// Calculate how many full pixels the object should move in the timeElapsedSinceLastMovement interval
				movementInPixels = Math.Floor(timeElapsedSinceLastMovement * objectSpeed);
				timeElapsedSinceLastMovement -= movementInPixels * (1 / objectSpeed);
			}

			this.position.X += Convert.ToInt32(movementInPixels);

            if (doPixelEffect)
            {
                pixelEffect.position.X += Convert.ToInt32(movementInPixels);
                pixelEffect.Update(deltaTime, playerSpeed);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if(doPixelEffect) pixelEffect.Draw(spriteBatch);
        }
    }
}
