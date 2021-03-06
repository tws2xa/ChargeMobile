#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
#endregion


namespace Charge
{
    class MainMenuManager
    {
        enum OptionSelection
        {
            Volume,
            ClearHighScores,
            Tutorial,
            Back
        };
		
        SpriteFont FontLarge;
        SpriteFont FontSmall;

        Texture2D WhiteTex;

        private String ClearHighScoresText;

        // UI Buttons
        Button skipTutorialButton;
        Button beginButton;
        Button optionsButton;
        Button creditsButton;

        Button volumeUp;
        Button volumeDown;
        Button tutorial;
        Button clearHighScores;
        Button optionsBackToTitle;

		Button playAgain;
		Button gameOverReturnToTitle;

		Button creditsReturnToTitle;

        //Title Menu Button Properties
        SpriteFont titleButtonFont;
        Color titleButtonTextColor;
        Color titleButtonBackColor;
        int titleButtonWidth;
        int titleButtonHeight;
        int titleButtonBorderSize;

        //Optins Men Properties
        String Volume = "Master Volume: ";
        int VolumeTextDrawX;
        int VolumeTextDrawY;
        int volumeBarLeft;
        int maxVolumeBarWidth;
        int volBarYPos;
        int volBarHeight;
        bool highScoresCleared = false;

		private ButtonState lastMouseState; // Needed to avoid registering the same click twice

		public MainMenuManager(Texture2D WhiteTex, SpriteFont FontLarge, SpriteFont FontSmall)
        {
            this.WhiteTex = WhiteTex;
            this.FontLarge = FontLarge;
            this.FontSmall = FontSmall;

            titleButtonFont = FontSmall;
            titleButtonTextColor = new Color(0, 234, 6);
            titleButtonBackColor = new Color(50, 50, 50);
            titleButtonWidth = GameplayVars.WinWidth / 5;
            titleButtonHeight = GameplayVars.WinHeight / 10;
            titleButtonBorderSize = titleButtonWidth / 50; //Tiny border!

			lastMouseState = ButtonState.Released;

            ClearHighScoresText = GameplayVars.DefaultClearHighScoresText;

            skipTutorialButton = new Button("Skip", new Rectangle(GameplayVars.WinWidth - 210, GameplayVars.WinHeight - 110, 200, 100), Color.Black, FontSmall, WhiteTex, Color.WhiteSmoke);
            
            InitTitleButtons();
            InitOptionsButtons();
			InitGameOverButtons();
			InitCreditsButtons();
        }


