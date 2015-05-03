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
    class WorldEntity
    {
		public Texture2D tex; //Sprite for the object
        public Rectangle position; //Object's position in the world
        public bool destroyMe; //Should the object be destroyed

		private double timeElapsedSinceLastMovement;	// How much time has passed since the last movement
		
		//Needed for inheritence
        public WorldEntity() { }

        /// <summary>
        /// Creates a standard world entity
        /// With the given position and sprite
        /// </summary>
        public WorldEntity(Rectangle position, Texture2D tex)
        {
            init(position, tex);
        }

        /// <summary>
        /// Sets up the WorldEntity
        /// With the given position and sprite
        /// </summary>
        public void init(Rectangle position, Texture2D tex) {
            this.position = position;
            this.tex = tex;
            destroyMe = false;

			timeElapsedSinceLastMovement = 0;
        }

        /// <summary>
        /// Draws the object's texture at the object's position
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch) {
           if(tex != null) spriteBatch.Draw(tex, position, Color.White);
        }

        /// <summary>
        /// Move in the opposite direction of the player speed
        /// Thus creating the illusion that the player is moving
        /// </summary>
        public virtual void Update(float deltaTime, float playerSpeed, float objectSpeed = 0.0f)
        {
			timeElapsedSinceLastMovement += deltaTime;

			double movementInPixels = 0;
			if (playerSpeed > 0) // Avoid divide by zero errors
			{
				// Calculate how many full pixels the object should move in the timeElapsedSinceLastMovement interval
				movementInPixels = Math.Floor(timeElapsedSinceLastMovement * playerSpeed);
				timeElapsedSinceLastMovement -= movementInPixels * (1 / playerSpeed);
			}

            this.position.X -= Convert.ToInt32(movementInPixels);

            if (!(this is Barrier))
                PerformScreenBoundsCheck();
        }

        /// <summary>
        /// Checks if the object is off the left side of the screen
        /// And, if so, flags the object to be destroyed
        /// </summary>
        public void PerformScreenBoundsCheck()
        {
            if (CheckOffLeftSideOfScreen()) this.destroyMe = true;
        }

        /// <summary>
        /// Checks if the entity is off the left side of the screen
        /// </summary>
        /// <returns>True if the entity is off of the left side of the screen</returns>
        public bool CheckOffLeftSideOfScreen()
        {
            //Buffer size 10 just in case
            return (position.Right < -10);
        }
    }
}
