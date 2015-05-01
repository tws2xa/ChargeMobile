#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
using System.IO;
using System.IO.IsolatedStorage;

#endregion

namespace Charge
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ChargeMain : Game
    {
        private static readonly String UserSettingsFile = "UserSettings.txt";

        private static readonly float VolumeChangeAmount = 0.05f;

        private static readonly Color[] ChargeBarLevelColors = { new Color(50, 50, 50), new Color(0, 234, 6), Color.Yellow, Color.Red, Color.Blue, Color.Pink }; // The bar colors for each charge level
        private static readonly Color[] PlatformLevelColors = { Color.White, new Color(0, 234, 6), Color.Yellow, Color.Tomato, Color.Blue, Color.DarkViolet }; // The platform colors for each charge level

        private static readonly String DefaultClearHighScoresText = "Clear High Scores";

        enum GameState
        {
            TitleScreen,
            OptionsScreen,
            CreditsScreen,
            InGame,
            Paused,
            GameOver
        };

        enum TitleSelection
        {
            Start,
            Options,
            Credits
        };

        enum OptionSelection
        {
            Volume,
            ClearHighScores,
            Back
        };

        enum PlayerLevel
        {
            Level1,
            Level2,
            Level3,
            Level4,
            Level5
        };

        GameState currentGameState;
        TitleSelection currentTitleSelection;
        OptionSelection currentOptionSelection;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player; //The player character
        List<Platform> platforms; //All platforms in game
        List<Enemy> enemies; //All enemies in game
        List<Projectile> projectiles; //All bullets in game
        List<WorldEntity> walls; //All walls in the game
        List<WorldEntity> batteries; //All batteries in the game
        List<WorldEntity> otherEnts; //Other objects, like effects
        Barrier backBarrier; //The death barrier behind the player
        Barrier frontBarrier; //The death barrier in front of the player
        Background background; //The scrolling backdrop
        ChargeBar chargeBar; // The chargebar
        SpecialAbilityIconSet specialAbilityIcons; //Discharge, Shoot, and Overcharge icons


        int score; //Player score
        float tempScore; //Keeps track of fractional score increases
        private static float globalCooldown; //The cooldown on powerups
        private static float totalGlobalCooldown; //The max from which the cooldown is decreasing
        int distanceSinceGeneration; // Distance since the last time that the charge orb spawning on each tier was decided
        int tierWithNoChargeOrbs; // Tier that spawns no charge orbs
        int tierWithSomeChargeOrbs; // Tier that spawns at least one charge orb per platform

        private SpriteFont FontSmall; //Sprite Font to draw score
        private SpriteFont FontLarge; //Sprite Font for title screen

        private float masterVolume;

        private SoundEffect shootSound;
        private SoundEffect jumpSound;
        private SoundEffect overchargeSound;
        private SoundEffect landSound;
        private SoundEffect enemyDeathSound;
        private SoundEffect chargeCollect;
        private SoundEffect dischargeSound;
        private SoundEffect rearmSound;
        private Song Background1;
        private Song TitleMusic;

        private static float playerSpeed; //Current run speed
        public static float barrierSpeed; //Speed of barriers

        PixelEffect fullScreenPixelEffect;
        bool doPausePixelEffect = true;
        bool doHighScorePixelEffect = true;
        bool doMainMenuPixelEffect = true;
        bool doPlayerPixelizeOnDeath = true;

        bool playLandSound = true;

        //Useful Tools
        Random rand; //Used for generating random variables
        LevelGenerator levelGenerator; //Generates the platforms
        Controls controls;
        bool soundOn = true;

        int glowWidth = GameplayVars.WinWidth / 7;
        int glowHeight = GameplayVars.WinHeight;
        int middlePinWidth = 30;
        int middlePinHeight = 30;

        String ClearHighScoresText;

        //Textures
        Texture2D BackgroundTex;
        Texture2D BarrierTex;
        Texture2D BatteryTex;
        Texture2D EnemyTex;
        Texture2D PlatformCenterTex;
        Texture2D PlatformLeftTex;
        Texture2D PlatformRightTex;
        Texture2D PlayerTex;
        Texture2D WallTex;
        Texture2D ChargeBarTex;
        Texture2D DischargeTex;
        Texture2D DischargeIconTex;
        Texture2D ShootIconTex;
        Texture2D OverchargeIconTex;
        Texture2D WhiteTex;
        Texture2D LeftGlow;
        Texture2D RightGlow;
        Texture2D MiddlePin;

        HighScoreManager highScoreManager;

        public ChargeMain()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            VirtualResolution.Init(ref graphics);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = true;
            
            VirtualResolution.SetVirtualResolution(GameplayVars.WinWidth, GameplayVars.WinHeight);
            VirtualResolution.SetResolution(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, true);

            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Set window size
            graphics.PreferredBackBufferWidth = GameplayVars.WinWidth;
            graphics.PreferredBackBufferHeight = GameplayVars.WinHeight;

            //Initialize all lists
            platforms = new List<Platform>(); //All platforms in game
            enemies = new List<Enemy>(); //All enemies in game
            projectiles = new List<Projectile>(); //All bullets in game
            walls = new List<WorldEntity>(); //All walls in the game
            batteries = new List<WorldEntity>(); //All batteries in the game
            otherEnts = new List<WorldEntity>(); //All other objects needed.

            //Initialize tools
            rand = new Random();
            levelGenerator = new LevelGenerator();
            controls = new Controls();
            highScoreManager = new HighScoreManager();
            
            //Initialize starting values for all numeric variables
            InitVars();
            playerSpeed = GameplayVars.titleScrollSpeed;

            LoadUserSettings();

            //Initialize Monogame Stuff
            base.Initialize();

            //Set title screen
            currentGameState = GameState.TitleScreen;
        }

        /// <summary>
        /// Sets all numeric variables to their starting values
        /// </summary>
        public void InitVars()
        {
            score = 0;
            globalCooldown = 0;
            totalGlobalCooldown = 0;
            distanceSinceGeneration = 0;
            tierWithNoChargeOrbs = 0;
            tierWithSomeChargeOrbs = 1;
            controls.Reset();

            ClearHighScoresText = DefaultClearHighScoresText;

            barrierSpeed = GameplayVars.BarrierStartSpeed;
        }

        /// <summary>
        /// Creates and Positions all objects for the start of the level
        /// </summary>
        public void SetupInitialConfiguration()
        {
            //Clear all entity lists
            platforms.Clear();
            enemies.Clear();
            projectiles.Clear();
            walls.Clear();
            batteries.Clear();
            otherEnts.Clear();
            if (currentGameState == GameState.TitleScreen)
            {
                MediaPlayer.Play(TitleMusic);
                MediaPlayer.IsRepeating = true;
            }
            if (currentGameState == GameState.InGame)
            {
                MediaPlayer.Play(Background1);
                MediaPlayer.IsRepeating = true;
            }

            //Create the initial objects
            player = new Player(new Rectangle(GameplayVars.PlayerStartX, LevelGenerationVars.Tier2Height - 110, GameplayVars.StartPlayerWidth, GameplayVars.StartPlayerHeight), PlayerTex); //The player character
            backBarrier = new Barrier(new Rectangle(GameplayVars.BackBarrierStartX, -50, GameplayVars.BarrierWidth, GameplayVars.WinHeight + 100), BarrierTex, WhiteTex); //The death barrier behind the player
            frontBarrier = new Barrier(new Rectangle(GameplayVars.FrontBarrierStartX, -50, GameplayVars.BarrierWidth, GameplayVars.WinHeight + 100), BarrierTex, WhiteTex); //The death barrier in front of the player
            background = new Background(BackgroundTex);
            chargeBar = new ChargeBar(new Rectangle(graphics.GraphicsDevice.Viewport.Width / 4, GameplayVars.ChargeBarY, graphics.GraphicsDevice.Viewport.Width / 2, GameplayVars.ChargeBarHeight), ChargeBarTex, ChargeBarLevelColors[0], ChargeBarLevelColors[1]);

            //Create UI Icons
            int iconSpacer = 0;
            int iconY = GameplayVars.WinHeight - SpecialAbilityIconSet.iconHeight - 10;
            specialAbilityIcons = new SpecialAbilityIconSet(iconSpacer + 10, iconY, iconSpacer, DischargeIconTex, ShootIconTex, OverchargeIconTex, WhiteTex);

            //Reset the level generator.
            levelGenerator.Reset();

            //Long floor to catch player at the beginning of the game
            int startPlatWidth = GameplayVars.WinWidth - GameplayVars.PlayerStartX / 3;
            startPlatWidth -= (startPlatWidth % LevelGenerationVars.SegmentWidth); //Make it evenly split into segments
            Platform startPlat = new Platform(new Rectangle(GameplayVars.PlayerStartX, LevelGenerationVars.Tier3Height, startPlatWidth, LevelGenerationVars.PlatformHeight),
                PlatformLeftTex, PlatformCenterTex, PlatformRightTex, GetCurrentPlatformColor());

            //Spawn a random platform in each of the upper two tiers
            int tier1X = rand.Next(0, GameplayVars.WinWidth);
            int tier1Width = LevelGenerationVars.SegmentWidth * rand.Next(LevelGenerationVars.MinNumSegments, LevelGenerationVars.MaxNumSegments);

            int tier2X = rand.Next(0, GameplayVars.WinWidth);
            int tier2Width = LevelGenerationVars.SegmentWidth * rand.Next(LevelGenerationVars.MinNumSegments, LevelGenerationVars.MaxNumSegments);

            Platform tier1 = new Platform(new Rectangle(tier1X, LevelGenerationVars.Tier1Height, tier1Width, LevelGenerationVars.PlatformHeight),
                PlatformLeftTex, PlatformCenterTex, PlatformRightTex, GetCurrentPlatformColor());
            Platform tier2 = new Platform(new Rectangle(tier2X, LevelGenerationVars.Tier2Height, tier2Width, LevelGenerationVars.PlatformHeight),
                PlatformLeftTex, PlatformCenterTex, PlatformRightTex, GetCurrentPlatformColor());

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

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Load all needed game textures and fonts
            BackgroundTex = this.Content.Load<Texture2D>("Background");
            BarrierTex = this.Content.Load<Texture2D>("BarrierAnimated");
            BatteryTex = this.Content.Load<Texture2D>("Battery");
            EnemyTex = this.Content.Load<Texture2D>("Enemy");
            PlatformCenterTex = this.Content.Load<Texture2D>("WhitePlatformCenterPiece");
            PlatformLeftTex = this.Content.Load<Texture2D>("WhitePlatformLeftCap");
            PlatformRightTex = this.Content.Load<Texture2D>("WhitePlatformRightCap");
            PlayerTex = this.Content.Load<Texture2D>("PlayerAnimation1");
            WallTex = this.Content.Load<Texture2D>("RedWall");
            ChargeBarTex = this.Content.Load<Texture2D>("ChargeBar");
            DischargeTex = this.Content.Load<Texture2D>("DischargeAnimated");
            DischargeIconTex = this.Content.Load<Texture2D>("DischargeIcon");
            ShootIconTex = this.Content.Load<Texture2D>("ShootIcon");
            OverchargeIconTex = this.Content.Load<Texture2D>("OverchargeIcon");
            WhiteTex = this.Content.Load<Texture2D>("White");
            LeftGlow = this.Content.Load<Texture2D>("GlowLeft");
            RightGlow = this.Content.Load<Texture2D>("GlowRight");
            MiddlePin = this.Content.Load<Texture2D>("MiddlePin");

            //Fonts
            FontSmall = this.Content.Load<SpriteFont>("fonts/OCR-A-Extended-24");
            FontLarge = this.Content.Load<SpriteFont>("fonts/OCR-A-Extended-48");

            //Sound Effects
            shootSound = Content.Load<SoundEffect>("SoundFX/shoot");
            jumpSound = Content.Load<SoundEffect>("SoundFX/jump");
            overchargeSound = Content.Load<SoundEffect>("SoundFX/overcharge");
            landSound = Content.Load<SoundEffect>("SoundFX/land");
            enemyDeathSound = Content.Load<SoundEffect>("SoundFX/enemyDeath");
            chargeCollect = Content.Load<SoundEffect>("SoundFX/charge_collect_quiet");
            dischargeSound = Content.Load<SoundEffect>("SoundFX/DischargeSound");
            rearmSound = Content.Load<SoundEffect>("SoundFX/Rearm");

            //BackgroundMusic
            Background1 = Content.Load<Song>("BackgroundMusic/Killing_Time");
            TitleMusic = Content.Load<Song>("BackgroundMusic/TitleLoop");

            //Init all objects and lists
            SetupInitialConfiguration();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //Delta time in seconds
            float deltaTime = (gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

            // This should be done regardless of the GameState
            controls.Update(); //Collect input data
            ProcessPlayerInput(); //Process input

            if (currentGameState == GameState.InGame && !this.IsActive)
            {
                PauseGame();
            }
            else if (currentGameState == GameState.TitleScreen || currentGameState == GameState.OptionsScreen || currentGameState == GameState.CreditsScreen)
            {
                background.Update(deltaTime); //Update the background scroll

                levelGenerator.Update(deltaTime);
                GenerateLevelContent();

                UpdateWorldEntities(deltaTime);
            }
            else if (currentGameState == GameState.InGame)
            {

                if (player.isDead)
                {
                    for (int i = 0; i < otherEnts.Count; i++)
                    {
                        WorldEntity e = otherEnts[i];

                        e.Update(deltaTime);

                        if (e.destroyMe)
                        {
                            otherEnts.Remove(e);
                            i--;
                        }
                    }
                }

                else
                {
                    background.Update(deltaTime); //Update the background scroll

                    player.Update(deltaTime); //Update the player

                    //Play the land sound if they player has jumped or fallen
                    if (!playLandSound && Math.Abs(player.vSpeed) > Math.Abs(GameplayVars.JumpInitialVelocity / 2))
                    {
                        playLandSound = true;
                    }
                    UpdateScore(deltaTime);	//Update the player score

                    UpdateWorldEntities(deltaTime);	//Update all entities in the world

                    CheckCollisions(); //Check for any collisions

                    UpdatePlayerCharge(deltaTime); // Decrements the player charge, given the amount of time that has passed

                    UpdatePlayerSpeed(); // Updates the player speed based on the current charge

                    levelGenerator.Update(deltaTime); //Update level generation info

                    GenerateLevelContent();	//Generate more level content

                    UpdateCooldown(deltaTime); //Update the global cooldown

                    UpdateEffects(deltaTime); //Handle effects for things like Overcharge, etc

                    specialAbilityIcons.Update(deltaTime); //Update the UI icons
                }
            }

            if (fullScreenPixelEffect != null)
            {
                fullScreenPixelEffect.Update(deltaTime);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Pauses the game
        /// </summary>
        private void PauseGame()
        {
            currentGameState = GameState.Paused;

            if (doPausePixelEffect)
            {
                CreateBasicFullScreenPixelEffect();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            VirtualResolution.BeginDraw();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, VirtualResolution.getTransformationMatrix());

            if (currentGameState == GameState.TitleScreen || currentGameState == GameState.OptionsScreen || currentGameState == GameState.CreditsScreen)
            {
                //Draw Background
                background.Draw(spriteBatch);

                //Draw Walls
                foreach (WorldEntity wall in walls)
                {
                    wall.Draw(spriteBatch);
                }

                //Draw platforms
                foreach (Platform platform in platforms)
                {
                    platform.Draw(spriteBatch, GetLevelChargePercent());
                }

                //Draw Enemies
                foreach (Enemy enemy in enemies)
                {
                    enemy.Draw(spriteBatch);
                }

                //Draw Batteries
                foreach (WorldEntity battery in batteries)
                {
                    battery.Draw(spriteBatch);
                }

                //Draw Other
                foreach (WorldEntity ent in otherEnts)
                {
                    ent.Draw(spriteBatch);
                }

                //Darken background
                spriteBatch.Draw(WhiteTex, new Rectangle(-10, -10, GameplayVars.WinWidth + 20, GameplayVars.WinHeight + 20), Color.Black * 0.3f);

                //Pixel effect if turned on
                if (doMainMenuPixelEffect)
                {
                    if (fullScreenPixelEffect == null) CreateUnobtrusiveFullScreenPixelEffect();
                    fullScreenPixelEffect.Draw(spriteBatch);
                }

                if (currentGameState == GameState.TitleScreen)
                {
                    //Draw Title Menu
                    String Title = "CHARGE";
                    String Options = "Options";
                    String Start = "Start Game";
                    String Credits = "Credits";
                    int TitleDrawX = GetCenteredStringLocation(FontLarge, Title, GameplayVars.WinWidth / 2);
                    int OptionsDrawX = GetCenteredStringLocation(FontSmall, Options, GameplayVars.WinWidth / 2);
                    int CreditsDrawX = GetCenteredStringLocation(FontSmall, Credits, GameplayVars.WinWidth / 2);
                    int StartDrawX = GetCenteredStringLocation(FontSmall, Start, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, Title, new Vector2(TitleDrawX, 100), Color.WhiteSmoke, Color.Black, FontLarge);

                    if (currentTitleSelection == TitleSelection.Start)
                    {
                        
                        DrawStringWithShadow(spriteBatch, Start, new Vector2(StartDrawX, 250), Color.Gold, Color.Black);
                        DrawStringWithShadow(spriteBatch, Options, new Vector2(OptionsDrawX, 325));
                        DrawStringWithShadow(spriteBatch, Credits, new Vector2(CreditsDrawX, 400));
                    }
                    else if (currentTitleSelection == TitleSelection.Options)
                    {
                        DrawStringWithShadow(spriteBatch, Start, new Vector2(StartDrawX, 250));
                        DrawStringWithShadow(spriteBatch, Options, new Vector2(OptionsDrawX, 325), Color.Gold, Color.Black);
                        DrawStringWithShadow(spriteBatch, Credits, new Vector2(CreditsDrawX, 400));
                    }
                    else if (currentTitleSelection == TitleSelection.Credits)
                    {
                        DrawStringWithShadow(spriteBatch, Start, new Vector2(StartDrawX, 250));
                        DrawStringWithShadow(spriteBatch, Options, new Vector2(OptionsDrawX, 325));
                        DrawStringWithShadow(spriteBatch, Credits, new Vector2(CreditsDrawX, 400), Color.Gold, Color.Black);
                    }
                }
                else if (currentGameState == GameState.OptionsScreen)
                {
                    String Title = "Options";
                    int TitleDrawX = GetCenteredStringLocation(FontSmall, Title, GameplayVars.WinWidth / 2);
                    spriteBatch.DrawString(FontSmall, Title, new Vector2(TitleDrawX, 25), Color.White);

                    // Set the color for the selected and unselected menu items
                    Color volumeColor = Color.White;
                    Color backColor = Color.White;
                    Color clearColor = Color.White;

                    if (currentOptionSelection == OptionSelection.Volume)
                    {
                        volumeColor = Color.Gold;
                    }
                    else if (currentOptionSelection == OptionSelection.Back)
                    {
                        backColor = Color.Gold;
                    }
                    else if (currentOptionSelection == OptionSelection.ClearHighScores)
                    {
                        clearColor = Color.Gold;
                    }

                    String Volume = "Master Volume: ";
                    int VolumeDrawX = GetCenteredStringLocation(FontSmall, Volume, GameplayVars.WinWidth / 4);
                    spriteBatch.DrawString(FontSmall, Volume, new Vector2(VolumeDrawX, 150), volumeColor);

                    // Draw the volume slider bar
                    int volumeBarLeft = Convert.ToInt32(Math.Round(3 * GameplayVars.WinWidth / 4.0f - ((GameplayVars.WinWidth / 3.0f + 5) / 2.0f)));
                    int maxWidth = Convert.ToInt32(GameplayVars.WinWidth / 3) + 5;
                    spriteBatch.Draw(WhiteTex, new Rectangle(volumeBarLeft, 162, maxWidth, 20), new Color(100, 100, 100));
                    spriteBatch.Draw(WhiteTex, new Rectangle(volumeBarLeft, 162, Convert.ToInt32(masterVolume * GameplayVars.WinWidth / 3) + 5, 20), volumeColor);

                    int ClearDrawX = GetCenteredStringLocation(FontSmall, ClearHighScoresText, GameplayVars.WinWidth / 2);
                    spriteBatch.DrawString(FontSmall, ClearHighScoresText, new Vector2(ClearDrawX, 300), clearColor);

                    String Back = "Back";
                    int BackDrawX = GetCenteredStringLocation(FontSmall, Back, GameplayVars.WinWidth / 2);
                    spriteBatch.DrawString(FontSmall, Back, new Vector2(BackDrawX, 350), backColor);
                }
                else if (currentGameState == GameState.CreditsScreen)
                {
                    int startY = GameplayVars.WinHeight / 23;
                    int largeGapSize = GameplayVars.WinHeight / 7;
                    int medGapSize = GameplayVars.WinHeight / 12;
                    int smallGapSize = Convert.ToInt32(FontSmall.MeasureString("[]").Y);

                    int yPos = startY;

                    String Title = "CHARGE";
                    int TitleDrawX = GetCenteredStringLocation(FontSmall, Title, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, Title, new Vector2(TitleDrawX, yPos));
                    yPos += largeGapSize;

                    String Developers = "Developers";
                    int DevDrawX = GetCenteredStringLocation(FontSmall, Developers, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, Developers, new Vector2(DevDrawX, yPos)); //100
                    yPos += medGapSize;

                    String Sam = "Sam Leonard";
                    int SamDrawX = GetCenteredStringLocation(FontSmall, Sam, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, Sam, new Vector2(SamDrawX, yPos)); //150
                    yPos += smallGapSize;

                    String Dan = "Dan O'Connor";
                    int DanDrawX = GetCenteredStringLocation(FontSmall, Dan, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, Dan, new Vector2(DanDrawX, yPos)); //175
                    yPos += smallGapSize;

                    String Adam = "Adam Rosenburg";
                    int AdamDrawX = GetCenteredStringLocation(FontSmall, Adam, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, Adam, new Vector2(AdamDrawX, yPos)); //200
                    yPos += smallGapSize;

                    String Thomas = "Thomas Sparks";
                    int ThomasDrawX = GetCenteredStringLocation(FontSmall, Thomas, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, Thomas, new Vector2(ThomasDrawX, yPos)); //225
                    yPos += largeGapSize;

                    String Music = "Music";
                    int MusicDrawX = GetCenteredStringLocation(FontSmall, Music, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, Music, new Vector2(MusicDrawX, yPos)); //300
                    yPos += medGapSize;

                    /* License:
                     * "Killing Time", "Space Fighter Loop"
                     * Kevin MacLeod (incompetech.com)
                     * Licensed under Creative Commons: By Attribution 3.0
                     * http://creativecommons.org/licenses/by/3.0/
                     */

                    String line1 = "\"Killing Time\", \"Space Fighter Loop\"";
                    int KillingTimeDrawX = GetCenteredStringLocation(FontSmall, line1, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, line1, new Vector2(KillingTimeDrawX, yPos)); //350
                    yPos += smallGapSize;

                    String line2 = "Kevin MacLeod (incompetech.com)";
                    int SpaceFighterDrawX = GetCenteredStringLocation(FontSmall, line2, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, line2, new Vector2(SpaceFighterDrawX, yPos)); //375
                    yPos += smallGapSize;

                    String ccAttr = "Licensed under Creative Commons: By Attribution 3.0";
                    int AcknowledgementDrawX = GetCenteredStringLocation(FontSmall, ccAttr, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, ccAttr, new Vector2(AcknowledgementDrawX, yPos)); //400
                    yPos += smallGapSize;

                    String url = "http://creativecommons.org/licenses/by/3.0/";
                    int urlDrawX = GetCenteredStringLocation(FontSmall, url, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, url, new Vector2(urlDrawX, yPos)); //400
                    yPos += medGapSize;

                    String Back = "Back";
                    int BackDrawX = GetCenteredStringLocation(FontSmall, Back, GameplayVars.WinWidth / 2);
                    DrawStringWithShadow(spriteBatch, Back, new Vector2(BackDrawX, yPos), Color.Yellow, Color.Black); //450
                }
            }

            if (currentGameState == GameState.InGame || currentGameState == GameState.Paused)
            {
                //Draw background
                background.Draw(spriteBatch);

                //Draw Walls
                foreach (WorldEntity wall in walls)
                {
                    wall.Draw(spriteBatch);
                }

                //Draw platforms
                foreach (Platform platform in platforms)
                {
                    platform.Draw(spriteBatch, GetLevelChargePercent());
                }

                //Draw Enemies
                foreach (Enemy enemy in enemies)
                {
                    enemy.Draw(spriteBatch);
                }

                //Draw Projectiles
                foreach (Projectile projectile in projectiles)
                {
                    projectile.Draw(spriteBatch);
                }

                //Draw Batteries
                foreach (WorldEntity battery in batteries)
                {
                    battery.Draw(spriteBatch);
                }

                //Draw Other
                foreach (WorldEntity ent in otherEnts)
                {
                    ent.Draw(spriteBatch);
                }

                //Draw the player
                if ((!doPlayerPixelizeOnDeath || !player.isDead))
                {
                    player.Draw(spriteBatch);
                }

                DrawBarrierWarningGlow(spriteBatch);

                //Draw Barriers
                frontBarrier.Draw(spriteBatch);
                backBarrier.Draw(spriteBatch);

                // Draw UI
                DrawUI(spriteBatch);

                // Draw Score
                if (player.isDead)
                {
                    spriteBatch.Draw(WhiteTex, new Rectangle(-10, -10, GameplayVars.WinWidth + 20, GameplayVars.WinHeight + 20), Color.Black * 0.5f);
                    if (doHighScorePixelEffect)
                    {
                        if (fullScreenPixelEffect == null) CreateUnobtrusiveFullScreenPixelEffect();
                        fullScreenPixelEffect.Draw(spriteBatch);
                    }

                    int rowHeight = Convert.ToInt32(Math.Round(FontSmall.MeasureString("[1st: 999]").Y * 1.15));
                    int initOffset = Convert.ToInt32(Math.Round(GameplayVars.WinHeight / 8.0));
                    bool hasDrawnMyScore = false;
                    for (int i = 0; i < GameplayVars.NumScores; i++)
                    {
                        String place;
                        if (i == 0)
                            place = "1st";
                        else if (i == 1)
                            place = "2nd";
                        else if (i == 2)
                            place = "3rd";
                        else
                            place = (i + 1) + "th";

                        string toDraw = place + ": " + highScoreManager.getHighScore(i);
                        int strDrawX = GetCenteredStringLocation(FontSmall, toDraw, GameplayVars.WinWidth / 2);
                        if (highScoreManager.getHighScore(i) == score && !hasDrawnMyScore)
                        {
                            //Highlight your score in the leaderboard
                            DrawStringWithShadow(spriteBatch, toDraw, new Vector2(strDrawX, initOffset + rowHeight * i), Color.Gold, new Color(10, 10, 10));
                            hasDrawnMyScore = true;
                        }
                        else
                        {
                            DrawStringWithShadow(spriteBatch, toDraw, new Vector2(strDrawX, initOffset + rowHeight * i));
                        }
                    }
                    if (hasDrawnMyScore)
                    {
                        string highScore = "New High Score!";
                        int highScoreDrawX = GetCenteredStringLocation(FontSmall, highScore, GameplayVars.WinWidth / 2);
                        DrawStringWithShadow(spriteBatch, highScore, new Vector2(highScoreDrawX, initOffset - rowHeight), Color.Gold, new Color(10, 10, 10));
                    }
                    string finalScore = ("Final Score: " + score);
                    string playAgain = controls.GetRestartString() + " to play again!";
                    string returnToTitle = controls.GetReturnToTitleString() + " to return to the title screen";
                    int scoreYPos = initOffset + rowHeight * GameplayVars.NumScores + 1;
                    DrawStringWithShadow(spriteBatch, finalScore, new Vector2(GetCenteredStringLocation(FontSmall, finalScore, GameplayVars.WinWidth / 2), scoreYPos));
                    DrawStringWithShadow(spriteBatch, playAgain, new Vector2(GetCenteredStringLocation(FontSmall, playAgain, GameplayVars.WinWidth / 2), scoreYPos + rowHeight));
                    DrawStringWithShadow(spriteBatch, returnToTitle, new Vector2(GetCenteredStringLocation(FontSmall, returnToTitle, GameplayVars.WinWidth / 2), scoreYPos + rowHeight * 2));


                }
                else
                {
                    String scoreStr = ("Score: " + score);
                    Vector2 strSize = FontSmall.MeasureString(scoreStr);
                    DrawStringWithShadow(spriteBatch, scoreStr, new Vector2(GameplayVars.WinWidth - strSize.X * 1.2f, GameplayVars.ChargeBarY));
                }

                // Draw the pause screen on top of all of the game assets
                if (currentGameState == GameState.Paused)
                {
                    spriteBatch.Draw(WhiteTex, new Rectangle(0, 0, GameplayVars.WinWidth, GameplayVars.WinHeight), Color.Black * 0.5f);
                    if (doPausePixelEffect) fullScreenPixelEffect.Draw(spriteBatch);
                    int lineHeight = Convert.ToInt32(Math.Ceiling(FontSmall.MeasureString("Paused").Y));
                    DrawStringWithShadow(spriteBatch, "Paused", new Vector2(15, 15));
                    DrawStringWithShadow(spriteBatch, controls.GetUnpauseText() + " to resume.", new Vector2(15, 15 + lineHeight));
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private int GetCenteredStringLocation(SpriteFont theFont, String str, int center)
        {
            return Convert.ToInt32(Math.Round(center - theFont.MeasureString(str).X / 2));
        }

        /// <summary>
        /// Draws all UI elements
        /// </summary>
        private void DrawUI(SpriteBatch spriteBatch)
        {
            chargeBar.Draw(spriteBatch, player.GetCharge());
            specialAbilityIcons.Draw(spriteBatch);
            Rectangle pinRect = new Rectangle(GameplayVars.WinWidth / 2 - middlePinWidth / 2, GameplayVars.WinHeight - middlePinHeight, middlePinWidth, middlePinHeight);
            spriteBatch.Draw(MiddlePin, pinRect, Color.White);
        }

        /// <summary>
        /// Draws the warning glow for each barrier
        /// </summary>
        private void DrawBarrierWarningGlow(SpriteBatch spriteBatch)
        {
            float distThreshold = GameplayVars.GlowThreshold;

            float backOpacity = ((distThreshold + backBarrier.position.Center.X) / distThreshold); //Back Barrier pos will usually be negative if barrier is off screen
            if (backOpacity > 1)
            {
                backOpacity = (1.0f / backOpacity);
            }
            backOpacity = Math.Max(0, backOpacity);

            float frontOffset = (frontBarrier.position.Center.X - GameplayVars.WinWidth);
            float frontOpacity = ((distThreshold - frontOffset) / distThreshold);
            if (frontOpacity > 1)
            {
                frontOpacity = (1.0f / frontOpacity);
            }
            frontOffset = Math.Max(0, frontOpacity);



            if (backOpacity > 0)
            {
                spriteBatch.Draw(LeftGlow, new Rectangle(0, 0, glowWidth, glowHeight), Color.White * backOpacity);
            }
            if (frontOpacity > 0)
            {
                spriteBatch.Draw(RightGlow, new Rectangle(GameplayVars.WinWidth - glowWidth, 0, glowWidth, glowHeight), Color.White * frontOpacity);
            }
        }

        /// <summary>
        /// Draws a string with a slight, black shadow behind it
        /// </summary>
        /// <param name="text">Text to draw</param>
        /// <param name="location">Upper left corner of string</param>
        void DrawStringWithShadow(SpriteBatch spriteBatch, String text, Vector2 location)
        {
            DrawStringWithShadow(spriteBatch, text, location, Color.WhiteSmoke, Color.Black);
        }

        /// <summary>
        /// Draws a string with a slight, black shadow behind it
        /// </summary>
        /// <param name="text">Text to draw</param>
        /// <param name="location">Upper left corner of string</param>
        /// <param name="backColor">Shadow Color</param>
        /// <param name="textColor">Main text Color</param>
        void DrawStringWithShadow(SpriteBatch spriteBatch, String text, Vector2 location, Color textColor, Color backColor)
        {
            DrawStringWithShadow(spriteBatch, text, location, textColor, backColor, FontSmall);
        }

        /// <summary>
        /// Draws a string with a slight, black shadow behind it
        /// </summary>
        /// <param name="text">Text to draw</param>
        /// <param name="location">Upper left corner of string</param>
        /// <param name="backColor">Shadow Color</param>
        /// <param name="textColor">Main text Color</param>
        /// <param name="font">The font to use</param>
        void DrawStringWithShadow(SpriteBatch spriteBatch, String text, Vector2 location, Color textColor, Color backColor, SpriteFont font)
        {
            spriteBatch.DrawString(font, text, new Vector2(location.X + 2, location.Y + 2), backColor);
            spriteBatch.DrawString(font, text, location, textColor);
        }

        /// <summary>
        /// This is called from the update loop to handle player input
        /// </summary>
        private void ProcessPlayerInput()
        {
            if (currentGameState == GameState.TitleScreen)
            {
                if (controls.MenuUpTrigger())
                {
                    if (currentTitleSelection > 0)
                    {
                        currentTitleSelection--;
                    }
                }
                else if (controls.MenuDownTrigger())
                {
                    if (currentTitleSelection < TitleSelection.Credits)
                    {
                        currentTitleSelection++;
                    }
                }

                if (controls.MenuSelectTrigger())
                {
                    if (currentTitleSelection == TitleSelection.Start)
                    {
                        InitVars();
                        SetupInitialConfiguration();
                        currentGameState = GameState.InGame;
                        if (currentGameState == GameState.InGame)
                        {
                            MediaPlayer.Play(Background1);
                            MediaPlayer.IsRepeating = true;
                        }
                    }
                    else if (currentTitleSelection == TitleSelection.Options)
                    {
                        currentGameState = GameState.OptionsScreen;
                    }
                    else if (currentTitleSelection == TitleSelection.Credits)
                    {
                        currentGameState = GameState.CreditsScreen;
                    }
                }
            }
            else if (currentGameState == GameState.CreditsScreen && controls.MenuSelectTrigger())
            {
                currentGameState = GameState.TitleScreen;
            }
            else if (currentGameState == GameState.OptionsScreen)
            {
                if (controls.MenuUpTrigger())
                {
                    if (currentOptionSelection > 0)
                    {
                        currentOptionSelection--;
                    }

                    // Once the user clears the high scores, we don't want that option to be selectable any more.
                    if (currentOptionSelection == OptionSelection.ClearHighScores && ClearHighScoresText != DefaultClearHighScoresText)
                    {
                        currentOptionSelection = OptionSelection.Volume;
                    }
                }
                else if (controls.MenuDownTrigger())
                {
                    if (currentOptionSelection < OptionSelection.Back)
                    {
                        currentOptionSelection++;
                    }

                    // Once the user clears the high scores, we don't want that option to be selectable any more.
                    if (currentOptionSelection == OptionSelection.ClearHighScores && ClearHighScoresText != DefaultClearHighScoresText)
                    {
                        currentOptionSelection = OptionSelection.Back;
                    }
                }

                if (controls.MenuSelectTrigger() && currentOptionSelection == OptionSelection.Back)
                {
                    ClearHighScoresText = DefaultClearHighScoresText;
                    currentGameState = GameState.TitleScreen;
                    SaveUserSettings();
                }

                if (controls.MenuSelectTrigger() && currentOptionSelection == OptionSelection.ClearHighScores)
                {
                    highScoreManager.ClearHighScores();
                    ClearHighScoresText = "High Scores Cleared";
                    currentOptionSelection++;
                }

                if (controls.MenuDecreaseTrigger() && currentOptionSelection == OptionSelection.Volume)
                {
                    masterVolume -= VolumeChangeAmount;

                    if (masterVolume < 0)
                    {
                        masterVolume = 0;
                    }

                    MediaPlayer.Volume = masterVolume;
                }

                if (controls.MenuIncreaseTrigger() && currentOptionSelection == OptionSelection.Volume)
                {
                    masterVolume += VolumeChangeAmount;

                    if (masterVolume > 1)
                    {
                        masterVolume = 1;
                    }

                    MediaPlayer.Volume = masterVolume;
                }
            }
            else if (currentGameState == GameState.InGame && !player.isDead)
            {

                // Player has pressed the jump command (A button on controller, space bar on keyboard)
                if (controls.JumpTrigger() && (player.jmpNum < GameplayVars.playerNumJmps || player.grounded))
                {
                    player.jmpNum++;
                    player.vSpeed = GameplayVars.JumpInitialVelocity;
                    player.grounded = false;
                    PlaySound(jumpSound);
                }
                else if (controls.JumpRelease() && player.vSpeed < 0)
                {
                    // Cut jump short on button release
                    player.vSpeed /= 2;
                }

                // Player has pressed the Discharge command (A key or left arrow key on keyboard)
                if (controls.DischargeTrigger())
                {
                    InitiateDischarge();
                }

                // Player has pressed the Shoot command (S key or down arrow key on keyboard)
                if (controls.ShootTrigger())
                {
                    InitiateShoot();
                }

                // Player has pressed the Overcharge command (D key or right arrow key on keyboard)
                if (controls.OverchargeTrigger())
                {
                    InitiateOvercharge();
                }

                // Player has pressed the Pause command (P key or Start button)
                if (controls.PauseTrigger())
                {
                    PauseGame();
                }

            }
            else if (currentGameState == GameState.InGame && player.isDead)
            {
                if (controls.RestartTrigger())
                {
                    player.isDead = false;
                    InitVars();
                    SetupInitialConfiguration();
                }
                else if (controls.TitleScreenTrigger())
                {
                    player.isDead = false;
                    InitVars();
                    playerSpeed = GameplayVars.titleScrollSpeed;
                    currentGameState = GameState.TitleScreen;
                    SetupInitialConfiguration();
                }
            }
            else if (currentGameState == GameState.Paused)
            {
                // Player has pressed the Pause command (P key or Start button)
                if (controls.UnpauseTrigger())
                {
                    if (doPausePixelEffect) fullScreenPixelEffect = null;
                    currentGameState = GameState.InGame;
                }
            }
        }

        /// <summary>
        /// Plays the given sound
        /// </summary>
        /// <param name="sound">The sound to play</param>
        private void PlaySound(SoundEffect sound)
        {
            if (soundOn)
            {
                try
                {
                    SoundEffectInstance soundInstance = sound.CreateInstance();
                    soundInstance.Volume = masterVolume;

                    soundInstance.Play();
                }
                catch (Microsoft.Xna.Framework.Audio.NoAudioHardwareException)
                {
                    Console.WriteLine("Failed to play sound: " + sound);
                    soundOn = false;
                }
                catch (System.DllNotFoundException)
                {
                    Console.WriteLine("Failed to play sound: " + sound);
                    soundOn = false;
                }
            }
        }

        /// <summary>
        /// Launches the overcharge special ability
        /// </summary>
        private void InitiateOvercharge()
        {
            if (globalCooldown > 0)
            {
                return;
            }

            player.Overcharge();
            PlaySound(overchargeSound);

            SetGlobalCooldown(GameplayVars.OverchargeCooldownTime[GetCurrentLevel() - 1]);
        }


        /// <summary>
        /// Launches the shoot special ability  
        /// </summary>
        private void InitiateShoot()
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
            Projectile bullet = new Projectile(new Rectangle(bulletX, bulletY, bulletWidth, bulletHeight), ChargeBarTex, GameplayVars.BulletMoveSpeed);
            projectiles.Add(bullet);

            SetGlobalCooldown(GameplayVars.ShootCooldownTime[GetCurrentLevel() - 1]);
            PlaySound(shootSound);
        }

        /// <summary>
        /// Launches the discharge special ability
        /// </summary>
        private void InitiateDischarge()
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

            DischargeAnimation discharge = new DischargeAnimation(new Rectangle(player.position.Left, player.position.Top, player.position.Width, player.position.Width), DischargeTex, player);
            otherEnts.Add(discharge);

            SetGlobalCooldown(GameplayVars.DischargeCooldownTime[GetCurrentLevel() - 1]);
            PlaySound(dischargeSound);
        }


        public void SetGlobalCooldown(float cooldown)
        {
            globalCooldown = cooldown;
            totalGlobalCooldown = cooldown;
        }

        public void UpdateEffects(float deltaTime)
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
                    OverchargeEffect effect = new OverchargeEffect(new Rectangle(effectX, effectY, effectWidth, effectHeight), ChargeBarTex, player);
                    otherEnts.Add(effect);
                }
            }
        }

        /// <summary>
        /// Update the global cooldown
        /// </summary>
        public void UpdateCooldown(float deltaTime)
        {
            globalCooldown = Math.Max(0, globalCooldown - deltaTime);
            if (globalCooldown < 0 + deltaTime * 2 && globalCooldown > 0)
            {
                PlaySound(rearmSound);
            }
            if (globalCooldown == 0)
            {
                totalGlobalCooldown = 0;
            }
        }

        /// <summary>
        /// Update the score
        /// </summary>
        public void UpdateScore(float deltaTime)
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
        /// Updates all the world entities
        /// </summary>
        public void UpdateWorldEntities(float deltaTime)
        {
            //Update platforms
            for (int i = 0; i < platforms.Count; i++)
            {
                Platform entity = platforms[i];
                entity.Update(deltaTime);
                //Check if it should be deleted
                if (entity.destroyMe)
                {
                    platforms.Remove(entity); //Remove from platforms list
                    levelGenerator.PlatformRemoved(entity); //Alert the level generator
                    entity = null; //Clear the platform
                    i--; //Move back one in the loop to adjust for the removal
                }
            }

            //Update Batteries
            for (int i = 0; i < batteries.Count; i++)
            {
                WorldEntity entity = batteries[i];
                entity.Update(deltaTime);

                //Check if it should be deleted
                if (entity.destroyMe)
                {
                    batteries.Remove(entity);
                    entity = null;
                    i--;
                }
            }

            //Update Barriers (don't let them move any go faster than the player could ever go)
            //barrierSpeed = Math.Min(barrierSpeed + GameplayVars.BarrierSpeedUpRate * deltaTime, GameplayVars.ChargeBarCapacity * GameplayVars.ChargeToSpeedCoefficient);
            barrierSpeed += GameplayVars.BarrierSpeedUpRate * deltaTime;


            frontBarrier.Update(deltaTime);
            backBarrier.Update(deltaTime);

            //Update Enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy entity = enemies[i];
                entity.Update(deltaTime);

                //Check if it should be deleted
                if (entity.destroyMe)
                {
                    enemies.Remove(entity);

                    /*
                    List<Color> destroyCols = new List<Color>() { Color.Red, Color.Black };
                    DisintegrationEffect disEffect = new DisintegrationEffect(entity.position, EnemyTex, WhiteTex, destroyCols, 0.2f, false);
                    otherEnts.Add(disEffect);
                    */

                    List<Color> destroyCols = new List<Color>() { Color.Red, Color.Black };
                    PixelEffect pixelEffect = new PixelEffect(entity.position, WhiteTex, destroyCols);
                    pixelEffect.yVel = -20;
                    otherEnts.Add(pixelEffect);



                    entity = null;
                    i--;
                }
            }

            //Update Walls
            for (int i = 0; i < walls.Count; i++)
            {
                WorldEntity entity = walls[i];
                entity.Update(deltaTime);

                //Check if it should be deleted
                if (entity.destroyMe)
                {
                    walls.Remove(entity);

                    /*
                    List<Color> destroyCols = new List<Color>() { Color.Red, Color.Black };
                    DisintegrationEffect disEffect = new DisintegrationEffect(entity.position, WallTex, WhiteTex, destroyCols, 0.1f, true);
                    otherEnts.Add(disEffect);
                    */

                    List<Color> destroyCols = new List<Color>() { Color.Red, Color.Black };
                    PixelEffect pixelEffect = new PixelEffect(entity.position, WhiteTex, destroyCols);
                    //pixelEffect.xVel = playerSpeed / 4;
                    //pixelEffect.yVel = -35;
                    pixelEffect.followCamera = false;
                    pixelEffect.yVel = -10;
                    pixelEffect.xVel = -80;
                    pixelEffect.SetSpawnFreqAndFade(5, 0.5f);
                    otherEnts.Add(pixelEffect);

                    entity = null;
                    i--;
                }
            }

            //Update Projectiles
            for (int i = 0; i < projectiles.Count; i++)
            {
                Projectile entity = projectiles[i];
                entity.Update(deltaTime);

                //Check if it should be deleted
                if (entity.destroyMe)
                {
                    projectiles.Remove(entity);
                    entity = null;
                    i--;
                }
            }

            //Update Other
            for (int i = 0; i < otherEnts.Count; i++)
            {
                WorldEntity entity = otherEnts[i];
                entity.Update(deltaTime);

                //Check if it should be deleted
                if (entity.destroyMe)
                {
                    otherEnts.Remove(entity);
                    entity = null;
                    i--;
                }
            }
            // Update distance since last tier charge orb decision
            distanceSinceGeneration += (int)(deltaTime * GetPlayerSpeed());
        }

        /// <summary>
        /// Checks all collisions in the game
        /// </summary>
        public void CheckCollisions()
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
        public void CheckPlayerBarrierCollisions()
        {
            if (player.position.Right > frontBarrier.position.Center.X)
            {
                PlayerDeath();
            }
            if (player.position.Left < backBarrier.position.Center.X)
            {
                PlayerDeath();
            }

            if (player.position.Top > GameplayVars.WinWidth + 10)
            {
                PlayerDeath();
            }
        }

        /// <summary>
        /// Checks the player against all platforms in the world
        /// </summary>
        public void CheckPlayerPlatformCollisions()
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
                            PlaySound(landSound);
                        }
                        break; //Hit a platform. No need to check any more.
                    }
                }
            }
        }

        /// <summary>
        /// Checks the player against all batteries in the world
        /// </summary>
        public void CheckPlayerBatteryCollisions()
        {
            foreach (WorldEntity battery in batteries)
            {
                if (player.position.Intersects(battery.position))
                {
                    player.IncCharge(GameplayVars.BatteryChargeReplenish);
                    battery.destroyMe = true;
                    PlaySound(chargeCollect);
                    break;
                }
            }
        }

        public void CheckPlayerEnemyCollisions()
        {
            foreach (WorldEntity enemy in enemies)
            {
                if (player.CheckEnemyCollision(enemy))
                {
                    PlayerDeath();
                }
            }
        }

        public void CheckPlayerWallCollisions()
        {
            foreach (WorldEntity wall in walls)
            {
                if (player.CheckWallCollision(wall))
                {
                    if (player.OverchargeActive())
                    {
                        //player.IncOverchargeCharge(-15);
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
        public void CheckEnemyDischargeBlastCollisions()
        {
            foreach (WorldEntity enemy in enemies)
            {
                foreach (WorldEntity effect in otherEnts)
                {
                    if (effect is DischargeAnimation && ((DischargeAnimation)effect).circle.Intersects(enemy.position))
                    {
                        enemy.destroyMe = true;
                        PlaySound(enemyDeathSound);
                    }
                }
            }
        }

        /// <summary>
        /// Destroy enemies hit by bullets
        /// </summary>
        public void CheckEnemyProjectileCollisions()
        {
            foreach (WorldEntity projectile in projectiles)
            {
                foreach (WorldEntity enemy in enemies)
                {
                    if (projectile.position.Intersects(enemy.position))
                    {
                        enemy.destroyMe = true;
                        projectile.destroyMe = true;
                        PlaySound(enemyDeathSound);
                    }
                }
                foreach (WorldEntity wall in walls)
                {
                    if (projectile.position.Intersects(wall.position))
                    {
                        projectile.destroyMe = true;
                    }
                }

                foreach (Platform plat in platforms)
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
        /// Kills the player
        /// This implementation temporary as I anticipte we will be adding a Title and death screen.
        /// </summary>
        public void PlayerDeath()
        {
            // freezeWorld();
            player.isDead = true;

            if (doPlayerPixelizeOnDeath)
            {
                List<Color> playerDeathColors = new List<Color>() { Color.Black, Color.White };
                PixelEffect playerDeathEffect = new PixelEffect(player.position, WhiteTex, playerDeathColors);
                playerDeathEffect.EnableRandomPixelDirection(40);
                playerDeathEffect.SetSpawnFreqAndFade(5, 4);
                playerDeathEffect.followCamera = false;
                otherEnts.Add(playerDeathEffect);
            }

            highScoreManager.updateHighScore(score);
        }

        /// <summary>
        /// Freezes GameplayVars on death before score is displayed.
        /// </summary>
        /*public void freezeWorld()
        {
            GameplayVars.ChargeToSpeedCoefficient = 0;
            GameplayVars.ChargeDecreaseRate = 0;
            GameplayVars.TimeToScoreCoefficient = 0f;          
            barrierSpeed = 0;
		}*/

        /// <summary>
        /// Updates the player speed based on the current charge level
        /// </summary>
        public void UpdatePlayerCharge(float deltaTime)
        {
            player.DecCharge(GameplayVars.ChargeDecreaseRate * deltaTime);

            // Pick the background color for the charge bar
            Color backColor;

            backColor = ChargeBarLevelColors[GetBackgroundColorIndex()];

            // Pick the foreground color for the charge bar
            Color foreColor = ChargeBarLevelColors[GetForegroundColorIndex()];

            // Set the colors for the charge bar
            chargeBar.SetBackgroundColor(backColor);
            chargeBar.SetForegroundColor(foreColor);
        }

        public Color GetCurrentPlatformColor()
        {
            int index = GetCurrentLevel();
            index %= PlatformLevelColors.Length;
            return PlatformLevelColors[index];
        }

        public int GetBackgroundColorIndex()
        {
            return (Convert.ToInt32(Math.Floor(player.GetCharge() / GameplayVars.ChargeBarCapacity)) % ChargeBarLevelColors.Length);
        }

        public int GetForegroundColorIndex()
        {
            return (GetBackgroundColorIndex() + 1) % ChargeBarLevelColors.Length;
        }

        /// <summary>
        /// Updates the player speed based on the current charge level
        /// Every 75 charge (One full charge bar) causes the rate of increase for subsequent charge to decrease. 
        /// </summary>
        public void UpdatePlayerSpeed()
        {

            int Level = ((int)player.GetCharge() / 75) + 1; //Gives the current level that the player is on
            float NextLevelCharge = 75 - (player.GetCharge() % 75); //Gives charge needed to levelup
            float newSpeed = 0; //Temp speed to be set after update

            for (int i = 0; i < Level; i++)
            {
                newSpeed += GameplayVars.LevelSpeeds[i] * 75;
            }
            newSpeed = newSpeed - GameplayVars.LevelSpeeds[Level - 1] * NextLevelCharge;
            //Console.WriteLine((newSpeed* GameplayVars.ChargeToSpeedCoefficient) - GetPlayerSpeed());
            playerSpeed = newSpeed * GameplayVars.ChargeToSpeedCoefficient;
            //playerSpeed = GameplayVars.ChargeToSpeedCoefficient * player.GetCharge();


        }

        /// <summary>
        /// Generates new level content
        /// </summary>
        public void GenerateLevelContent()
        {
            //Get the new platforms
            List<Platform> newPlatforms = levelGenerator.GenerateNewPlatforms(platforms.Count, PlatformLeftTex, PlatformCenterTex, PlatformRightTex, GetCurrentPlatformColor());

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
                float playerBarrierSpeedDiff = GetPlayerSpeed() - barrierSpeed;
                float multiplier = playerBarrierSpeedDiff / barrierSpeed;
                batteryRollRange -= Convert.ToInt32(LevelGenerationVars.MaxBatteryVariation * multiplier);

                //Either a battery is necessary or the roll results in battery spawning and a battery can spawn on that tier
                if ((i == necessaryOrbLocation) || (roll < batteryRollRange && numBatteries < LevelGenerationVars.MaxBatteriesPerPlatform))
                    if (platform.getTier() != tierWithNoChargeOrbs)
                    {
                        //Spawn Battery
                        int width = LevelGenerationVars.BatteryWidth;
                        int height = LevelGenerationVars.BatteryHeight;
                        WorldEntity battery = new WorldEntity(new Rectangle(sectionCenter - width / 2, platform.position.Top - height / 2 - GameplayVars.StartPlayerHeight / 3, width, height), BatteryTex);
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
                        WorldEntity wall = new WorldEntity(new Rectangle(platform.sections[i].position.Right - width / 2, platform.position.Top - height + 3, width, height), WallTex);
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
                        Enemy enemy = new Enemy(new Rectangle(sectionCenter - width / 2, platform.position.Top - height, width, height), EnemyTex, platform);
                        enemies.Add(enemy);
                        numEnemies++;
                    }
            }

        }

        /// <summary>
        /// Provides the speed of the player (and thus
        /// the opposite of the speed that the world should scroll).
        /// </summary>
        /// <returns>The player's speed</returns>
        public static float GetPlayerSpeed()
        {
            return playerSpeed;
        }

        /// <summary>
        /// Returns the remaining global cooldown time
        /// </summary>
        internal static float GetGlobalCooldown()
        {
            return globalCooldown;
        }

        /// <summary>
        /// Returns the amount from which the global cooldown is decreasing.
        /// </summary>
        internal static float GetTotalCooldown()
        {
            return totalGlobalCooldown;
        }

        public int GetCurrentLevel()
        {

            int level = 0;
            int n = 0;
            float barrierChargeEquivalent = barrierSpeed / GameplayVars.ChargeToSpeedCoefficient;

            while (barrierChargeEquivalent > 0)
            {
                barrierChargeEquivalent -= GameplayVars.ChargeBarCapacity * GameplayVars.LevelSpeeds[n];
                level++;
                n++;
            }

            //int level = Convert.ToInt32(Math.Floor(barrierChargeEquivalent / GameplayVars.ChargeBarCapacity));

            return level;
        }

        public float GetLevelChargePercent()
        {
            float curCharge = player.GetCharge();
            float maxCharge = GameplayVars.ChargeBarCapacity * GetCurrentLevel();

            if (maxCharge - curCharge >= GameplayVars.ChargeBarCapacity) return 0; //A whole level's worth too slow
            if (maxCharge - curCharge <= 0) return 1; //Current charge > max

            curCharge %= GameplayVars.ChargeBarCapacity;

            return (curCharge / (float)GameplayVars.ChargeBarCapacity);
        }

        public void CreateBasicFullScreenPixelEffect()
        {
            List<Color> colors = new List<Color>();
            for (int i = 0; i <= 255; i += 20)
            {
                colors.Add(new Color(i, i, i));
            }
            fullScreenPixelEffect = new PixelEffect(new Rectangle(0, 0, GameplayVars.WinWidth, GameplayVars.WinHeight), WhiteTex, colors);
            fullScreenPixelEffect.pixelSize = 10;
            fullScreenPixelEffect.spawnFadeTime = -1;
            fullScreenPixelEffect.followCamera = false;
            fullScreenPixelEffect.pixelFadeTime = 3;
            fullScreenPixelEffect.spawnFrequency = 0.2f;
            fullScreenPixelEffect.pixelYVel = 20;
        }

        public void CreateUnobtrusiveFullScreenPixelEffect()
        {
            List<Color> colors = new List<Color>();
            for (int i = 0; i <= 150; i += 10)
            {
                colors.Add(new Color(i, i, i));
            }
            fullScreenPixelEffect = new PixelEffect(new Rectangle(0, 0, GameplayVars.WinWidth, GameplayVars.WinHeight), WhiteTex, colors);
            fullScreenPixelEffect.pixelSize = 10;
            fullScreenPixelEffect.spawnFadeTime = -1;
            fullScreenPixelEffect.followCamera = false;
            fullScreenPixelEffect.pixelFadeTime = 3;
            fullScreenPixelEffect.spawnFrequency = 0.1f;
            fullScreenPixelEffect.pixelYVel = 20;
        }

        private void SaveUserSettings()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream settingsFileStream = store.OpenFile(UserSettingsFile, FileMode.OpenOrCreate);

            StreamWriter settingsWriter = new StreamWriter(settingsFileStream);

            settingsWriter.Write(masterVolume);

            settingsWriter.Close();
        }

        private void LoadUserSettings()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();

            // Load user volume settings
            if (store.FileExists(UserSettingsFile))
            {
                IsolatedStorageFileStream settingsFileStream = store.OpenFile(UserSettingsFile, FileMode.Open);
                StreamReader settingsReader = new StreamReader(settingsFileStream);
                
                String volumeAsText = settingsReader.ReadLine();
                masterVolume = (float)Convert.ToDouble(volumeAsText);
                Console.WriteLine(volumeAsText);
                settingsReader.Close();
            }
            else
            {
                masterVolume = 0.5f;
                SaveUserSettings();
            }

            // Sets the volume for the MediaPlayer. This controls the volume for the Songs used for the title screen and background music
            MediaPlayer.Volume = masterVolume;
        }
    }
}
