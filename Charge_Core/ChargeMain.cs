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

        public static SpriteFont FontSmall; //Sprite Font to draw score
        public static SpriteFont FontLarge; //Sprite Font for title screen

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

        enum PlayerLevel
        {
            Level1,
            Level2,
            Level3,
            Level4,
            Level5
        };

        internal GameState currentGameState;

        MainMenuManager mainMenuManager;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        
        // User Settings
        public static float masterVolume;
        private bool showTutorial;
        
        private Song TitleMusic;
        
        private PixelEffect fullScreenPixelEffect;
        private bool doPausePixelEffect = true;
        private bool doHighScorePixelEffect = true;
        private bool doMainMenuPixelEffect = true;
        
        private Controls controls;

        private int middlePinWidth = 30;
        private int middlePinHeight = 30;
                
        private HighScoreManager highScoreManager;

        // UI variables
        private SpecialAbilityIconSet specialAbilityIcons; //Discharge, Shoot, and Overcharge icons
        private Texture2D DischargeIconTex;
        private Texture2D ShootIconTex;
        private Texture2D OverchargeIconTex;
        private Texture2D MiddlePin;
        private Texture2D Arrow;

        private GameWorld gameWorld; // Represents the current GameWorld

        private ChargeBar chargeBar; // The chargebar

        // Tutorial variables
        private List<TutorialMessage> tutorialMessages;
        private int EndTutorialJumpMessageId;
        private int EndTutorialDischargeMessageId;
        private int EndTutorialOverchargeMessageId;
        private int EndTutorialShootMessageId;

        public ChargeMain()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            VirtualResolution.Init(ref graphics);
            Content.RootDirectory = "Content";
			            
            VirtualResolution.SetVirtualResolution(GameplayVars.WinWidth, GameplayVars.WinHeight);
            VirtualResolution.SetResolution(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, true);

            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }

		public void SetScreenSize(int width, int height)
		{
			graphics.PreferredBackBufferWidth = width;
			graphics.PreferredBackBufferHeight = height;
			VirtualResolution.SetResolution(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, false);
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
            //graphics.PreferredBackBufferWidth = GameplayVars.WinWidth / 2;
            //graphics.PreferredBackBufferHeight = GameplayVars.WinHeight / 2;

            controls = new Controls();
            highScoreManager = new HighScoreManager();

            LoadUserSettings();

            //Set title screen
            currentGameState = GameState.TitleScreen;
            
            controls.Reset();
            
            tutorialMessages = new List<TutorialMessage>();
            EndTutorialDischargeMessageId = -1; // Avoid bugs caused by this value evaulating to 0 before it is set
            EndTutorialJumpMessageId = -1;
            EndTutorialOverchargeMessageId = -1;
            EndTutorialShootMessageId = -1;
            
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
            DischargeIconTex = Content.Load<Texture2D>("DischargeIcon");
            ShootIconTex = Content.Load<Texture2D>("ShootIcon");
            OverchargeIconTex = Content.Load<Texture2D>("OverchargeIcon");
            WhiteTex = Content.Load<Texture2D>("White");
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
            Arrow = Content.Load<Texture2D>("Arrow");

			//Fonts
			FontSmall = Content.Load<SpriteFont>("fonts/OCR-A-Extended-24");
			FontLarge = Content.Load<SpriteFont>("fonts/OCR-A-Extended-48");

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
            TitleMusic = Content.Load<Song>("BackgroundMusic/TitleLoop");
            Background1 = Content.Load<Song>("BackgroundMusic/Killing_Time");

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

            mainMenuManager = new MainMenuManager(WhiteTex, FontLarge, FontSmall);

            chargeBar = new ChargeBar(new Rectangle(GameplayVars.WinWidth / 4, GameplayVars.ChargeBarY, GameplayVars.WinWidth / 2, GameplayVars.ChargeBarHeight), ChargeBarTex, GameplayVars.ChargeBarLevelColors[0], GameplayVars.ChargeBarLevelColors[1]);
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
            if (currentGameState == GameState.InGame && !IsActive)
            {
                PauseGame();
            }

            // Update the game world
            gameWorld.Update(deltaTime, currentGameState);

            // Update any tutorial messages
            UpdateTutorialMessages(deltaTime);

            if (currentGameState == GameState.InGame || currentGameState == GameState.Paused || currentGameState == GameState.GameOver)
            {
                // Update the Special Icons cooldown
                float totalCooldown = gameWorld.GetTotalCooldown();
                float globalCooldown = gameWorld.GetGlobalCooldown();
                specialAbilityIcons.SetCooldown(totalCooldown, globalCooldown);

                // Update the charge bar colors
                UpdateChargeBar();
            }
            else if (currentGameState == GameState.TutorialJump && gameWorld.PlayerHasCompletedTutorialJump())
            {
                currentGameState = GameState.TutorialDischarge;
                gameWorld.InitializeStateSpecificVariables(currentGameState);

                tutorialMessages.RemoveAt(0);
                LoadTutorialDischargeMessages();
            }
            else if (currentGameState == GameState.TutorialDischarge)
            {
                if (gameWorld.PlayerHasCompletedTutorialDischarge())
                {
                    currentGameState = GameState.TutorialOvercharge;
                    gameWorld.InitializeStateSpecificVariables(currentGameState);
                }
                else if (gameWorld.GetHasDischarged() && tutorialMessages.Count > 0 && tutorialMessages[0].GetId() == EndTutorialDischargeMessageId)
                {
                    tutorialMessages.RemoveAt(0);

                    LoadTutorialOverchargeMessages();
                }
            }
            else if (currentGameState == GameState.TutorialShoot)
            {
                if (gameWorld.PlayerHasCompletedTutorialShoot())
                {
                    SwitchFromTutorialToInGame();
                }
                else if (gameWorld.GetHasShot() && tutorialMessages.Count > 0 && tutorialMessages[0].GetId() == EndTutorialShootMessageId)
                {
                    tutorialMessages.RemoveAt(0);

                    LoadTutorialDoneMessages();
                }
            }
            else if (currentGameState == GameState.TutorialOvercharge)
            {
                if (gameWorld.PlayerHasCompletedTutorialOvercharge())
                {
                    currentGameState = GameState.TutorialShoot;
                    gameWorld.InitializeStateSpecificVariables(currentGameState);
                }
                else if (gameWorld.GetHasOvercharged() && tutorialMessages.Count > 0 && tutorialMessages[0].GetId() == EndTutorialOverchargeMessageId)
                {
                    tutorialMessages.RemoveAt(0);

                    LoadTutorialShootMessages();
                }
            }

            // If the game has ended, but the GameState is not set to GameOver. This ensures that the high score will only be updated once, instead of every time the update loop executes.
            if (gameWorld.IsGameOver() && currentGameState != GameState.GameOver)
            {
                highScoreManager.updateHighScore(gameWorld.GetScore());

                currentGameState = GameState.GameOver;
            }

            // Need to update charge bar colors for tutorial states
            if (IsTutorialState(currentGameState))
            {
                UpdateChargeBar();
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

            // Draw any tutorial messages
            DrawTutorialMessages(spriteBatch);

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
            else if (IsTutorialState(currentGameState))
            {
                // Draw the charge bar
                chargeBar.Draw(spriteBatch, gameWorld.GetCharge());

                if (currentGameState == GameState.TutorialDischarge)
                {
                    // Draw discharge icon
                    specialAbilityIcons.DrawDischargeIcon(spriteBatch);
                }
                else if (currentGameState == GameState.TutorialShoot)
                {
                    // Draw the shoot icon
                    specialAbilityIcons.DrawShootIcon(spriteBatch);
                }
                else if (currentGameState == GameState.TutorialOvercharge)
                {
                    // Draw the overcharge icon
                    specialAbilityIcons.DrawOverChargeIcon(spriteBatch);
                }

                // Draw the skip tutorial UI
                mainMenuManager.DrawSkipTutorial(spriteBatch);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawTitleScreen(SpriteBatch spriteBatch)
        {
            mainMenuManager.DrawTitleScreen(spriteBatch);
        }

        private void DrawOptionsScreen(SpriteBatch spriteBatch)
        {
            mainMenuManager.DrawOptionsScreen(spriteBatch);
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

        public static int GetCenteredStringLocation(SpriteFont theFont, String str, int center)
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
        public static void DrawStringWithShadow(SpriteBatch spriteBatch, String text, Vector2 location)
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
        public static void DrawStringWithShadow(SpriteBatch spriteBatch, String text, Vector2 location, Color textColor, Color backColor)
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
        public static void DrawStringWithShadow(SpriteBatch spriteBatch, String text, Vector2 location, Color textColor, Color backColor, SpriteFont font)
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
                mainMenuManager.ProcessMainMenuInput(this, controls);
            } 
            else if (currentGameState == GameState.CreditsScreen && controls.MenuSelectTrigger())
            {
                currentGameState = GameState.TitleScreen;
            }
            else if (currentGameState == GameState.OptionsScreen)
            {
                mainMenuManager.ProcessOptionsInput(this, controls);
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
            else if (IsTutorialState(currentGameState))
            {
                // Handle tutorial skip case
                if (mainMenuManager.SkipTutorialTriggered(controls))
                {
                    SwitchFromTutorialToInGame();
                }

                // Player has pressed the jump command (A button on controller, space bar on keyboard)
                if (controls.JumpTrigger() && tutorialMessages.Count > 0 && tutorialMessages[0].GetId() == EndTutorialJumpMessageId)
                {
                    gameWorld.InitiateJump();
                }
                else if (controls.JumpRelease())
                {
                    gameWorld.CutJump();
                }

                // Handle state specific controls
                if (controls.DischargeTrigger() && tutorialMessages.Count > 0 && tutorialMessages[0].GetId() == EndTutorialDischargeMessageId)
                {
                    gameWorld.InitiateDischarge();
                }
                else if (controls.ShootTrigger() && tutorialMessages.Count > 0 && tutorialMessages[0].GetId() == EndTutorialShootMessageId)
                {
                    gameWorld.InitiateShoot();
                }
                else if (controls.OverchargeTrigger() && tutorialMessages.Count > 0 && tutorialMessages[0].GetId() == EndTutorialOverchargeMessageId)
                {
                    gameWorld.InitiateOvercharge();
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
            using (Stream settingsFileStream = FileSystemManager.GetFileStream(GameplayVars.UserSettingsFile, FileMode.Create))
            {
                settings.Save(settingsFileStream);
            }
        }

        private void LoadUserSettings()
        {
            // Load user volume settings
            if (FileSystemManager.FileExists(GameplayVars.UserSettingsFile))
            {
                using (Stream settingsFileStream = FileSystemManager.GetFileStream(GameplayVars.UserSettingsFile, System.IO.FileMode.Open))
                {
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
                }
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

        /// <summary>
        /// Initializes tutorial messages for the explainatory part of the tutorial
        /// </summary>
        private void LoadTutorialExplainMessages()
        {
            // Explain the charge bar
            String message1 = "This is the Charge Bar";
            String message2 = "The more charge you have, the faster you will go.";
            String message3 = "You lose charge over time, causing you to go slower.";
            String message4 = "Collect charge orbs to increase your charge and go faster!";

            // Calculate the position for each message. They will all share the same y-value, but will have different x-values depending on message length
            int messageTop = GameplayVars.WinHeight / 4;

            int message1Left = GetCenteredStringLocation(FontSmall, message1, GameplayVars.WinWidth / 2);
            int message2Left = GetCenteredStringLocation(FontSmall, message2, GameplayVars.WinWidth / 2);
            int message3Left = GetCenteredStringLocation(FontSmall, message3, GameplayVars.WinWidth / 2);
            int message4Left = GetCenteredStringLocation(FontSmall, message4, GameplayVars.WinWidth / 2);
            
            int chargeBarTarget = chargeBar.position.Left + chargeBar.position.Width / 4; // The position on the charge bar that the arrow will point to

            // Calculate arrow rotation. 
            double distToChargeBar = Math.Sqrt(Math.Pow(message1Left - chargeBarTarget, 2) + Math.Pow(messageTop - chargeBar.position.Bottom, 2));
            float rotation = (float)Math.Asin((chargeBarTarget - message1Left) / distToChargeBar);

            int arrowPositionLeft = (int)(message1Left + chargeBarTarget) / 2 - 10;
            int arrowPositionTop = (int)(messageTop + chargeBar.position.Bottom) / 2;
            int arrowPositionWidth = (int)(distToChargeBar / 4);
            int arrowPositionHeight = (int)distToChargeBar;

            Rectangle arrowPosition = new Rectangle(arrowPositionLeft, arrowPositionTop, arrowPositionWidth, arrowPositionHeight);

            TutorialMessage tm1 = new TutorialMessage(message1, true, new Vector2(message1Left, messageTop), Color.White, arrowPosition, Arrow, rotation, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm2 = new TutorialMessage(message2, true, new Vector2(message2Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm3 = new TutorialMessage(message3, true, new Vector2(message3Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm4 = new TutorialMessage(message4, true, new Vector2(message4Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);

            tutorialMessages.Add(tm1);
            tutorialMessages.Add(tm2);
            tutorialMessages.Add(tm3);
            tutorialMessages.Add(tm4);
        }

        private void LoadTutorialJumpMessages()
        {
            // Explain the jump mechanic
            String message1 = "While playing Charge, there will be obstacles to avoid";
            String message2 = "and charge orbs to sometimes collect, and sometimes dodge.";
            String message3 = "You will need to jump and double jump to survive.";
            String message4 = controls.GetJumpString() + " to jump, and again to double jump.";
            String message5 = "Holding the jump longer will cause you to go higher,";
            String message6 = "while releasing it early will cause a shorter jump.";
            String message7 = "Perform a double jump now (" + controls.GetJumpString() + ")";

            // Calculate the position for each message. They will all share the same y-value, but will have different x-values depending on message length
            int messageTop = GameplayVars.WinHeight / 4;
            int message1Left = GetCenteredStringLocation(FontSmall, message1, GameplayVars.WinWidth / 2);
            int message2Left = GetCenteredStringLocation(FontSmall, message2, GameplayVars.WinWidth / 2);
            int message3Left = GetCenteredStringLocation(FontSmall, message3, GameplayVars.WinWidth / 2);
            int message4Left = GetCenteredStringLocation(FontSmall, message4, GameplayVars.WinWidth / 2);
            int message5Left = GetCenteredStringLocation(FontSmall, message5, GameplayVars.WinWidth / 2);
            int message6Left = GetCenteredStringLocation(FontSmall, message6, GameplayVars.WinWidth / 2);
            int message7Left = GetCenteredStringLocation(FontSmall, message7, GameplayVars.WinWidth / 2);

            // Create the tutorial messages
            TutorialMessage tm1 = new TutorialMessage(message1, true, new Vector2(message1Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm2 = new TutorialMessage(message2, true, new Vector2(message2Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm3 = new TutorialMessage(message3, true, new Vector2(message3Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm4 = new TutorialMessage(message4, true, new Vector2(message4Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm5 = new TutorialMessage(message5, true, new Vector2(message5Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm6 = new TutorialMessage(message6, true, new Vector2(message6Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm7 = new TutorialMessage(message7, false, new Vector2(message7Left, messageTop), Color.White, null, null, 0, 0);

            // Set the id of the final jump tutorial message
            EndTutorialJumpMessageId = tm7.GetId();

            // Add the tutorial messages to the message queue
            tutorialMessages.Add(tm1);
            tutorialMessages.Add(tm2);
            tutorialMessages.Add(tm3);
            tutorialMessages.Add(tm4);
            tutorialMessages.Add(tm5);
            tutorialMessages.Add(tm6);
            tutorialMessages.Add(tm7);
        }

        private void LoadTutorialDischargeMessages()
        {
            // Explain Discharge
            String message1 = "This is a death barrier.";
            String message2 = "There is one in front of you, and one behind you at all times.";
            String message3 = "If you go too fast, you will run into the barrier.";
            String message4 = "If you go too slow, the barrier behind you will hit you.";
            String message5 = "If you find yourself going too fast,\n you can avoid picking up charge orbs to slow down.";
            String message6 = "In emergencies, you can use the Discharge Ability.";
            String message7 = "Discharge will blast enemies away and slow you down immediately.";
            String message8 = "Be warned that using any ability will block you\n from using any other ability for a short time.";
            String message9 = controls.GetDischargeString() + " to use Discharge now";

            // Calculate the position for each message. They will all share the same y-value, but will have different x-values depending on message length
            int messageTop = GameplayVars.WinHeight / 4;
            int message1Left = GetCenteredStringLocation(FontSmall, message1, GameplayVars.WinWidth / 2);
            int message2Left = GetCenteredStringLocation(FontSmall, message2, GameplayVars.WinWidth / 2);
            int message3Left = GetCenteredStringLocation(FontSmall, message3, GameplayVars.WinWidth / 2);
            int message4Left = GetCenteredStringLocation(FontSmall, message4, GameplayVars.WinWidth / 2);
            int message5Left = GetCenteredStringLocation(FontSmall, message5, GameplayVars.WinWidth / 2);
            int message6Left = GetCenteredStringLocation(FontSmall, message6, GameplayVars.WinWidth / 2);
            int message7Left = GetCenteredStringLocation(FontSmall, message7, GameplayVars.WinWidth / 2);
            int message8Left = GetCenteredStringLocation(FontSmall, message8, GameplayVars.WinWidth / 2);
            int message9Left = GetCenteredStringLocation(FontSmall, message9, GameplayVars.WinWidth / 2);

            // Create the tutorial messages
            TutorialMessage tm1 = new TutorialMessage(message1, true, new Vector2(message1Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm2 = new TutorialMessage(message2, true, new Vector2(message2Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm3 = new TutorialMessage(message3, true, new Vector2(message3Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm4 = new TutorialMessage(message4, true, new Vector2(message4Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm5 = new TutorialMessage(message5, true, new Vector2(message5Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm6 = new TutorialMessage(message6, true, new Vector2(message6Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm7 = new TutorialMessage(message7, true, new Vector2(message7Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm8 = new TutorialMessage(message8, true, new Vector2(message8Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm9 = new TutorialMessage(message9, false, new Vector2(message9Left, messageTop), Color.White, null, null, 0, 0);

            // Set the id of the final discharge tutorial message
            EndTutorialDischargeMessageId = tm9.GetId();

            // Add the tutorial messages to the message queue
            tutorialMessages.Add(tm1);
            tutorialMessages.Add(tm2);
            tutorialMessages.Add(tm3);
            tutorialMessages.Add(tm4);
            tutorialMessages.Add(tm5);
            tutorialMessages.Add(tm6);
            tutorialMessages.Add(tm7);
            tutorialMessages.Add(tm8);
            tutorialMessages.Add(tm9);
        }

        private void LoadTutorialShootMessages()
        {
            // Explain Shoot
            String message1 = "If you are going just a little too fast,\n you can use Shoot to slow down.";
            String message2 = "Shoot won't slow you down as much as Discharge,\n but has a much shorter cooldown time.";
            String message3 = controls.GetShootString() + " to use Shoot now";

            // Calculate the position for each message. They will all share the same y-value, but will have different x-values depending on message length
            int messageTop = GameplayVars.WinHeight / 4;
            int message1Left = GetCenteredStringLocation(FontSmall, message1, GameplayVars.WinWidth / 2);
            int message2Left = GetCenteredStringLocation(FontSmall, message2, GameplayVars.WinWidth / 2);
            int message3Left = GetCenteredStringLocation(FontSmall, message3, GameplayVars.WinWidth / 2);

            // Create the tutorial messages
            TutorialMessage tm1 = new TutorialMessage(message1, true, new Vector2(message1Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm2 = new TutorialMessage(message2, true, new Vector2(message2Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm3 = new TutorialMessage(message3, false, new Vector2(message3Left, messageTop), Color.White, null, null, 0, 0);

            // Set the id of the final shoot tutorial message
            EndTutorialShootMessageId = tm3.GetId();

            // Add the tutorial messages to the message queue
            tutorialMessages.Add(tm1);
            tutorialMessages.Add(tm2);
            tutorialMessages.Add(tm3);
        }

        private void LoadTutorialOverchargeMessages()
        {
            // Explain Overcharge
            String message1 = "When you are going too slow you should \npick up charge orbs to increase your speed.";
            String message2 = "In emergencies, you can use Overcharge to \ntemporarily boost your speed and break through walls.";
            String message3 = "Like Discharge, Overcharge comes with a cooldown cost.";
            String message4 = "You will not be able to use other \nabilities for a short time after using Overcharge.";
            String message8 = controls.GetOverchargeString() + " to use Overcharge now";

            // Calculate the position for each message. They will all share the same y-value, but will have different x-values depending on message length
            int messageTop = GameplayVars.WinHeight / 4;
            int message1Left = GetCenteredStringLocation(FontSmall, message1, GameplayVars.WinWidth / 2);
            int message2Left = GetCenteredStringLocation(FontSmall, message2, GameplayVars.WinWidth / 2);
            int message3Left = GetCenteredStringLocation(FontSmall, message3, GameplayVars.WinWidth / 2);
            int message4Left = GetCenteredStringLocation(FontSmall, message4, GameplayVars.WinWidth / 2);
            int message8Left = GetCenteredStringLocation(FontSmall, message8, GameplayVars.WinWidth / 2);

            // Create the tutorial messages
            TutorialMessage tm1 = new TutorialMessage(message1, true, new Vector2(message1Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm2 = new TutorialMessage(message2, true, new Vector2(message2Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm3 = new TutorialMessage(message3, true, new Vector2(message3Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm4 = new TutorialMessage(message4, true, new Vector2(message4Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm8 = new TutorialMessage(message8, false, new Vector2(message8Left, messageTop), Color.White, null, null, 0, 0);

            // Set the id of the final overcharge tutorial message
            EndTutorialOverchargeMessageId = tm8.GetId();

            // Add the tutorial messages to the message queue
            tutorialMessages.Add(tm1);
            tutorialMessages.Add(tm2);
            tutorialMessages.Add(tm3);
            tutorialMessages.Add(tm4);
            tutorialMessages.Add(tm8);
        }

        private void LoadTutorialDoneMessages()
        {
            // Explain Discharge
            String message1 = "Well done!";
            String message2 = "You've completed the tutorial.";
            String message3 = "Now watch out for those barriers...";

            // Calculate the position for each message. They will all share the same y-value, but will have different x-values depending on message length
            int messageTop = GameplayVars.WinHeight / 4;
            int message1Left = GetCenteredStringLocation(FontSmall, message1, GameplayVars.WinWidth / 2);
            int message2Left = GetCenteredStringLocation(FontSmall, message2, GameplayVars.WinWidth / 2);
            int message3Left = GetCenteredStringLocation(FontSmall, message3, GameplayVars.WinWidth / 2);

            // Create the tutorial messages
            TutorialMessage tm1 = new TutorialMessage(message1, true, new Vector2(message1Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm2 = new TutorialMessage(message2, true, new Vector2(message2Left, messageTop), Color.White, null, null, 0, GameplayVars.DefaultTutorialMessageTimeout);
            TutorialMessage tm3 = new TutorialMessage(message3, false, new Vector2(message3Left, messageTop), Color.Black, new Rectangle(GameplayVars.WinWidth / 2, GameplayVars.WinHeight / 2, GameplayVars.WinWidth, GameplayVars.WinHeight), WhiteTex, 0, 0);

            // Add the tutorial messages to the message queue
            tutorialMessages.Add(tm1);
            tutorialMessages.Add(tm2);
            tutorialMessages.Add(tm3);
        }

        /// <summary>
        /// Updates the first tutorial message, which should currently be showing. If that tutorial message has expired, then remove it.
        /// </summary>
        /// <param name="deltaTime">How much time, in seconds, has passed since the last update.</param>
        private void UpdateTutorialMessages(float deltaTime)
        {
            // Only update the first message
            if (tutorialMessages.Count > 0)
            {
                TutorialMessage message = tutorialMessages[0];
                message.Update(deltaTime);
                
                if (message.ShouldRemove())
                {
                    tutorialMessages.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Draws only the first tutorial message in the list.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to draw to</param>
        private void DrawTutorialMessages(SpriteBatch spriteBatch)
        {
            if (tutorialMessages.Count > 0)
            {
                tutorialMessages[0].Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Returns true if the given game state is a tutorial state.
        /// </summary>
        /// <param name="gameState">The game state to check</param>
        /// <returns></returns>
        private bool IsTutorialState(GameState gameState)
        {
            return gameState == GameState.TutorialJump || gameState == GameState.TutorialDischarge || gameState == GameState.TutorialOvercharge || gameState == GameState.TutorialShoot;
        }

        /// <summary>
        /// Switches the current game state from the tutorial to the in game state
        /// </summary>
        private void SwitchFromTutorialToInGame()
        {
            tutorialMessages.Clear();

            currentGameState = GameState.InGame;
            gameWorld.InitializeStateSpecificVariables(currentGameState);
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        public void StartGame()
        {
            if (showTutorial)
            {
                currentGameState = GameState.TutorialJump;
                LoadTutorialExplainMessages();
                LoadTutorialJumpMessages();

                showTutorial = false;
                SaveUserSettings();
            }
            else
            {
                currentGameState = GameState.InGame;
            }

            gameWorld.InitializeStateSpecificVariables(currentGameState);

            // Stop any currently playing song, and play the in-game background music
            MediaPlayer.Stop();
            MediaPlayer.Play(ChargeMain.Background1);
            MediaPlayer.IsRepeating = true;
        }

        internal void TitleToOptionsScreen()
        {
            currentGameState = GameState.OptionsScreen;
        }

        internal void TitleToCreditsScene()
        {
            currentGameState = GameState.CreditsScreen;
        }

        internal void OptionsToTitleScreen()
        {
            currentGameState = GameState.TitleScreen;
            SaveUserSettings();
        }

        internal void ClearHighScores()
        {
            highScoreManager.ClearHighScores();
        }

        internal void LaunchTutorial()
        {
            currentGameState = GameState.TutorialJump;
            gameWorld.InitializeStateSpecificVariables(currentGameState);

            LoadTutorialExplainMessages();
            LoadTutorialJumpMessages();
        }

        internal void AdjustMasterVolume(float amt)
        {
            masterVolume += amt;

            if (masterVolume < 0)
            {
                masterVolume = 0;
            }

            if (masterVolume > 1)
            {
                masterVolume = 1;
            }

            MediaPlayer.Volume = masterVolume;
            gameWorld.SetMasterVolume(masterVolume);
        }
    }
}
