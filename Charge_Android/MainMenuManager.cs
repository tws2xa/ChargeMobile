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
    class MainMenuManager
    {
        enum OptionSelection
        {
            Volume,
            ClearHighScores,
            Tutorial,
            Back
        };

        private OptionSelection currentOptionSelection;

        SpriteFont FontLarge;
        SpriteFont FontSmall;

        Texture2D WhiteTex;

        private String ClearHighScoresText;

        // UI Buttons
        Button skipTutorialButton;
        Button beginButton;
        Button optionsButton;
        Button creditsButton;

        //Title Menu Button Properties
        SpriteFont titleButtonFont;
        Color titleButtonTextColor;
        Color titleButtonBackColor;
        int titleButtonWidth;
        int titleButtonHeight;
        int titleButtonBorderSize;

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

            currentOptionSelection = OptionSelection.Volume;

            ClearHighScoresText = GameplayVars.DefaultClearHighScoresText;

            skipTutorialButton = new Button("Skip", new Rectangle(GameplayVars.WinWidth - 210, GameplayVars.WinHeight - 110, 200, 100), Color.Black, FontSmall, WhiteTex, Color.WhiteSmoke);

            // Button layout 1
            // [options]  [             ]
            //            |    begin    |
            // [credits]  [             ]

            int vSpacer = titleButtonHeight;
            int hSpacer = titleButtonWidth * 1 / 4;

            int centerPos = GameplayVars.WinWidth / 2;
            int beginX = centerPos + hSpacer/ 2;
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

        public void DrawTitleScreen(SpriteBatch spriteBatch)
        {
            //Draw Title Menu
            String Title = "CHARGE";
            String Options = "Options";
            String Start = "Start Game";
            String Credits = "Credits";
            int TitleDrawX = ChargeMain.GetCenteredStringLocation(FontLarge, Title, GameplayVars.WinWidth / 2);
            int OptionsDrawX = ChargeMain.GetCenteredStringLocation(FontSmall, Options, GameplayVars.WinWidth / 2);
            int CreditsDrawX = ChargeMain.GetCenteredStringLocation(FontSmall, Credits, GameplayVars.WinWidth / 2);
            int StartDrawX = ChargeMain.GetCenteredStringLocation(FontSmall, Start, GameplayVars.WinWidth / 2);
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
            Color backColor = Color.White;
            Color clearColor = Color.White;
            Color tutorialColor = Color.White;

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
            else if (currentOptionSelection == OptionSelection.Tutorial)
            {
                tutorialColor = Color.Gold;
            }

            String Volume = "Master Volume: ";
            int VolumeDrawX = ChargeMain.GetCenteredStringLocation(FontSmall, Volume, GameplayVars.WinWidth / 4);
            spriteBatch.DrawString(FontSmall, Volume, new Vector2(VolumeDrawX, 150), volumeColor);

            // Draw the volume slider bar
            int volumeBarLeft = Convert.ToInt32(Math.Round(3 * GameplayVars.WinWidth / 4.0f - ((GameplayVars.WinWidth / 3.0f + 5) / 2.0f)));
            int maxWidth = Convert.ToInt32(GameplayVars.WinWidth / 3) + 5;
            spriteBatch.Draw(WhiteTex, new Rectangle(volumeBarLeft, 162, maxWidth, 20), new Color(100, 100, 100));
            spriteBatch.Draw(WhiteTex, new Rectangle(volumeBarLeft, 162, Convert.ToInt32(ChargeMain.masterVolume * GameplayVars.WinWidth / 3) + 5, 20), volumeColor);

            int ClearDrawX = ChargeMain.GetCenteredStringLocation(FontSmall, ClearHighScoresText, GameplayVars.WinWidth / 2);
            spriteBatch.DrawString(FontSmall, ClearHighScoresText, new Vector2(ClearDrawX, 300), clearColor);

            String tutorial = "Play Tutorial";
            int tutorialDrawX = ChargeMain.GetCenteredStringLocation(FontSmall, tutorial, GameplayVars.WinWidth / 2);
            spriteBatch.DrawString(FontSmall, tutorial, new Vector2(tutorialDrawX, 350), tutorialColor);

            String Back = "Back";
            int BackDrawX = ChargeMain.GetCenteredStringLocation(FontSmall, Back, GameplayVars.WinWidth / 2);
            spriteBatch.DrawString(FontSmall, Back, new Vector2(BackDrawX, 400), backColor);
        }

        /// <summary>
        /// Draws the buttons for the title menu
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawTitleMenuButtons(SpriteBatch spriteBatch)
        {
            beginButton.Draw(spriteBatch);
            optionsButton.Draw(spriteBatch);
            creditsButton.Draw(spriteBatch);
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
            if (controls.TapRegionCheck(beginButton.GetButtonRegion()))
            {
                main.StartGame();
            }
            else if (controls.TapRegionCheck(optionsButton.GetButtonRegion()))
            {
                main.TitleToOptionsScreen();
            }
            else if (controls.TapRegionCheck(creditsButton.GetButtonRegion()))
            {
                main.TitleToCreditsScene();
            }
        }

        internal void ProcessOptionsInput(ChargeMain main, Controls controls)
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
                main.OptionsToTitleScreen();
            }
            else if (controls.MenuSelectTrigger() && currentOptionSelection == OptionSelection.ClearHighScores)
            {
                main.ClearHighScores();
                ClearHighScoresText = "High Scores Cleared";
                currentOptionSelection++;
            }
            else if (controls.MenuSelectTrigger() && currentOptionSelection == OptionSelection.Tutorial)
            {
                main.LaunchTutorial();
            }
            
            if (controls.MenuDecreaseTrigger() && currentOptionSelection == OptionSelection.Volume)
            {
                main.AdjustMasterVolume(-GameplayVars.VolumeChangeAmount);
            }

            if (controls.MenuIncreaseTrigger() && currentOptionSelection == OptionSelection.Volume)
            {
                main.AdjustMasterVolume(GameplayVars.VolumeChangeAmount);
            }
        }

        internal bool SkipTutorialTriggered(Controls controls)
        {
            return controls.TapRegionCheck(skipTutorialButton.GetButtonRegion());
        }
    }
}