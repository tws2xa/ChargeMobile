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

    enum OverchargeState { OFF, INCREASING, DECREASING };

    //More methods and fields may be added later
    class Player : AnimatedWorldEntity
    {
        private static readonly double FrameTime = 0.1; // How long to wait before switching to the next animation frame
        private static readonly int NumAnimationFrames = 6; // How many frames are included in the sprite strip

		public float vSpeed;
		public bool grounded;
        public int jmpNum;
        public bool isDead;

        private float overcharge = 0;
        private float playerChargeLevel; // Current charge
        private OverchargeState overchargeState;

        // Animation variables
        private Rectangle spriteSrcRect;
        private double timeElapsedSinceLastFrameUpdate; // The cumulative time that has passed since the animation frame was last updated
        private int currentFrameNum;
        private int frameWidth;

		/// <summary>
		/// Create the player with position and sprite
		/// </summary>
		public Player(Rectangle position, Texture2D tex) : base(position, tex, FrameTime, NumAnimationFrames, false)
        {
            vSpeed = 0;
            jmpNum = 0;
            grounded = false;
            isDead = false;

            spriteSrcRect = new Rectangle();
            timeElapsedSinceLastFrameUpdate = 0;
            currentFrameNum = 0;
            frameWidth = tex.Width / NumAnimationFrames;

            SetCharge(2 * GameplayVars.ChargeBarCapacity / 3);	// Init the player charge level to half of the max
        }

        /// <summary>
        /// Change update to allow for player movement
        /// </summary>
        public override void Update(float deltaTime, float playerSpeed, float objectSpeed = 0.0f)
        {
            base.Update(deltaTime, playerSpeed); // Updates animation frames

            if (overchargeState == OverchargeState.INCREASING)
            {
                overcharge += GameplayVars.OverchargeIncAmt * deltaTime;
                IncCharge(GameplayVars.OverchargePermanentAddAmt * deltaTime);
                if (overcharge >= GameplayVars.OverchargeMax)
                {
                    overcharge = GameplayVars.OverchargeMax;
                    overchargeState = OverchargeState.DECREASING;
                }
            }
            else if (overchargeState == OverchargeState.DECREASING)
            {
                overcharge -= GameplayVars.OverchargeDecAmt * deltaTime;
                if (overcharge <= 0)
                {
                    overcharge = 0;
                    overchargeState = OverchargeState.OFF;
                }
            }

            if (!grounded)
            {
                vSpeed += GameplayVars.Gravity * deltaTime;
                //Cap speed
                vSpeed = Math.Min(GameplayVars.maxPlayerVSpeed, Math.Max(-1 * GameplayVars.maxPlayerVSpeed, vSpeed));
                position.Y += Convert.ToInt32(Math.Round(vSpeed));
            }
            else
            {
                vSpeed = 0;
            }
        }

        /// <summary>
        /// Handles potential platform collision
        /// </summary>
        /// <param name="plat">Platform with which the player may collide</param>
        public bool CheckPlatformCollision(Platform plat)
        {
            if (HitPlatform(plat))
            {
                if (vSpeed > 0) this.position.Y = plat.position.Y - this.position.Height;
                grounded = true;
                jmpNum = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the player hits the top of a platform
        /// </summary>
        /// <param name="plat">Platform with which the player may collide</param>
        /// <returns>True if collides. False otherwise.</returns>
        public bool HitPlatform(Platform plat)
        {
            //We Check if the difference in X vals is less than the width of the left-most object
            int width = this.position.Width;
            if (plat.position.X < this.position.X) width = plat.position.Width;
            if (Math.Abs(this.position.X - plat.position.X) < width)
            {
                //Check if the player is going down and the distance between the bottom
                //of the player and the top of the platform is greater than the player's vSpeed.
                return (this.vSpeed >= 0 && Math.Abs(this.position.Bottom - plat.position.Top) <= Math.Abs(this.vSpeed));
            }
            return false;
        }

        
        public bool CheckWallCollision(WorldEntity wall)
        {
            if (HitWall(wall))
            {
                return true;
            }
            return false;
        }

        public bool HitWall(WorldEntity wall)
        {
            //Wall collision. The player must be roughly 10px of the visible sprite into the wall for a hit.
            //Checks all non collision conditions
            bool hit = true;      
            if(this.position.X + this.position.Width - GameplayVars.PlayerXBuffer < wall.position.X ||
               wall.position.X + wall.position.Width - GameplayVars.wallXBuffer   < this.position.X ||
               this.position.Y < wall.position.Y - wall.position.Height + GameplayVars.wallYBuffer  ||
               wall.position.Y < this.position.Y - this.position.Height + GameplayVars.PlayerYBuffer)
            {
                hit = false;
            }
   
            return hit;
        }

        public bool CheckEnemyCollision(WorldEntity enemy)
        {
            if (HitEnemy(enemy))
            {
                return true;
            }
            return false;
        }

        public bool HitEnemy(WorldEntity enemy)
        {
            //Enemy collision. Checks all non collsion conditions
            bool hit = true;
            if (this.position.X + this.position.Width - GameplayVars.PlayerXBuffer < enemy.position.X ||
               enemy.position.X + enemy.position.Width - GameplayVars.enemyXBuffer < this.position.X  ||
               this.position.Y < enemy.position.Y - enemy.position.Height  ||
               enemy.position.Y < this.position.Y)
            {
                hit = false;
            }

            return hit;
        }

        /// <summary>
        /// Returns the total player charge (includes overcharge)
        /// </summary>
        public float GetCharge()
        {
            return playerChargeLevel + overcharge;
        }

        /// <summary>
        /// Increase player charge by given amount
        /// </summary>
        public void IncCharge(float amt)
        {
            SetCharge(playerChargeLevel + amt);
        }

        /// <summary>
        /// Decrease player charge by given amount
        /// </summary>
        public void DecCharge(float amt)
        {
            SetCharge(playerChargeLevel - amt);
        }

        /// <summary>
        /// Set the player charge to a given amount
        /// </summary>
        public void SetCharge(float val)
        {
            playerChargeLevel = Math.Max(0, val);
        }

        /// <summary>
        /// Handle overcharge charge effects
        /// </summary>
        public void Overcharge()
        {
            overchargeState = OverchargeState.INCREASING;
        }

        /// <summary>
        /// Change the overcharge charge
        /// </summary>
        public void IncOverchargeCharge(int amt)
        {
            overcharge += amt;
        }

        public bool OverchargeActive()
        {
            return !(overchargeState == OverchargeState.OFF);
        }
    }
}
