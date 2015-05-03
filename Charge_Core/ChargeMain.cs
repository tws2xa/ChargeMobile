#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
#endregion

namespace Charge
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class ChargeMain : Game
    {
        // Static Content Assets. They are static because they will need to be accessed from the GameWorld object.
        public static SoundEffect shootSound;
        public static SoundEffect jumpSound;
        public static SoundEffect overchargeSound;
        public static SoundEffect landSound;
        public static SoundEffect enemyDeathSound;
        public static SoundEffect chargeCollect;
        public static SoundEffect dischargeSound;
        public static SoundEffect rearmSound;
        public static Song Background1;
        
        public static Texture2D BackgroundTex;
        public static Texture2D BarrierTex;
        public static Texture2D BatteryTex;
        public static Texture2D EnemyTex;
        public static Texture2D PlatformCenterTex;
        public static Texture2D PlatformLeftTex;
        public static Texture2D PlatformRightTex;
        public static Texture2D PlayerTex;
        public static Texture2D WallTex;
        public static Texture2D DischargeTex;
        public static Texture2D WhiteTex;
        public static Texture2D LeftGlow;
        public static Texture2D RightGlow;
        public static Texture2D ChargeBarTex;

        public enum GameState
        {
            TitleScreen,
            OptionsScreen,
            CreditsScreen,
            InGame,
            Paused,
            GameOver,
            TutorialJump,
            TutorialDischarge,
            TutorialShoot,
            TutorialOvercharge
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

        private GameState currentGameState;
        private TitleSelection currentTitleSelection;
        private OptionSelection currentOptionSelection;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private SpriteFont FontSmall; //Sprite Font to draw score
        private SpriteFont FontLarge; //Sprite Font for title screen

        // User Settings
        private float masterVolume;
        private bool showTutorial;
        
        private Song TitleMusic;
        
        private PixelEffect fullScreenPixelEffect;
        private bool doPausePixelEffect = true;
        private bool doHighScorePixelEffect = true;
        private bool doMainMenuPixelEffect = true;
        
        private Controls controls;

        private int middlePinWidth = 30;
        private int middlePinHeight = 30;

        private String ClearHighScoresText;
        
        private HighScoreManager highScoreManager;

        // UI variables
        private SpecialAbilityIconSet specialAbilityIcons; //Discharge, Shoot, and Overcharge icons
        private Texture2D DischargeIconTex;
        private Texture2D ShootIconTex;
        private Texture2D OverchargeIconTex;
        private Texture2D MiddlePin;

        private GameWorld gameWorld; // Represents the current GameWorld

        private ChargeBar chargeBar; // The chargebar

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

            controls = new Controls();
            highScoreManager = new HighScoreManager();

            LoadUserSettings();

            //Set title screen
            currentGameState = GameState.TitleScreen;
            
            controls.Reset();

            ClearHighScoresText = GameplayVars.DefaultClearHighScoresText;
            
            //Initialize Monogame Stuff
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            //UI Textures
            DischargeIconTex = this.Content.Load<Texture2D>("DischargeIcon");
            ShootIconTex = this.Content.Load<Texture2D>("ShootIcon");
            OverchargeIconTex = this.Content.Load<Texture2D>("OverchargeIcon");
            WhiteTex = this.Content.Load<Texture2D>("White");
            MiddlePin = Content.Load<Texture2D>("MiddlePin");
            ChargeBarTex = Content.Load<Texture2D>("ChargeBar");
            BackgroundTex = Content.Load<Texture2D>("Background");
            BarrierTex = Content.Load<Texture2D>("BarrierAnimated");
            BatteryTex = Content.Load<Texture2D>("Battery");
            EnemyTex = Content.Load<Texture2D>("Enemy");
            PlatformCenterTex = Content.Load<Texture2D>("WhitePlatformCenterPiece");
            PlatformLeftTex = Content.Load<Texture2D>("WhitePlatformLeftCap");
            PlatformRightTex = Content.Load<Texture2D>("WhitePlatformRightCap");
            PlayerTex = Content.Load<Texture2D>("PlayerAnimation1");
            WallTex = Content.Load<Texture2D>("RedWall");
            DischargeTex = Content.Load<Texture2D>("DischargeAnimated");
            WhiteTex = Content.Load<Texture2D>("White");
            LeftGlow = Content.Load<Texture2D>("GlowLeft");
            RightGlow = Content.Load<Texture2D>("GlowRight");
            ChargeBarTex = Content.Load<Texture2D>("ChargeBar");

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
            TitleMusic = Content.Load<Song>("BackgroundMusic/TitleLoop.wav");
            Background1 = Content.Load<Song>("BackgroundMusic/Killing_Time.wav");

            InitializeContentDependentVariables();
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
        /// Initialize all variables that rely on the content to be loaded first
        /// </summary>
        private void InitializeContentDependentVariables()
        {
            // Create a new game world
            gameWorld = new GameWorld();
            gameWorld.InitializeStateSpecificVariables(currentGameState);
            gameWorld.SetMasterVolume(masterVolume);

            // Stop any currently playing song, and play the title screen background music
            MediaPlayer.Stop();
            MediaPlayer.Play(TitleMusic);
            MediaPlayer.IsRepeating = true;

            //Create UI Icons
            int iconSpacer = 0;
            int iconY = GameplayVars.WinHeight - SpecialAbilityIconSet.iconHeight - 10;
            specialAbilityIcons = new SpecialAbilityIconSet(iconSpacer + 10, iconY, iconSpacer, DischargeIconTex, ShootIconTex, OverchargeIconTex, WhiteTex);

            chargeBar = new ChargeBar(new Rectangle(graphics.GraphicsDevice.Viewport.Width / 4, GameplayVars.ChargeBarY, graphics.GraphicsDevice.Viewport.Width / 2, GameplayVars.ChargeBarHeight), ChargeBarTex, GameplayVars.ChargeBarLevelColors[0], GameplayVars.ChargeBarLevelColors[1]);
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

            // Pause the game if it loses focus
            if (currentGameState == GameState.InGame && !this.IsActive)
            {
                PauseGame();
            }

            // Update the game world
            gameWorld.Update(deltaTime, currentGameState);

            if (currentGameState == GameState.InGame || currentGameState == GameState.Paused || currentGameState == GameState.GameOver)
            {
                // Update the Special Icons cooldown
                float totalCooldown = gameWorld.GetTotalCooldown();
                float globalCooldown = gameWorld.GetGlobalCooldown();
                specialAbilityIcons.SetCooldown(totalCooldown, globalCooldown);

                // Update the charge bar colors
                UpdateChargeBar();
            }

            // If the game has ended, but the GameState is not set to GameOver. This ensures that the high score will only be updated once, instead of every time the update loop executes.
            if (gameWorld.IsGameOver() && currentGameState != GameState.GameOver)
            {
                highScoreManager.updateHighScore(gameWorld.GetScore());

                currentGameState = GameState.GameOver;
            }

            // Update the pixel effect
            if (fullScreenPixelEffect != null)
            {
                fullScreenPixelEffect.Update(deltaTime, gameWorld.GetPlayerSpeed());
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

            // Draw the game world
            gameWorld.Draw(spriteBatch);

            // Handle UI elements for various game states
            if (currentGameState == GameState.TitleScreen || currentGameState == GameState.OptionsScreen || currentGameState == GameState.CreditsScreen)
            {
                //Darken background
                spriteBatch.Draw(WhiteTex, new Rectangle(-10, -10, GameplayVars.WinWidth + 20, GameplayVars.WinHeight + 20), Color.Black * 0.3f);

                //Pixel effect if turned on
                if (doMainMenuPixelEffect)
                {
                    if (fullScreenPixelEffect == null) CreateUnobtrusiveFullScreenPixelEffect();
                    fullScreenPixelEffect.Draw(spriteBatch);
                }

                // Handle state-specific drawing
                if (currentGameState == GameState.TitleScreen)
                {
                    DrawTitleScreen(spriteBatch);
                }
                else if (currentGameState == GameState.OptionsScreen)
                {
                    DrawOptionsScreen(spriteBatch);
                }
                else if (currentGameState == GameState.CreditsScreen)
                {
                    DrawCreditsScreen(spriteBatch);
                }
            }
            else if (currentGameState == GameState.InGame || currentGameState == GameState.Paused || currentGameState == GameState.GameOver)
            {
                // Draw UI
                DrawUI(spriteBatch);

                // Draw Score
                if (currentGameState == GameState.GameOver)
                {
                    DrawGameOverUI(spriteBatch);
                }
                else
                {
                    DrawInGameUI(spriteBatch);
                }

                // Draw the pause screen on top of all of the game assets
                if (currentGameState == GameState.Paused)
                {
                    DrawGamePausedUI(spriteBatch);
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawTitleScreen(SpriteBatch spriteBatch)
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

        private void DrawOptionsScreen(SpriteBatch spriteBatch)
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

        private void DrawCreditsScreen(SpriteBatch spriteBatch)
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

        private void DrawInGameUI(SpriteBatch spriteBatch)
        {
            // Get the score from the game world object
            int score = gameWorld.GetScore();

            String scoreStr = ("Score: " + score);
            Vector2 strSize = FontSmall.MeasureString(scoreStr);
            DrawStringWithShadow(spriteBatch, scoreStr, new Vector2(GameplayVars.WinWidth - strSize.X * 1.2f, GameplayVars.ChargeBarY));
        }

        private void DrawGameOverUI(SpriteBatch spriteBatch)
        {
            // Get score from the game world object
            int score = gameWorld.GetScore();

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

        private void DrawGamePausedUI(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(WhiteTex, new Rectangle(0, 0, GameplayVars.WinWidth, GameplayVars.WinHeight), Color.Black * 0.5f);
            if (doPausePixelEffect) fullScreenPixelEffect.Draw(spriteBatch);
            int lineHeight = Convert.ToInt32(Math.Ceiling(FontSmall.MeasureString("Paused").Y));
            DrawStringWithShadow(spriteBatch, "Paused", new Vector2(15, 15));
            DrawStringWithShadow(spriteBatch, controls.GetUnpauseText() + " to resume.", new Vector2(15, 15 + lineHeight));
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
            chargeBar.Draw(spriteBatch, gameWorld.GetCharge());
            specialAbilityIcons.Draw(spriteBatch);
            Rectangle pinRect = new Rectangle(GameplayVars.WinWidth / 2 - middlePinWidth / 2, GameplayVars.WinHeight - middlePinHeight, middlePinWidth, middlePinHeight);
            spriteBatch.Draw(MiddlePin, pinRect, Color.White);
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
                        /*if (showTutorial)
                        {
                            currentGameState = GameState.TutorialJump;

                            showTutorial = false;
                            SaveUserSettings();
                        }
                        else
                        {*/
                        currentGameState = GameState.InGame;
                        gameWorld.InitializeStateSpecificVariables(currentGameState);

                        // Stop any currently playing song, and play the in-game background music
                        MediaPlayer.Stop();
                        MediaPlayer.Play(ChargeMain.Background1);
                        MediaPlayer.IsRepeating = true;
                        //}
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
                    if (currentOptionSelection == OptionSelection.ClearHighScores && ClearHighScoresText != GameplayVars.DefaultClearHighScoresText)
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
                    if (currentOptionSelection == OptionSelection.ClearHighScores && ClearHighScoresText != GameplayVars.DefaultClearHighScoresText)
                    {
                        currentOptionSelection = OptionSelection.Back;
                    }
                }

                if (controls.MenuSelectTrigger() && currentOptionSelection == OptionSelection.Back)
                {
                    ClearHighScoresText = GameplayVars.DefaultClearHighScoresText;
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
                    masterVolume -= GameplayVars.VolumeChangeAmount;

                    if (masterVolume < 0)
                    {
                        masterVolume = 0;
                    }

                    MediaPlayer.Volume = masterVolume;
                }

                if (controls.MenuIncreaseTrigger() && currentOptionSelection == OptionSelection.Volume)
                {
                    masterVolume += GameplayVars.VolumeChangeAmount;

                    if (masterVolume > 1)
                    {
                        masterVolume = 1;
                    }

                    MediaPlayer.Volume = masterVolume;
                }
            }
            else if (currentGameState == GameState.InGame)
            {

                // Player has pressed the jump command (A button on controller, space bar on keyboard)
                if (controls.JumpTrigger())
                {
                    gameWorld.InitiateJump();
                }
                else if (controls.JumpRelease())
                {
                    gameWorld.CutJump();
                }

                // Player has pressed the Discharge command (A key or left arrow key on keyboard)
                if (controls.DischargeTrigger())
                {
                    gameWorld.InitiateDischarge();
                }

                // Player has pressed the Shoot command (S key or down arrow key on keyboard)
                if (controls.ShootTrigger())
                {
                    gameWorld.InitiateShoot();
                }

                // Player has pressed the Overcharge command (D key or right arrow key on keyboard)
                if (controls.OverchargeTrigger())
                {
                    gameWorld.InitiateOvercharge();
                }

                // Player has pressed the Pause command (P key or Start button)
                if (controls.PauseTrigger())
                {
                    PauseGame();
                }

            }
            else if (currentGameState == GameState.GameOver)
            {
                if (controls.RestartTrigger())
                {
                    currentGameState = GameState.InGame;
                    gameWorld.InitializeStateSpecificVariables(currentGameState);
                }
                else if (controls.TitleScreenTrigger())
                {
                    currentGameState = GameState.TitleScreen;
                    gameWorld.InitializeStateSpecificVariables(currentGameState);

                    // Stop any currently playing song, and play the title screen background music
                    MediaPlayer.Stop();
                    MediaPlayer.Play(TitleMusic);
                    MediaPlayer.IsRepeating = true;
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
        /// Updates the player speed based on the current charge level
        /// </summary>
        public void UpdateChargeBar()
        {
            // Pick the background color for the charge bar
            Color backColor;

            backColor = GameplayVars.ChargeBarLevelColors[GetBackgroundColorIndex()];

            // Pick the foreground color for the charge bar
            Color foreColor = GameplayVars.ChargeBarLevelColors[GetForegroundColorIndex()];

            // Set the colors for the charge bar
            chargeBar.SetBackgroundColor(backColor);
            chargeBar.SetForegroundColor(foreColor);
        }

        public Color GetCurrentPlatformColor()
        {
            int index = gameWorld.GetCurrentLevel();
            index %= GameplayVars.PlatformLevelColors.Length;
            return GameplayVars.PlatformLevelColors[index];
        }

        public int GetBackgroundColorIndex()
        {
            return (Convert.ToInt32(Math.Floor(gameWorld.GetCharge() / GameplayVars.ChargeBarCapacity)) % GameplayVars.ChargeBarLevelColors.Length);
        }

        public int GetForegroundColorIndex()
        {
            return (GetBackgroundColorIndex() + 1) % GameplayVars.ChargeBarLevelColors.Length;
        }

        public float GetLevelChargePercent()
        {
            float curCharge = gameWorld.GetCharge();
            float maxCharge = GameplayVars.ChargeBarCapacity * gameWorld.GetCurrentLevel();

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
            XmlDocument settings = new XmlDocument();
            XmlElement settingsRoot = settings.CreateElement("settings");
            settings.AppendChild(settingsRoot);

            XmlElement volumeElement = settings.CreateElement("volume");
            XmlText volumeText = settings.CreateTextNode(Convert.ToString(masterVolume));
            settings.DocumentElement.AppendChild(volumeElement);
            settings.DocumentElement.LastChild.AppendChild(volumeText);

            XmlElement tutorialElement = settings.CreateElement("tutorial");
            XmlText tutorialText = settings.CreateTextNode(Convert.ToString(showTutorial));
            settings.DocumentElement.AppendChild(tutorialElement);
            settings.DocumentElement.LastChild.AppendChild(tutorialText);

            // Get the file
            FileStream settingsFileStream = FileSystemManager.GetFileStream(GameplayVars.UserSettingsFile, FileMode.OpenOrCreate);

            settings.Save(settingsFileStream);

            settingsFileStream.Close();
        }

        private void LoadUserSettings()
        {
            // Load user volume settings
            if (FileSystemManager.FileExists(GameplayVars.UserSettingsFile))
            {
                FileStream settingsFileStream = FileSystemManager.GetFileStream(GameplayVars.UserSettingsFile, FileMode.Open);

                XmlDocument settings = new XmlDocument();
                settings.Load(settingsFileStream);

                // Get the user's volume information
                XmlNode volumeNode = settings.GetElementsByTagName("volume").Item(0);
                String volumeAsText = volumeNode.InnerText;
                
                masterVolume = (float)Convert.ToDouble(volumeAsText);

                // Get the info on whether the tutorial should be shown when the user first starts a game
                XmlNode tutorialNode = settings.GetElementsByTagName("tutorial").Item(0);
                String tutorialAsText = tutorialNode.InnerText;

                showTutorial = Convert.ToBoolean(tutorialAsText);

                settingsFileStream.Close();
            }
            else
            {
                masterVolume = 0.5f;

                // If there is no settings file, then it is probably the first time that the game has been run so we should show the tutorial.
                showTutorial = true;

                SaveUserSettings();
            }

            // Sets the volume for the MediaPlayer. This controls the volume for the Songs used for the title screen and background music
            MediaPlayer.Volume = masterVolume;
        }
    }
}
