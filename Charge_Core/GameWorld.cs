using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace Charge
{
    class GameWorld
    {
        // WorldEntities
        private Player player; //The player character
        private List<WorldEntity> platforms; //All platforms in game
        private List<WorldEntity> enemies; //All enemies in game
        private List<WorldEntity> projectiles; //All bullets in game
        private List<WorldEntity> walls; //All walls in the game
        private List<WorldEntity> batteries; //All batteries in the game
        private List<WorldEntity> otherEnts; //Other objects, like effects
        private Barrier backBarrier; //The death barrier behind the player
        private Barrier frontBarrier; //The death barrier in front of the player
        private Background background; //The scrolling backdrop

        // Score variables
        private int score; //Player score
        private float tempScore; //Keeps track of fractional score increases

        // Cooldown variables
        private float globalCooldown; //The cooldown on powerups
        private float totalGlobalCooldown; //The max from which the cooldown is decreasing

        // Variables for generating platform contents
        private int distanceSinceGeneration; // Distance since the last time that the charge orb spawning on each tier was decided
        private int tierWithNoChargeOrbs; // Tier that spawns no charge orbs
        private int tierWithSomeChargeOrbs; // Tier that spawns at least one charge orb per platform
        
        // Barrier variables
        private float playerSpeed; //Current run speed
        private float barrierSpeed; //Speed of barriers
        private int glowWidth;
        private int glowHeight;

        //Useful Tools
        private Random rand; //Used for generating random variables
        private LevelGenerator levelGenerator; //Generates the platforms

        // Sound variables
        private bool playLandSound;
        private float masterVolume;

        // Function pointers for when a WorldEntity is removed from the world
        private Action<WorldEntity> destroyWall;
        private Action<WorldEntity> destroyEnemy;
        private Action<WorldEntity> destroyPlatform;

        // Game state values
        private bool isGameOver;

        public GameWorld()
        {
            InitializeGeneralVariables();
        }
        
        /// <summary>
        /// Initializes the Game World variables for the given GameState.
        /// </summary>
        /// <param name="gameState">GameState for which to set up the Game World</param>
        public void InitializeStateSpecificVariables(ChargeMain.GameState gameState)
        {
            ResetAllLists(); // Always clear all entity lists when reinitializing

            switch (gameState)
            {
                case ChargeMain.GameState.InGame:
                    InitializeInGameState();
                    break;
                case ChargeMain.GameState.TitleScreen:
                case ChargeMain.GameState.OptionsScreen:
                case ChargeMain.GameState.CreditsScreen:
                    InitializeMenuScreenState();
                    break;
                case ChargeMain.GameState.TutorialJump:
                    break;
                case ChargeMain.GameState.TutorialDischarge:
                    break;
                case ChargeMain.GameState.TutorialShoot:
                    break;
                case ChargeMain.GameState.TutorialOvercharge:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Initializes the Game World variables for the given GameState.
        /// </summary>
        /// <param name="deltaTime">Time since last update in seconds</param>
        /// <param name="gameState">The current GameState</param>
        public void Update(float deltaTime, ChargeMain.GameState gameState)
        {
            // Handles updates that are needed when the screen is scrolling (i.e. the game is not paused and the player is not dead)
            if (ShouldScrollScreen(gameState))
            {
                // Update the background scroll
                background.Update(deltaTime, playerSpeed);

                // Update all entities in the world
                UpdateAllWorldEntities(deltaTime);

                // Update distance since last tier charge orb decision
                distanceSinceGeneration += (int)(deltaTime * playerSpeed);

                // Generate new platforms
                levelGenerator.Update(deltaTime);
                GenerateLevelContent();
            }

            // Handles updates specific to the InGame gamestate
            if (gameState == ChargeMain.GameState.InGame)
            {
                // Update the player's position
                player.Update(deltaTime, playerSpeed);

                //Play the land sound if they player has jumped or fallen
                if (!playLandSound && Math.Abs(player.vSpeed) > Math.Abs(GameplayVars.JumpInitialVelocity / 2))
                {
                    playLandSound = true;
                }

                // Update the barriers
                barrierSpeed += GameplayVars.BarrierSpeedUpRate * deltaTime;
                frontBarrier.Update(deltaTime, playerSpeed, barrierSpeed);
                backBarrier.Update(deltaTime, playerSpeed, barrierSpeed);

                UpdateScore(deltaTime); //Update the player score
                CheckCollisions(); //Check for any collisions
                UpdatePlayerCharge(deltaTime); // Decrements the player charge, given the amount of time that has passed
                UpdatePlayerSpeed(); // Updates the player speed based on the current charge
                UpdateCooldown(deltaTime); //Update the global cooldown
                UpdateEffects(deltaTime); //Handle effects for things like Overcharge, etc
            }
            else if (gameState == ChargeMain.GameState.GameOver)
            {
                // The only thing that needs to be updated is the effects, so that they can end even after the player loses the game.
                UpdateWorldEntityList(ref otherEnts, deltaTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw background
            background.Draw(spriteBatch);

            // Draw all of the world entities
            DrawWorldEntityList(walls, spriteBatch);
            DrawPlatforms(spriteBatch); // Platforms are a special case because their brightness changes with the charge level
            DrawWorldEntityList(enemies, spriteBatch);
            DrawWorldEntityList(batteries, spriteBatch);
            DrawWorldEntityList(otherEnts, spriteBatch);
            DrawWorldEntityList(projectiles, spriteBatch);

            //Draw the player
            if (player != null && !isGameOver)
            {
                player.Draw(spriteBatch);
            }
            
            //Draw Barriers
            if (frontBarrier != null)
            {
                frontBarrier.Draw(spriteBatch);
                DrawFrontBarrierWarningGlow(spriteBatch);
            }

            if (backBarrier != null)
            {
                backBarrier.Draw(spriteBatch);
                DrawBackBarrierWarningGlow(spriteBatch);
            }
        }

        /// <summary>
        /// Plays the given sound
        /// </summary>
        /// <param name="sound">The sound to play</param>
        public void PlaySound(SoundEffect sound)
        {
            SoundEffectInstance soundInstance = sound.CreateInstance();
            soundInstance.Volume = masterVolume;

            soundInstance.Play();
        }

        /// <summary>
        /// If the player can jump, causes the player to jump. Otherwise, it does nothing.
        /// </summary>
        public void InitiateJump()
        {
            if (player.jmpNum < GameplayVars.playerNumJmps || player.grounded)
            {
                player.jmpNum++;
                player.vSpeed = GameplayVars.JumpInitialVelocity;
                player.grounded = false;
                PlaySound(ChargeMain.jumpSound);
            }
        }

        /// <summary>
        /// If the player is in the air and the jump command is released early, cut the player's vertical speed in half so that the jump does not go as high.
        /// </summary>
        public void CutJump()
        {
            if (player.vSpeed < 0)
            {
                player.vSpeed /= 2;
            }
        }

        /// <summary>
        /// Launches the overcharge special ability
        /// </summary>
        public void InitiateOvercharge()
        {
            if (globalCooldown > 0)
            {
                return;
            }

            player.Overcharge();
            PlaySound(ChargeMain.overchargeSound);

            SetGlobalCooldown(GameplayVars.OverchargeCooldownTime[GetCurrentLevel() - 1]);
        }


        /// <summary>
        /// Launches the shoot special ability  
        /// </summary>
        public void InitiateShoot()
        {
            if (globalCooldown > 0)
            {
                return;
            }

            player.DecCharge(GameplayVars.ShootCost);

            int bulletWidth = 15;
            int bulletHeight = 8;
            int bulletX = player.position.Right + bulletWidth;
            int bulletY = player.position.Center.Y - bulletHeight / 2 + 5;
            Projectile bullet = new Projectile(new Rectangle(bulletX, bulletY, bulletWidth, bulletHeight), ChargeMain.ChargeBarTex, GameplayVars.BulletMoveSpeed);
            projectiles.Add(bullet);

            SetGlobalCooldown(GameplayVars.ShootCooldownTime[GetCurrentLevel() - 1]);
            PlaySound(ChargeMain.shootSound);
        }

        /// <summary>
        /// Launches the discharge special ability
        /// </summary>
        public void InitiateDischarge()
        {
            if (globalCooldown > 0)
            {
                return;
            }
            if (GameplayVars.DischargeMaxCost < player.GetCharge() * GameplayVars.DischargeCost)
            {
                player.DecCharge(GameplayVars.DischargeCost);
            }
            else
            {
                player.DecCharge(player.GetCharge() * GameplayVars.DischargeCost);
            }

            DischargeAnimation discharge = new DischargeAnimation(new Rectangle(player.position.Left, player.position.Top, player.position.Width, player.position.Width), ChargeMain.DischargeTex, player);
            otherEnts.Add(discharge);

            SetGlobalCooldown(GameplayVars.DischargeCooldownTime[GetCurrentLevel() - 1]);
            PlaySound(ChargeMain.dischargeSound);
        }


        public void SetGlobalCooldown(float cooldown)
        {
            globalCooldown = cooldown;
            totalGlobalCooldown = cooldown;
        }
        
        /// <summary>
        /// Sets the master volume to be used for all game world sounds
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = volume;
        }

        /// <summary>
        /// Returns true if the player has died. Returns false otherwise.
        /// </summary>
        public bool IsGameOver()
        {
            return isGameOver;
        }

        /// <summary>
        /// Returns the current player score.
        /// </summary>
        public int GetScore()
        {
            return score;
        }

        /// <summary>
        /// Returns the current charge level.
        /// </summary>
        public float GetCharge()
        {
            return player.GetCharge();
        }

        /// <summary>
        /// Returns the current number of charge bars that the player has filled up.
        /// </summary>
        public int GetCurrentLevel()
        {
            int level = 0;
            float barrierChargeEquivalent = barrierSpeed / GameplayVars.ChargeToSpeedCoefficient;

            while (barrierChargeEquivalent > 0)
            {
                barrierChargeEquivalent -= GameplayVars.ChargeBarCapacity * GameplayVars.LevelSpeeds[level];
                level++;
            }

            return level;
        }

        /// <summary>
        /// Returns the total cooldown
        /// </summary>
        public float GetTotalCooldown()
        {
            return totalGlobalCooldown;
        }

        /// <summary>
        /// Returns the current cooldown
        /// </summary>
        public float GetGlobalCooldown()
        {
            return globalCooldown;
        }

        /// <summary>
        /// Returns the current player speed
        /// </summary>
        public float GetPlayerSpeed()
        {
            return playerSpeed;
        }

        /// <summary>
        /// Initializes the Game World variables that are common across all GameStates.
        /// </summary>
        private void InitializeGeneralVariables()
        {
            // Initialize tools
            rand = new Random();
            levelGenerator = new LevelGenerator();

            // Initialize general barrier variables
            glowWidth = GameplayVars.WinWidth / 7;
            glowHeight = GameplayVars.WinHeight;

            // Initialize sound variables
            playLandSound = true;
            masterVolume = 0.5f;

            //Initialize all lists
            platforms = new List<WorldEntity>();
            enemies = new List<WorldEntity>();
            projectiles = new List<WorldEntity>();
            walls = new List<WorldEntity>();
            batteries = new List<WorldEntity>();
            otherEnts = new List<WorldEntity>();

            // Initialize WorldEntities
            background = new Background(ChargeMain.BackgroundTex);

            // Initialize function pointers
            destroyEnemy = this.EnemyDestroyed;
            destroyPlatform = this.PlatformDestroyed;
            destroyWall = this.WallDestroyed;

            // Initialize game state values
            isGameOver = false;
        }

        /// <summary>
        /// Clears all entity lists
        /// </summary>
        private void ResetAllLists()
        {
            platforms.Clear();
            enemies.Clear();
            projectiles.Clear();
            walls.Clear();
            batteries.Clear();
            otherEnts.Clear();
        }

        /// <summary>
        /// Draws the warning glow for the front barrier
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to draw to.</param>
        private void DrawFrontBarrierWarningGlow(SpriteBatch spriteBatch)
        {
            float distThreshold = GameplayVars.GlowThreshold;

            float backOpacity = ((distThreshold + backBarrier.position.Center.X) / distThreshold); //Back Barrier pos will usually be negative if barrier is off screen
            if (backOpacity > 1)
            {
                backOpacity = (1.0f / backOpacity);
            }
            backOpacity = Math.Max(0, backOpacity);

            if (backOpacity > 0)
            {
                spriteBatch.Draw(ChargeMain.LeftGlow, new Rectangle(0, 0, glowWidth, glowHeight), Color.White * backOpacity);
            }
        }

        /// <summary>
        /// Draws the warning glow for the back barrier
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to draw to.</param>
        private void DrawBackBarrierWarningGlow(SpriteBatch spriteBatch)
        {
            float distThreshold = GameplayVars.GlowThreshold;

            float frontOffset = (frontBarrier.position.Center.X - GameplayVars.WinWidth);
            float frontOpacity = ((distThreshold - frontOffset) / distThreshold);
            if (frontOpacity > 1)
            {
                frontOpacity = (1.0f / frontOpacity);
            }
            frontOpacity = Math.Max(0, frontOpacity);

            if (frontOpacity > 0)
            {
                spriteBatch.Draw(ChargeMain.RightGlow, new Rectangle(GameplayVars.WinWidth - glowWidth, 0, glowWidth, glowHeight), Color.White * frontOpacity);
            }
        }
        
        /// <summary>
        /// Returns the percentage of the current charge bar that the player has filled up.
        /// </summary>
        private float GetLevelChargePercent()
        {
            float curCharge = player.GetCharge();
            float maxCharge = GameplayVars.ChargeBarCapacity * GetCurrentLevel();

            if (maxCharge - curCharge >= GameplayVars.ChargeBarCapacity) return 0; //A whole level's worth too slow
            if (maxCharge - curCharge <= 0) return 1; //Current charge > max

            curCharge %= GameplayVars.ChargeBarCapacity;

            return (curCharge / (float)GameplayVars.ChargeBarCapacity);
        }

        /// <summary>
        /// Returns the current platform color based on the player's level.
        /// </summary>
        private Color GetCurrentPlatformColor()
        {
            int index = GetCurrentLevel();
            index %= GameplayVars.PlatformLevelColors.Length;
            return GameplayVars.PlatformLevelColors[index];
        }

        /// <summary>
        /// If the game world should scroll left in the current game state, return true. Otherwise, return false.
        /// </summary>
        private bool ShouldScrollScreen(ChargeMain.GameState gameState)
        {
            return gameState != ChargeMain.GameState.Paused && gameState != ChargeMain.GameState.GameOver;
        }

        /// <summary>
        /// Generates new level content
        /// </summary>
        private void GenerateLevelContent()
        {
            //Get the new platforms
            List<Platform> newPlatforms = levelGenerator.GenerateNewPlatforms(platforms.Count, ChargeMain.PlatformLeftTex, ChargeMain.PlatformCenterTex, ChargeMain.PlatformRightTex, GetCurrentPlatformColor());

            //Add each platform to the list of platforms
            //And generates items to go above each platform
            foreach (Platform platform in newPlatforms)
            {
                platforms.Add(platform);
                GeneratePlatformContents(platform);
            }
        }

        /// <summary>
        /// Generates the items to go above the given platform,
        /// Like walls, enemies, and batteries, and adds them
        /// To the world
        /// </summary>
        /// <param name="platform">Platform for which to generate content.</param>
        private void GeneratePlatformContents(Platform platform)
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

        /// <summary>
        /// Updates all entities in the game world, other than the player and the barriers.
        /// </summary>
        /// <param name="deltaTime">The time since last update in seconds.</param>
        private void UpdateAllWorldEntities(float deltaTime)
        {
            UpdateWorldEntityList(ref platforms, deltaTime, destroyPlatform);
            UpdateWorldEntityList(ref batteries, deltaTime);
            UpdateWorldEntityList(ref enemies, deltaTime, destroyEnemy);
            UpdateWorldEntityList(ref walls, deltaTime, destroyWall);
            UpdateWorldEntityList(ref projectiles, deltaTime);
            UpdateWorldEntityList(ref otherEnts, deltaTime);
        }

        /// <summary>
        /// Updates each entity in the given list, and removes it from the GameWorld if necessary.
        /// To the world
        /// </summary>
        /// <param name="list">A reference to a list of entities.</param>
        /// <param name="deltaTime">The time since last update in seconds.</param>
        /// <param name="destroyFunction">A function pointer which will be called if the entity is destroyed.</param>
        private void UpdateWorldEntityList(ref List<WorldEntity> list, float deltaTime, Action<WorldEntity> destroyFunction = null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                WorldEntity entity = list[i];
                list[i].Update(deltaTime, playerSpeed);

                if (list[i].destroyMe)
                {
                    if (destroyFunction != null)
                    {
                        destroyFunction(list[i]);
                    }

                    list.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Called when a platform has been removed from the game world. The level generator needs to be updated to know that this platform no longer exists
        /// </summary>
        private void PlatformDestroyed(WorldEntity platform)
        {
            Platform p = (Platform)platform;
            levelGenerator.PlatformRemoved(p);
        }

        /// <summary>
        /// Called when a enemy has been removed from the game world. A pixel effect needs to be added where the enemy originally was.
        /// </summary>
        private void EnemyDestroyed(WorldEntity enemy)
        {
            Enemy e = (Enemy)enemy;

            List<Color> destroyCols = new List<Color>() { Color.Red, Color.Black };
            PixelEffect pixelEffect = new PixelEffect(e.position, ChargeMain.WhiteTex, destroyCols);
            pixelEffect.yVel = -20;

            otherEnts.Add(pixelEffect);
        }

        /// <summary>
        /// Called when a wall has been removed from the game world. A pixel effect needs to be added where the wall originally was.
        /// </summary>
        private void WallDestroyed(WorldEntity wall)
        {
            List<Color> destroyCols = new List<Color>() { Color.Red, Color.Black };
            PixelEffect pixelEffect = new PixelEffect(wall.position, ChargeMain.WhiteTex, destroyCols);
            
            pixelEffect.followCamera = false;
            pixelEffect.yVel = -10;
            pixelEffect.xVel = -80;
            pixelEffect.SetSpawnFreqAndFade(5, 0.5f);

            otherEnts.Add(pixelEffect);
        }

        /// <summary>
        /// Handle effects for things like OverCharge, etc.
        /// </summary>
        private void UpdateEffects(float deltaTime)
        {
            if (player.OverchargeActive())
            {
                if (rand.NextDouble() < 0.4)
                {
                    int effectWidth = 5;
                    int effectHeight = 5;
                    int effectX = player.position.X - effectWidth;
                    int effectY = player.position.Center.Y - effectHeight / 2;
                    double heightRand = rand.NextDouble();
                    if (heightRand < 0.3)
                    {
                        effectY += player.position.Height / 3;
                    }
                    else if (heightRand > 0.7)
                    {
                        effectY -= player.position.Height / 3;
                    }
                    OverchargeEffect effect = new OverchargeEffect(new Rectangle(effectX, effectY, effectWidth, effectHeight), ChargeMain.ChargeBarTex, player);
                    otherEnts.Add(effect);
                }
            }
        }

        /// <summary>
        /// Update the global cooldown
        /// </summary>
        private void UpdateCooldown(float deltaTime)
        {
            globalCooldown = Math.Max(0, globalCooldown - deltaTime);
            if (globalCooldown < 0 + deltaTime * 2 && globalCooldown > 0)
            {
                PlaySound(ChargeMain.rearmSound);
            }
            if (globalCooldown == 0)
            {
                totalGlobalCooldown = 0;
            }
        }

        /// <summary>
        /// Update the score
        /// </summary>
        private void UpdateScore(float deltaTime)
        {
            tempScore += deltaTime * GameplayVars.TimeToScoreCoefficient;
            //Add to score if tempScore is at least 1
            if (tempScore > 1)
            {
                int addAmt = Convert.ToInt32(Math.Floor(tempScore));
                score += addAmt;
                tempScore -= addAmt;
            }
        }

        /// <summary>
        /// Checks all collisions in the game
        /// </summary>
        private void CheckCollisions()
        {
            CheckPlayerPlatformCollisions();
            CheckPlayerBatteryCollisions();
            CheckPlayerEnemyCollisions();
            CheckPlayerWallCollisions();
            CheckPlayerBarrierCollisions();
            CheckEnemyDischargeBlastCollisions();
            CheckEnemyProjectileCollisions();
        }

        /// <summary>
        /// Checks if the player collided with either barrier
        /// </summary>
        private void CheckPlayerBarrierCollisions()
        {
            if (player.position.Right > frontBarrier.position.Center.X)
            {
                PlayerDeath();
            }
            else if (player.position.Left < backBarrier.position.Center.X)
            {
                PlayerDeath();
            }
            else if (player.position.Top > GameplayVars.WinHeight + 10)
            {
                PlayerDeath();
            }
        }

        /// <summary>
        /// Checks the player against all platforms in the world
        /// </summary>
        private void CheckPlayerPlatformCollisions()
        {
            player.grounded = false;
            foreach (Platform plat in platforms)
            {
                if (plat.position.Left < player.position.Right * 2)
                {
                    bool collided = player.CheckPlatformCollision(plat); //Handles the checking and results of collisions
                    if (collided)
                    {
                        if (playLandSound)
                        {
                            playLandSound = false;
                            PlaySound(ChargeMain.landSound);
                        }
                        break; //Hit a platform. No need to check any more.
                    }
                }
            }
        }

        /// <summary>
        /// Checks the player against all batteries in the world
        /// </summary>
        private void CheckPlayerBatteryCollisions()
        {
            foreach (WorldEntity battery in batteries)
            {
                if (player.position.Intersects(battery.position))
                {
                    player.IncCharge(GameplayVars.BatteryChargeReplenish);
                    battery.destroyMe = true;
                    PlaySound(ChargeMain.chargeCollect);
                    break;
                }
            }
        }

        /// <summary>
        /// Checks if the player is colliding with an enemy. If they are, then the player dies and the game ends.
        /// </summary>
        private void CheckPlayerEnemyCollisions()
        {
            foreach (WorldEntity enemy in enemies)
            {
                if (player.CheckEnemyCollision(enemy))
                {
                    PlayerDeath();
                }
            }
        }

        /// <summary>
        /// Checks if the player is colliding with a wall. If OverCharge is active, then the wall is destroyed. If not, then the player dies and the game ends.
        /// </summary>
        private void CheckPlayerWallCollisions()
        {
            foreach (WorldEntity wall in walls)
            {
                if (player.CheckWallCollision(wall))
                {
                    if (player.OverchargeActive())
                    {
                        wall.destroyMe = true;
                    }
                    else
                    {
                        PlayerDeath();
                    }
                }
            }
        }

        /// <summary>
        /// Kill enemies as the discharge blast collides with them
        /// </summary>
        private void CheckEnemyDischargeBlastCollisions()
        {
            foreach (WorldEntity enemy in enemies)
            {
                foreach (WorldEntity effect in otherEnts)
                {
                    if (effect is DischargeAnimation && ((DischargeAnimation)effect).circle.Intersects(enemy.position))
                    {
                        enemy.destroyMe = true;
                        PlaySound(ChargeMain.enemyDeathSound);
                    }
                }
            }
        }

        /// <summary>
        /// Destroy enemies hit by bullets
        /// </summary>
        private void CheckEnemyProjectileCollisions()
        {
            foreach (WorldEntity projectile in projectiles)
            {
                foreach (WorldEntity enemy in enemies)
                {
                    if (projectile.position.Intersects(enemy.position))
                    {
                        enemy.destroyMe = true;
                        projectile.destroyMe = true;
                        PlaySound(ChargeMain.enemyDeathSound);
                    }
                }
                foreach (WorldEntity wall in walls)
                {
                    if (projectile.position.Intersects(wall.position))
                    {
                        projectile.destroyMe = true;
                    }
                }

                foreach (WorldEntity plat in platforms)
                {
                    if (plat.position.Left >= (projectile.position.Right + GameplayVars.WinWidth / 2)) continue;
                    if (projectile.position.Intersects(plat.position))
                    {
                        projectile.destroyMe = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the player death and transitions the game to the game-over state
        /// </summary>
        private void PlayerDeath()
        {
            // Set the game over variable
            isGameOver = true;

            // Create the player pixelized death
            List<Color> playerDeathColors = new List<Color>() { Color.Black, Color.White };
            PixelEffect playerDeathEffect = new PixelEffect(player.position, ChargeMain.WhiteTex, playerDeathColors);
            playerDeathEffect.EnableRandomPixelDirection(40);
            playerDeathEffect.SetSpawnFreqAndFade(5, 4);
            playerDeathEffect.followCamera = false;
            otherEnts.Add(playerDeathEffect);
        }

        /// <summary>
        /// Updates the player speed based on the current charge level
        /// </summary>
        private void UpdatePlayerCharge(float deltaTime)
        {
            player.DecCharge(GameplayVars.ChargeDecreaseRate * deltaTime);
        }
        
        /// <summary>
        /// Updates the player speed based on the current charge level
        /// Every 75 charge (One full charge bar) causes the rate of increase for subsequent charge to decrease. 
        /// </summary>
        private void UpdatePlayerSpeed()
        {

            int Level = ((int)player.GetCharge() / 75) + 1; //Gives the current level that the player is on
            float NextLevelCharge = 75 - (player.GetCharge() % 75); //Gives charge needed to levelup
            float newSpeed = 0; //Temp speed to be set after update

            for (int i = 0; i < Level; i++)
            {
                newSpeed += GameplayVars.LevelSpeeds[i] * 75;
            }
            newSpeed = newSpeed - GameplayVars.LevelSpeeds[Level - 1] * NextLevelCharge;
            playerSpeed = newSpeed * GameplayVars.ChargeToSpeedCoefficient;
        }

        /// <summary>
        /// Draws all entities in the given list
        /// </summary>
        private void DrawWorldEntityList(List<WorldEntity> list, SpriteBatch spriteBatch)
        {
            foreach (WorldEntity entity in list)
            {
                entity.Draw(spriteBatch);
            }
        }

        private void DrawPlatforms(SpriteBatch spriteBatch)
        {
            foreach (WorldEntity platform in platforms)
            {
                // If the player is null, then we can't get the current charge level, so just draw with full brightness
                if (player != null)
                {
                    Platform p = (Platform)platform;
                    p.Draw(spriteBatch, GetLevelChargePercent());
                }
                else
                {
                    platform.Draw(spriteBatch);
                }
            }
        }

        /// <summary>
        /// Initializes all variables for the InGame gamestate
        /// </summary>
        private void InitializeInGameState()
        {
            //Create the initial objects
            player = new Player(new Rectangle(GameplayVars.PlayerStartX, LevelGenerationVars.Tier2Height - 110, GameplayVars.StartPlayerWidth, GameplayVars.StartPlayerHeight), ChargeMain.PlayerTex); //The player character
            backBarrier = new Barrier(new Rectangle(GameplayVars.BackBarrierStartX, -50, GameplayVars.BarrierWidth, GameplayVars.WinHeight + 100), ChargeMain.BarrierTex, ChargeMain.WhiteTex); //The death barrier behind the player
            frontBarrier = new Barrier(new Rectangle(GameplayVars.FrontBarrierStartX, -50, GameplayVars.BarrierWidth, GameplayVars.WinHeight + 100), ChargeMain.BarrierTex, ChargeMain.WhiteTex); //The death barrier in front of the player

            //Reset the level generator.
            levelGenerator.Reset();

            // Set game state variables
            isGameOver = false;
            score = 0;
            tempScore = 0;
            globalCooldown = 0;
            totalGlobalCooldown = 0;

            LoadPlatformsForGameStart();
        }

        /// <summary>
        /// Initializes all variables for any menu screen state (TitleScreen, OptionsScreen, etc)
        /// </summary>
        private void InitializeMenuScreenState()
        {
            // Set the initial speeds
            playerSpeed = GameplayVars.PlayerStartSpeed;
            barrierSpeed = GameplayVars.BarrierStartSpeed;

            // Set game state variables
            isGameOver = false;
            score = 0;
            tempScore = 0;

            // Set the player and barriers to null. This is so they won't get drawn
            player = null;
            frontBarrier = null;
            backBarrier = null;

            levelGenerator.Reset();
        }

        /// <summary>
        /// Loads the initial platforms for the start of the game
        /// </summary>
        private void LoadPlatformsForGameStart()
        {
            //Long floor to catch player at the beginning of the game
            int startPlatWidth = GameplayVars.WinWidth - GameplayVars.PlayerStartX / 3;
            startPlatWidth -= (startPlatWidth % LevelGenerationVars.SegmentWidth); //Make it evenly split into segments
            Platform startPlat = new Platform(new Rectangle(GameplayVars.PlayerStartX, LevelGenerationVars.Tier3Height, startPlatWidth, LevelGenerationVars.PlatformHeight),
                ChargeMain.PlatformLeftTex, ChargeMain.PlatformCenterTex, ChargeMain.PlatformRightTex, GetCurrentPlatformColor());

            //Spawn a random platform in each of the upper two tiers
            int tier1X = rand.Next(0, GameplayVars.WinWidth);
            int tier1Width = LevelGenerationVars.SegmentWidth * rand.Next(LevelGenerationVars.MinNumSegments, LevelGenerationVars.MaxNumSegments);

            int tier2X = rand.Next(0, GameplayVars.WinWidth);
            int tier2Width = LevelGenerationVars.SegmentWidth * rand.Next(LevelGenerationVars.MinNumSegments, LevelGenerationVars.MaxNumSegments);

            Platform tier1 = new Platform(new Rectangle(tier1X, LevelGenerationVars.Tier1Height, tier1Width, LevelGenerationVars.PlatformHeight),
                ChargeMain.PlatformLeftTex, ChargeMain.PlatformCenterTex, ChargeMain.PlatformRightTex, GetCurrentPlatformColor());
            Platform tier2 = new Platform(new Rectangle(tier2X, LevelGenerationVars.Tier2Height, tier2Width, LevelGenerationVars.PlatformHeight),
                ChargeMain.PlatformLeftTex, ChargeMain.PlatformCenterTex, ChargeMain.PlatformRightTex, GetCurrentPlatformColor());

            //Since they're currently the only platform in their tier,
            //Set the newly created platforms as the right most in each of their tiers.
            levelGenerator.SetRightMost(tier1, 0);
            levelGenerator.SetRightMost(tier2, 1);
            levelGenerator.SetRightMost(startPlat, 2);

            //Add them to the platform list
            platforms.Add(tier1);
            platforms.Add(tier2);
            platforms.Add(startPlat);
        }
    }
}