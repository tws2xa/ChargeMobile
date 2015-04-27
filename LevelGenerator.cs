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
    class LevelGenerator
    {

        //The right most platforms in each tier
        Platform[] rightMostInTiers;
        //Used for generating random variables
        Random rand;

        /// <summary>
        /// Craete the level generator, assumes no platforms
        /// </summary>
        public LevelGenerator()
        {
            rightMostInTiers = new Platform[3];
            rightMostInTiers[0] = null;
            rightMostInTiers[1] = null;
            rightMostInTiers[2] = null;
            rand = new Random();
        }

        /// <summary>
        /// Updates any changing level generation variables
        /// Like the minimum and maximum allowed spacing between platforms
        /// Based on player speed
        /// </summary>
        /// <param name="deltaTime">Time passed since last frame</param>
        public void Update(float deltaTime)
        {
            //Update min and max spacing for level generation
            //LevelGenerationVars.MinBetweenSpace = Convert.ToInt32(Math.Round(ChargeMain.GetPlayerSpeed() * LevelGenerationVars.SpeedToMinSpaceMultipler));
            //LevelGenerationVars.MaxBetweenSpace = Convert.ToInt32(Math.Round(ChargeMain.GetPlayerSpeed() * LevelGenerationVars.SpeedToMaxSpaceMultipler));
        }

        /// <summary>
        /// Set the right most platform of a tier to a new platform
        /// </summary>
        /// <param name="newRight">The new right most platform of the tier</param>
        /// <param name="tier">The tier in which to add it.</param>
        public void SetRightMost(Platform newRight, int tier)
        {
            if ( (tier < 0) || (tier > (rightMostInTiers.Length - 1)) ) return; //Bounds check

            //Check if the new value is more left than the current right-most.
            //If so, provide warning but still procceed.
            if (rightMostInTiers[tier] != null && rightMostInTiers[tier].position.X > newRight.position.X)
            {
                Console.WriteLine("Warning: Setting Right-Most Platform in Tier " + tier + " to a Platform Left of the Current Right-Most.");
            }
            rightMostInTiers[tier] = newRight;
        }

        /// <summary>
        /// Handles the removal of a platform from the level
        /// </summary>
        /// <param name="entity">The platform that was removed</param>
        internal void PlatformRemoved(Platform entity)
        {
            //Check if it was the right most platform in a tier
            //If so, set the right most platform to null
            for (int j = 0; j < rightMostInTiers.Length; j++)
            {
                if (entity == rightMostInTiers[j]) rightMostInTiers[j] = null;
            }
        }


        /// <summary>
        /// Generates new platforms for the level
        /// </summary>
        /// <param name="curNumPlatforms">The current number of platforms in the world</param>
        /// <param name="PlatformLeftTex">Texture for generated left cap platform sections</param>
        /// <param name="PlatformCenterTex">Texture for generated center platform sections</param>
        /// <param name="PlatformRightTex">Texture for generated right cap platform sections</param>
        /// <returns>List of newly generated platforms</returns>
        public List<Platform> GenerateNewPlatforms(int curNumPlatforms, Texture2D PlatformLeftTex, Texture2D PlatformCenterTex, Texture2D PlatformRightTex, Color platColor)
        {
            //The list of newly generated platforms
            List<Platform> newPlatforms = new List<Platform>();

            //Check if it should create a new platform for each tier
            for (int i = 0; i < rightMostInTiers.Length; i++)
            {
                Platform rightMost = rightMostInTiers[i];
                if (ShouldSpawnPlatform(rightMost, curNumPlatforms, i))
                {
                    //Find the new platforms's spawn height
                    int height = LevelGenerationVars.Tier1Height;
                    if (i == 1) height = LevelGenerationVars.Tier2Height;
                    if (i == 2) height = LevelGenerationVars.Tier3Height;

                    //Make the new platform
                    Platform nextPlat = GenerateNewPlatform(rightMost, i, height, PlatformLeftTex, PlatformCenterTex, PlatformRightTex, platColor);

                    //Update the right most platform in the tier
                    //(Which is now the just created platform)
                    SetRightMost(nextPlat, i);

                    //Add to the list of ground pieces
                    newPlatforms.Add(nextPlat);
                    
                    //Increment the number of platforms
                    curNumPlatforms++;
                }
            }

            return newPlatforms;
        }

        /// <summary>
        /// Checks if it should spawn a new platform in the tier
        /// of the given platform
        /// </summary>
        /// <param name="rightMostInTier">The current, right most platform in the tier</param>
        /// <returns>True if a new platform should be created</returns>
        private bool ShouldSpawnPlatform(Platform rightMostInTier, int curNumPlatforms, int tierNum)
        {
            //Do not exceeed the maximum number of ground pieces
            if (curNumPlatforms > LevelGenerationVars.MaxGroundPieces) return false;

            //If the row is empty, make a new platform
            if (rightMostInTier == null) return true;

            //If the row is about to exceed the maximum between distance, make a new platform
            if (rightMostInTier.position.Right <= (GameplayVars.WinWidth - LevelGenerationVars.MaxBetweenSpaces[tierNum])) return true;

            if(rightMostInTier.position.Right > GameplayVars.WinWidth * 2) return false;

            //Randomly spawn
            return (rand.NextDouble() < LevelGenerationVars.PlatformSpawnFreq);
        }

        /// <summary>
        /// Generates a new platform following the given platform
        /// At the given tier height
        /// </summary>
        /// <param name="rightMost">Right most platform of the tier in which to add the platform</param>
        /// <param name="tierHeight">The height of the tier</param>
        /// <returns>The newly generated platform</returns>
        private Platform GenerateNewPlatform(Platform rightMost, int tierNum, int tierHeight, Texture2D PlatformLeftTex, Texture2D PlatformCenterTex, Texture2D PlatformRightTex, Color platColor)
        {
            //Must spawn off the right side of the screen (at a minimum)
            int minX = GameplayVars.WinWidth;

            //Make sure it's at least the minimum distance from the previous platform in the tier
            if (rightMost != null &&
                minX < (rightMost.position.Right + LevelGenerationVars.MinBetweenSpaces[tierNum]))
            {
                minX = rightMost.position.Right + LevelGenerationVars.MinBetweenSpaces[tierNum];
            }

            //Can't go over the maximum space between platforms
            int maxX = minX + LevelGenerationVars.MaxBetweenSpaces[tierNum];
            if (rightMost != null)
            {
                maxX = rightMost.position.Right + LevelGenerationVars.MaxBetweenSpaces[tierNum];
            }

            int spawnX = -1;

            if (minX > maxX) spawnX = minX; //If the minimum is less than the maximum, just spawn at the minimum.
            else spawnX = rand.Next(minX, maxX); //Randomly decide on new location.

            //Calculate random size
            int width = LevelGenerationVars.SegmentWidth * rand.Next(LevelGenerationVars.MinNumSegments, LevelGenerationVars.MaxNumSegments);

            Platform newPlatform = new Platform(new Rectangle(spawnX, tierHeight, width, LevelGenerationVars.PlatformHeight),
                PlatformLeftTex, PlatformCenterTex, PlatformRightTex, platColor);
            return newPlatform;
        }

        /// <summary>
        /// Resets the level generation
        /// </summary>
        internal void Reset()
        {
            rightMostInTiers[0] = null;
            rightMostInTiers[1] = null;
            rightMostInTiers[2] = null;
        }
    }
}
