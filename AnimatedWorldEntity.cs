using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Charge
{
    class AnimatedWorldEntity : WorldEntity
    {
        // Animation variables
        private Rectangle spriteSrcRect;
        private double timeElapsedSinceLastFrameUpdate; // The cumulative time that has passed since the animation frame was last updated
        private int currentFrameNum;
        private int frameWidth;
        private double frameTime;
        private int numAnimationFrames;

        private bool shouldCallBaseUpdate; // Should the WorldEntity Update funciton be called? E.g. should this object move left across the screen as the player moves

        public AnimatedWorldEntity(Rectangle position, Texture2D tex, double frameTime, int numAnimationFrames, bool shouldCallBaseUpdate = true)
        {
            base.init(position, tex);

            this.frameTime = frameTime;
            this.numAnimationFrames = numAnimationFrames;
            this.shouldCallBaseUpdate = shouldCallBaseUpdate;

            spriteSrcRect = new Rectangle();
            timeElapsedSinceLastFrameUpdate = 0;
            currentFrameNum = 0;
            frameWidth = tex.Width / this.numAnimationFrames;
        }

        /// <summary>
        /// Updates the animation frame
        /// </summary>
        public override void Update(float deltaTime)
        {
            if (shouldCallBaseUpdate)
            {
                base.Update(deltaTime);
            }

            timeElapsedSinceLastFrameUpdate += deltaTime;

            if (timeElapsedSinceLastFrameUpdate > frameTime)
            {
                currentFrameNum = (currentFrameNum + 1) % numAnimationFrames;
                spriteSrcRect = new Rectangle(currentFrameNum * frameWidth, 0, frameWidth, tex.Height);

                timeElapsedSinceLastFrameUpdate -= frameTime;
            }
        }

        /// <summary>
        /// Draws the player animation
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(tex, position, spriteSrcRect, Color.White);
        }
    }
}
