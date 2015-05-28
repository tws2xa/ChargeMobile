using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

///
/// This class is from Professor Sherriff's Monogame Platform demo.
///
namespace Charge
{
    class Controls
    {
        public KeyboardState kb;
        public KeyboardState kbo;
        public GamePadState gp;
        public GamePadState gpo;

        public Controls()
        {
            this.kb = Keyboard.GetState();
            this.kbo = Keyboard.GetState();
            this.gp = GamePad.GetState(PlayerIndex.One);
            this.gpo = GamePad.GetState(PlayerIndex.One);

        }

        /// <summary>
        /// Resets all controls to clear for a new game
        /// Mainly useful in Android version for clearing swipes, etc
        /// </summary>
        public void Reset()
        {
            //Nothing to reset
        }

        public void Update()
        {
            kbo = kb;
            gpo = gp;
            kb = Keyboard.GetState();
            this.gp = GamePad.GetState(PlayerIndex.One);
        }

        public bool isPressed(Keys key, Buttons button)
        {
            return kb.IsKeyDown(key) || gp.IsButtonDown(button);
        }

        public bool onPress(Keys key, Buttons button)
        {
            return (kb.IsKeyDown(key) && kbo.IsKeyUp(key)) ||
                (gp.IsButtonDown(button) && gpo.IsButtonUp(button));
        }

        public bool onRelease(Keys key, Buttons button)
        {
            return (kb.IsKeyUp(key) && kbo.IsKeyDown(key)) ||
                (gp.IsButtonUp(button) && gpo.IsButtonDown(button));
        }

        public bool isHeld(Keys key, Buttons button)
        {
            return (kb.IsKeyDown(key) && kbo.IsKeyDown(key)) ||
                (gp.IsButtonDown(button) && gpo.IsButtonDown(button));
        }





        /// <summary>
        /// Checks if the jump control has been triggered
        /// </summary>
        public bool JumpTrigger()
        {
            return onPress(Keys.Space, Buttons.A);
        }

        /// <summary>
        /// Checks if the jump control has been released
        /// </summary>
        public bool JumpRelease()
        {
            return onRelease(Keys.Space, Buttons.A);
        }

        /// <summary>
        /// Checks if the discharge control has been triggered
        /// </summary>
        public bool DischargeTrigger()
        {
            return (isPressed(Keys.A, Buttons.X) || isPressed(Keys.Left, Buttons.X));
        }

        /// <summary>
        /// Checks if the overcharge control has been triggered
        /// </summary>
        public bool OverchargeTrigger()
        {
            return (isPressed(Keys.D, Buttons.B) || isPressed(Keys.D, Buttons.B));
        }

        /// <summary>
        /// Checks if the shoot control has been triggered
        /// </summary>
        public bool ShootTrigger()
        {
            return (isPressed(Keys.S, Buttons.Y) || isPressed(Keys.S, Buttons.Y));
        }

        /// <summary>
        /// Checks if the pause control has been triggered
        /// </summary>
        public bool PauseTrigger()
        {
            return onPress(Keys.P, Buttons.Start);
        }

        /// <summary>
        /// Checks if the unpause control has been triggered
        /// </summary>
        public bool UnpauseTrigger()
        {
            return onPress(Keys.P, Buttons.Start);
        }

        /// <summary>
        /// Checks if the menu up control has been triggered
        /// </summary>
        public bool MenuUpTrigger()
        {
            return onPress(Keys.Up, Buttons.LeftThumbstickUp);
        }

        /// <summary>
        /// Checks if the menu down control has been triggered
        /// </summary>
        public bool MenuDownTrigger()
        {
            return onPress(Keys.Down, Buttons.LeftThumbstickDown);
        }

        /// <summary>
        /// Checks if the menu select control has been triggered
        /// </summary>
        public bool MenuSelectTrigger()
        {
            return (onPress(Keys.Space, Buttons.A) || onPress(Keys.Enter, Buttons.Start));
        }

        /// <summary>
        /// Checks if the restart control has been triggered
        /// </summary>
        public bool RestartTrigger()
        {
            return onPress(Keys.Enter, Buttons.Start);
        }

        /// <summary>
        /// Checks if the menu increase control has been triggered
        /// </summary>
        public bool MenuIncreaseTrigger()
        {
            return onPress(Keys.Right, Buttons.LeftThumbstickRight);
        }

        /// <summary>
        /// Checks if the menu decrease control has been triggered
        /// </summary>
        public bool MenuDecreaseTrigger()
        {
            return onPress(Keys.Left, Buttons.LeftThumbstickLeft);
        }

        /// <summary>
        /// Checks if the return to title screen control has been triggered
        /// </summary>
        /// <returns></returns>
        public bool TitleScreenTrigger()
        {
            return onPress(Keys.Back, Buttons.B);
        }

        /// <summary>
        /// Checks if the tutorial skip control has been triggered
        /// </summary>
        /// <returns></returns>
        public bool TutorialSkipTrigger()
        {
            return onPress(Keys.Escape, Buttons.Start);
        }

        /// <summary>
        /// String explaining the control to start a new game.
        /// </summary>
        public string GetRestartString()
        {
            return "Press [Enter]";
        }

        /// <summary>
        /// String explaining the control to return to title.
        /// </summary>
        public string GetReturnToTitleString()
        {
            return "Press [Backspace]";
        }

        /// <summary>
        /// String explaining the control to unpause the game.
        /// </summary>
        public string GetUnpauseText()
        {
            return "Press [P]";
        }

        public string GetJumpString()
        {
            return "Press [Space]";
        }

        public string GetDischargeString()
        {
            return "Press [A]";
        }

        public string GetShootString()
        {
            return "Press [S]";
        }

        public string GetOverchargeString()
        {
            return "Press [D]";
        }
    }
}
