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
        // Variables for platform content generation
        private int distanceSinceGeneration; // Distance since the last time that the charge orb spawning on each tier was decided
        private int tierWithNoChargeOrbs; // Tier that spawns no charge orbs
        private int tierWithSomeChargeOrbs; // Tier that spawns at least one charge orb per platform

        // Platform textures
        Texture2D platformLeftTex;
        Texture2D platformCenterTex;
        Texture2D platformRightTex;

        //The right most platforms in each tier
        private Platform[] rightMostInTiers;

        //Used for generating random variables
        private Random rand;

        /// <summary>
        /// Craete the level generator, assumes no platforms
        /// </summary>
        public LevelGenerator(Texture2D pLeftTex, Texture2D pCenterTex, Texture2D pRightTex)
        {
            rightMostInTiers = new Platform[3];
            rightMostInTiers[0] = null;
            rightMostInTiers[1] = null;
            rightMostInTiers[2] = null;
            rand = new Random();

            distanceSinceGeneration = 0;
            tierWithNoChargeOrbs = 0;
            tierWithSomeChargeOrbs = 1;

            platformLeftTex = pLeftTex;
            platformCenterTex = pCenterTex;
            platformRightTex = pRightTex;
        }

        /// <summary>
        /// Updates any changing level generation variables
        /// Like the minimum and maximum allowed spacing between platforms
        /// Based on player speed
        /// </summary>
        /// <param name="deltaTime">Time passed since last frame</param>
        public void Update(float deltaTime, float playerSpeed)
        {
            // Update distance since last tier charge orb decision
            distanceSinceGeneration += (int)(deltaTime * playerSpeed);
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
        public List<Platform> GenerateNewPlatforms(int curNumPlatforms, Color platColor)
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
                    Platform nextPlat = GenerateNewPlatform(rightMost, i, height, platColor);

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

        private List<Platform> GenerateNewPlatformsForTutorial(int curNumPlatforms, Color platColor, int lastPlatformRight)
        {
            List<Platform> newPlatforms = new List<Platform>();

            if (curNumPlatforms < 3)
            {
                Platform p = new Platform(new Rectangle(lastPlatformRight, LevelGenerationVars.Tier3Height, GameplayVars.WinWidth, LevelGenerationVars.PlatformHeight), platformLeftTex, platformCenterTex, platformRightTex, platColor);
                newPlatforms.Add(p);
            }

            return newPlatforms;
        }

        /// <summary>
        /// Generates new level content
        /// </summary>
        public void GenerateLevelContent(float barrierSpeed, float playerSpeed, Color currentPlatformColor, ref List<WorldEntity> platforms, ref List<WorldEntity> enemies, ref List<WorldEntity> batteres, ref List<WorldEntity> walls)
        {
            //Get the new platforms
            List<Platform> newPlatforms = GenerateNewPlatforms(platforms.Count, currentPlatformColor);

            //Add each platform to the list of platforms
            //And generates items to go above each platform
            foreach (Platform platform in newPlatforms)
            {
                platforms.Add(platform);
                GeneratePlatformContents(platform, barrierSpeed, playerSpeed, ref enemies, ref batteres, ref walls);
            }
        }

        /// <summary>
        /// Generates new level content for Tutorial Levels. This should just create a world with a single platform on the bottom tier.
        /// </summary>
        public void GenerateLevelContentForTutorial(Color currentPlatformColor, ref List<WorldEntity> platforms)
        {
            // Find the right side of the most recent platform. This will be used to create a new platform if necessary
            int lastPlatformRight = platforms[platforms.Count - 1].position.Right;

            //Get the new platforms
            List<Platform> newPlatforms = GenerateNewPlatformsForTutorial(platforms.Count, currentPlatformColor, lastPlatformRight);

            //Add each platform to the list of platforms
            foreach (Platform platform in newPlatforms)
            {
                platforms.Add(platform);
            }
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
        private Platform GenerateNewPlatform(Platform rightMost, int tierNum, int tierHeight, Color platColor)
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
                platformLeftTex, platformCenterTex, platformRightTex, platColor);
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

        /// <summary>
        /// Generates the items to go above the given platform,
        /// Like walls, enemies, and batteries, and adds them
        /// To the world
        /// </summary>
        /// <param name="platform">Platform for which to generate content.</param>
        private void GeneratePlatformContents(Platform platform, float barrierSpeed, float playerSpeed, ref List<WorldEntity> enemies, ref List<WorldEntity> batteries, ref List<WorldEntity> walls)
        {
            //The number of sections in the platform
            int numSections = platform.sections.Count;

            int numWalls = 0;
            int numEnemies = 0;
            int numBatteries = 0;

            //Check whether the charge orb spawning per tier should change
            if (distanceSinceGeneration > GameplayVars.WinWidth / 2)
            {
                distanceSinceGeneration = 0;
                int orbRoll = rand.Next(0, 6);
                switch (orbRoll)
                {
                    case 0:
                        tierWithNoChargeOrbs = 0;
                        tierWithSomeChargeOrbs = 1;
                        break;
                    case 1:
                        tierWithNoChargeOrbs = 0;
                        tierWithSomeChargeOrbs = 2;
                        break;
                    case 2:
                        tierWithNoChargeOrbs = 1;
                        tierWithSomeChargeOrbs = 0;
                        break;
                    case 3:
                        tierWithNoChargeOrbs = 1;
                        tierWithSomeChargeOrbs = 2;
                        break;
                    case 4:
                        tierWithNoChargeOrbs = 2;
                        tierWithSomeChargeOrbs = 0;
                        break;
                    case 5:
                        tierWithNoChargeOrbs = 2;
                        tierWithSomeChargeOrbs = 1;
                        break;
                    default:
                        Console.WriteLine("Error in charge orb generation");
                        break;
                }
            }
            int necessaryOrbLocation = -1;
            if (platform.getTier() == tierWithSomeChargeOrbs)
            {
                necessaryOrbLocation = rand.Next(0, numSections);
            }
            //Check whether or not to add somthing to each section
            for (int i = 0; i < numSections; i++)
            {
                int roll = rand.Next(0, LevelGenerationVars.SectionContentRollNum);

                int sectionCenter = platform.sections[i].position.Center.X;

                int batteryRollRange = LevelGenerationVars.BatterySpawnRollRange;

                float multiplier = 1;
                if (barrierSpeed > 0)
                {
                    float playerBarrierSpeedDiff = playerSpeed - barrierSpeed;
                    multiplier = playerBarrierSpeedDiff / barrierSpeed;
                }
                batteryRollRange -= Convert.ToInt32(LevelGenerationVars.MaxBatteryVariation * multiplier);

                //Either a battery is necessary or the roll results in battery spawning and a battery can spawn on that tier
                if ((i == necessaryOrbLocation) || (roll < batteryRollRange && numBatteries < LevelGenerationVars.MaxBatteriesPerPlatform))
                    if (platform.getTier() != tierWithNoChargeOrbs)
                    {
                        //Spawn Battery
                        int width = LevelGenerationVars.BatteryWidth;
                        int height = LevelGenerationVars.BatteryHeight;
                        WorldEntity battery = new WorldEntity(new Rectangle(sectionCenter - width / 2, platform.position.Top - height / 2 - GameplayVars.StartPlayerHeight / 3, width, height), ChargeMain.BatteryTex);
                        batteries.Add(battery);
                        platform.sections[i].containedObj = PlatformSection.BATTERYSTR;
                        numBatteries++;
                    }
                    else if (roll < batteryRollRange + LevelGenerationVars.WallSpawnFrequency && numWalls < LevelGenerationVars.MaxWallsPerPlatform)
                    {
                        //Spawn Wall (takes up two platform spaces)
                        if (i >= numSections - 1) continue; //Need two sections

                        int width = LevelGenerationVars.WallWidth;
                        int height = LevelGenerationVars.WallHeight;
                        WorldEntity wall = new WorldEntity(new Rectangle(platform.sections[i].position.Right - width / 2, platform.position.Top - height + 3, width, height), ChargeMain.WallTex);
                        walls.Add(wall);
                        platform.sections[i].containedObj = PlatformSection.WALLSTR;
                        platform.sections[i + 1].containedObj = PlatformSection.WALLSTR;
                        numWalls++;
                        i++; //Took up an extra section
                    }
                    else if (roll < batteryRollRange + LevelGenerationVars.WallSpawnFrequency + LevelGenerationVars.EnemySpawnFrequency
                        && numEnemies < LevelGenerationVars.MaxEnemiesPerPlatform && enemies.Count < LevelGenerationVars.MaxNumEnemiesTotal)
                    {
                        //Enemy needs at least one place to which he/she may walk.
                        bool hasRoom = false;
                        if ((i > 0) && (platform.sections[i - 1].containedObj == null || platform.sections[i - 1].containedObj == PlatformSection.BATTERYSTR)) hasRoom = true;
                        else if ((i < numSections - 1) && (platform.sections[i + 1].containedObj == null || platform.sections[i + 1].containedObj == PlatformSection.BATTERYSTR)) hasRoom = true;
                        if (!hasRoom) continue;

                        //Spawn Enemy
                        int width = LevelGenerationVars.EnemyWidth;
                        int height = LevelGenerationVars.EnemyHeight;
                        Enemy enemy = new Enemy(new Rectangle(sectionCenter - width / 2, platform.position.Top - height, width, height), ChargeMain.EnemyTex, platform);
                        enemies.Add(enemy);
                        numEnemies++;
                    }
            }
        }
    }
}