        public void InitTitleButtons()
        {
            // Button layout 1
            // [options]  [             ]
            //            |    begin    |
            // [credits]  [             ]

            int vSpacer = titleButtonHeight;
            int hSpacer = titleButtonWidth * 1 / 4;

            int centerPos = GameplayVars.WinWidth / 2;
            int beginX = centerPos + hSpacer / 2;
            int optionsAndCreditsX = centerPos - hSpacer / 2 - titleButtonWidth;
            int yPos = 5 * GameplayVars.WinHeight / 12; //Between 1/2 and 1/3
            beginButton = new Button("Play  >", new Rectangle(beginX, yPos, titleButtonWidth, titleButtonHeight * 2 + vSpacer), titleButtonTextColor,
                titleButtonFont, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
            optionsButton = new Button("Options", new Rectangle(optionsAndCreditsX, yPos, titleButtonWidth, titleButtonHeight), titleButtonTextColor,
                titleButtonFont, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
            yPos += titleButtonHeight + vSpacer;
            creditsButton = new Button("Credits", new Rectangle(optionsAndCreditsX, yPos, titleButtonWidth, titleButtonHeight), titleButtonTextColor,
                titleButtonFont, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);

            /*
            // Button layout 2
            // [  Begin  ]
            // [ Options ]
            // [ Credits ]
            int xPos = GameplayVars.WinWidth / 2 - titleButtonWidth / 2;
            int yPos = GameplayVars.WinHeight / 3;
            beginButton = new Button("Play", new Rectangle(xPos, yPos, titleButtonWidth, titleButtonHeight), titleButtonTextColor,
                titleButtonFont, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
            yPos += titleButtonHeight * 3 / 2;
            optionsButton = new Button("Options", new Rectangle(xPos, yPos, titleButtonWidth, titleButtonHeight), titleButtonTextColor,
                titleButtonFont, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
            yPos += titleButtonHeight * 3 / 2;
            creditsButton = new Button("Credits", new Rectangle(xPos, yPos, titleButtonWidth, titleButtonHeight), titleButtonTextColor,
                titleButtonFont, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
            */
        }

        public void InitOptionsButtons()
        {
            //Layout:
            //        Master Volume     
            // [-] [----------       ] [+]
            //
            //    [      Tutorial     ]
            //    [ Clear High Scores ]
            //    [        Back       ]

            //Volume Bar Positioning
            float strHeight = FontSmall.MeasureString(Volume).Y;
            
            VolumeTextDrawX = ChargeMain.GetCenteredStringLocation(FontSmall, Volume, GameplayVars.WinWidth / 2);
            VolumeTextDrawY = Convert.ToInt32(Math.Ceiling(strHeight * 3));
            
            maxVolumeBarWidth = Convert.ToInt32(GameplayVars.WinWidth / 2) + 5;
            volBarHeight = Convert.ToInt32(strHeight);
            
            volumeBarLeft = Convert.ToInt32(Math.Round(GameplayVars.WinWidth / 2.0f - ((GameplayVars.WinWidth / 2.0f + 5) / 2.0f)));
            volBarYPos = VolumeTextDrawY + Convert.ToInt32(strHeight * 2);
            
            int volButtonSize = volBarHeight * 2;

            volumeDown = new Button("-", new Rectangle(volumeBarLeft- 3 * volButtonSize / 2, volBarYPos + volBarHeight/2 - volButtonSize/2, volButtonSize, volButtonSize),
                titleButtonTextColor, FontSmall, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
            volumeUp = new Button("+", new Rectangle(volumeBarLeft + maxVolumeBarWidth + volButtonSize / 2, volBarYPos + volBarHeight / 2 - volButtonSize / 2, volButtonSize, volButtonSize),
                titleButtonTextColor, FontSmall, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);

            int yPos = 5 * GameplayVars.WinHeight / 12;
            int btnWidth = GameplayVars.WinWidth / 3;
            int btnHeight = titleButtonHeight;
            int btnX = GameplayVars.WinWidth / 2 - btnWidth / 2;

            tutorial = new Button("Tutorial", new Rectangle(btnX, yPos, btnWidth, btnHeight), titleButtonTextColor, FontSmall, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
            yPos += btnHeight * 3 / 2;
            clearHighScores = new Button("Clear High Scores", new Rectangle(btnX, yPos, btnWidth, btnHeight), titleButtonTextColor, FontSmall, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
            yPos += btnHeight * 3 / 2;
            optionsBackToTitle = new Button("Back", new Rectangle(btnX, yPos, btnWidth, btnHeight), titleButtonTextColor, FontSmall, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
            yPos += btnHeight * 3 / 2;
        }

		public void InitGameOverButtons()
		{
			//Layout:
			//			New High Score
			//		1. [score #1]
			//		2. [score #2]
			//		...
			//		Final Score: [final score]
			//	[Play Again]		[Main Menu]

			int btnWidth = GameplayVars.WinWidth / 4;
			int btnHeight = titleButtonHeight;

			int btnY = 763;

			int playBtnX = 1 * GameplayVars.WinWidth / 8;
			int titleBtnX = 5 * GameplayVars.WinWidth / 8;

			playAgain = new Button("Play Again", new Rectangle(playBtnX, btnY, btnWidth, btnHeight), titleButtonTextColor, FontSmall, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
			gameOverReturnToTitle = new Button("Main Menu", new Rectangle(titleBtnX, btnY, btnWidth, btnHeight), titleButtonTextColor, FontSmall, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
		}

		public void InitCreditsButtons()
		{
			int btnX = GameplayVars.WinWidth / 3;
			int btnY = 924;

			int btnWidth = GameplayVars.WinWidth / 3;
			int btnHeight = titleButtonHeight;

			creditsReturnToTitle = new Button("Back", new Rectangle(btnX, btnY, btnWidth, btnHeight), titleButtonTextColor, FontSmall, WhiteTex, titleButtonBackColor, titleButtonBorderSize, titleButtonTextColor);
		}

        public void DrawTitleScreen(SpriteBatch spriteBatch)
        {
            //Draw Title Menu
            String Title = "CHARGE";
            int TitleDrawX = ChargeMain.GetCenteredStringLocation(FontLarge, Title, GameplayVars.WinWidth / 2);
            ChargeMain.DrawStringWithShadow(spriteBatch, Title, new Vector2(TitleDrawX, 100), Color.WhiteSmoke, Color.Black, FontLarge);

            DrawTitleMenuButtons(spriteBatch);
        }

        public void DrawOptionsScreen(SpriteBatch spriteBatch)
        {
            String Title = "Options";
            int TitleDrawX = ChargeMain.GetCenteredStringLocation(FontSmall, Title, GameplayVars.WinWidth / 2);
            spriteBatch.DrawString(FontSmall, Title, new Vector2(TitleDrawX, 25), Color.White);

            // Set the color for the selected and unselected menu items
            Color volumeColor = Color.White;
            
            spriteBatch.DrawString(FontSmall, Volume, new Vector2(VolumeTextDrawX, VolumeTextDrawY), volumeColor);

            // Draw the volume slider bar
            spriteBatch.Draw(WhiteTex, new Rectangle(volumeBarLeft, volBarYPos, maxVolumeBarWidth, volBarHeight), new Color(100, 100, 100));
            spriteBatch.Draw(WhiteTex, new Rectangle(volumeBarLeft, volBarYPos, Convert.ToInt32(ChargeMain.masterVolume * GameplayVars.WinWidth / 2) + 5, volBarHeight), volumeColor);

            DrawOptionsButtons(spriteBatch);
        }

        /// <summary>
        /// Draws the buttons for the title menu
        /// </summary>
        public void DrawTitleMenuButtons(SpriteBatch spriteBatch)
        {
            beginButton.Draw(spriteBatch);
            optionsButton.Draw(spriteBatch);
            creditsButton.Draw(spriteBatch);
        }

        /// <summary>
        /// Draw the buttons for the options menu
        /// </summary>
        public void DrawOptionsButtons(SpriteBatch spriteBatch)
        {
            volumeUp.Draw(spriteBatch);
            volumeDown.Draw(spriteBatch);
            tutorial.Draw(spriteBatch);
            clearHighScores.Draw(spriteBatch);
            optionsBackToTitle.Draw(spriteBatch);
        }

		public void DrawGameOverInput(SpriteBatch spriteBatch)
		{
			playAgain.Draw(spriteBatch);
			gameOverReturnToTitle.Draw(spriteBatch);
		}

		public void DrawCreditsInput(SpriteBatch spriteBatch)
		{
			creditsReturnToTitle.Draw(spriteBatch);
		}

		/// <summary>
		/// Draws the interface for the skip tutorial action. For desktop it will display a string indicating which keyboard key to push. On mobile it will draw a button that can be tapped to skip.
		/// </summary>
		/// <param name="spriteBatch"></param>
		public void DrawSkipTutorial(SpriteBatch spriteBatch)
        {
            skipTutorialButton.Draw(spriteBatch);
        }

        public void ProcessMainMenuInput(ChargeMain main, Controls controls)
        {
			if (controls.ClickedOrTapped(beginButton.GetButtonRegion(), lastMouseState))
            {
                main.StartGame();
            }
            else if (controls.ClickedOrTapped(optionsButton.GetButtonRegion(), lastMouseState))
            {
                main.TitleToOptionsScreen();
            }
            else if (controls.ClickedOrTapped(creditsButton.GetButtonRegion(), lastMouseState))
            {
                main.TitleToCreditsScene();
            }

			lastMouseState = Mouse.GetState().LeftButton;
        }

        internal void ProcessOptionsInput(ChargeMain main, Controls controls)
        {
            if (controls.ClickedOrTapped(optionsBackToTitle.GetButtonRegion(), lastMouseState))
            {
                //Reset the clear high scores button
                highScoresCleared = false;
                clearHighScores.SetText(GameplayVars.DefaultClearHighScoresText);
                main.OptionsToTitleScreen();
            }
            else if (!highScoresCleared && controls.ClickedOrTapped(clearHighScores.GetButtonRegion(), lastMouseState))
            {
                main.ClearHighScores();
                highScoresCleared = true;
                clearHighScores.SetText("High Scores Cleared");
            }
            else if (controls.ClickedOrTapped(tutorial.GetButtonRegion(), lastMouseState))
            {
                main.LaunchTutorial();
            }
            else if (controls.ClickedOrTapped(volumeDown.GetButtonRegion(), lastMouseState))
            {
                main.AdjustMasterVolume(-GameplayVars.VolumeChangeAmount);
            }
            else if (controls.ClickedOrTapped(volumeUp.GetButtonRegion(), lastMouseState))
            {
                main.AdjustMasterVolume(GameplayVars.VolumeChangeAmount);
            }

			lastMouseState = Mouse.GetState().LeftButton;
		}

        internal bool SkipTutorialTriggered(Controls controls)
        {
			bool skipTriggered = controls.ClickedOrTapped(skipTutorialButton.GetButtonRegion(), lastMouseState);
            lastMouseState = Mouse.GetState().LeftButton;

			return skipTriggered;
        }

		internal void ProcessGameOverInput(ChargeMain main, Controls controls)
		{
			if (controls.ClickedOrTapped(playAgain.GetButtonRegion(), lastMouseState))
			{
				main.StartGame();
			}
			else if (controls.ClickedOrTapped(gameOverReturnToTitle.GetButtonRegion(), lastMouseState))
			{
				main.GameOverToTitleScreen();
			}

			lastMouseState = Mouse.GetState().LeftButton;
		}

		internal void ProcessCreditsInput(ChargeMain main, Controls controls)
		{
			if (controls.ClickedOrTapped(creditsReturnToTitle.GetButtonRegion(), lastMouseState))
			{
				main.OptionsToTitleScreen();
			}

			lastMouseState = Mouse.GetState().LeftButton;
		}
    }
}