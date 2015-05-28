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
            Tutorial,
            Back
        };

        private TitleSelection currentTitleSelection;
        private OptionSelection currentOptionSelection;

        SpriteFont FontLarge;
        SpriteFont FontSmall;

        Texture2D WhiteTex;

        private String ClearHighScoresText;

        public MainMenuManager(Texture2D WhiteTex, SpriteFont FontLarge, SpriteFont FontSmall)
        {
            this.WhiteTex = WhiteTex;
            this.FontLarge = FontLarge;
            this.FontSmall = FontSmall;

            currentTitleSelection = TitleSelection.Start;
            currentOptionSelection = OptionSelection.Volume;

            ClearHighScoresText = GameplayVars.DefaultClearHighScoresText;
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

            if (currentTitleSelection == TitleSelection.Start)
            {
                ChargeMain.DrawStringWithShadow(spriteBatch, Start, new Vector2(StartDrawX, 250), Color.Gold, Color.Black);
                ChargeMain.DrawStringWithShadow(spriteBatch, Options, new Vector2(OptionsDrawX, 325));
                ChargeMain.DrawStringWithShadow(spriteBatch, Credits, new Vector2(CreditsDrawX, 400));
            }
            else if (currentTitleSelection == TitleSelection.Options)
            {
                ChargeMain.DrawStringWithShadow(spriteBatch, Start, new Vector2(StartDrawX, 250));
                ChargeMain.DrawStringWithShadow(spriteBatch, Options, new Vector2(OptionsDrawX, 325), Color.Gold, Color.Black);
                ChargeMain.DrawStringWithShadow(spriteBatch, Credits, new Vector2(CreditsDrawX, 400));
            }
            else if (currentTitleSelection == TitleSelection.Credits)
            {
                ChargeMain.DrawStringWithShadow(spriteBatch, Start, new Vector2(StartDrawX, 250));
                ChargeMain.DrawStringWithShadow(spriteBatch, Options, new Vector2(OptionsDrawX, 325));
                ChargeMain.DrawStringWithShadow(spriteBatch, Credits, new Vector2(CreditsDrawX, 400), Color.Gold, Color.Black);
            }
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
        /// Draws the interface for the skip tutorial action. For desktop it will display a string indicating which keyboard key to push. On mobile it will draw a button that can be tapped to skip.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void DrawSkipTutorial(SpriteBatch spriteBatch)
        {
            String skipText = "Press [Escape] to skip";

            // Put the text in the bottom right corner of the screen
            Vector2 textSize = FontSmall.MeasureString(skipText);
            float textX = GameplayVars.WinWidth - textSize.X;
            float textY = GameplayVars.WinHeight - textSize.Y;
            Vector2 textPosition = new Vector2(textX, textY);

            spriteBatch.DrawString(FontSmall, skipText, textPosition, Color.WhiteSmoke);
        }

        public void ProcessMainMenuInput(ChargeMain main, Controls controls)
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
                    main.StartGame();
                }
                else if (currentTitleSelection == TitleSelection.Options)
                {
                    main.TitleToOptionsScreen();
                }
                else if (currentTitleSelection == TitleSelection.Credits)
                {
                    main.TitleToCreditsScene();
                }
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
            return controls.TutorialSkipTrigger();
        }
    }
}